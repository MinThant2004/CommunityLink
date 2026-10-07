using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using CommunityLink.Api.Tests.Infrastructure;
using CommunityLink.Database.AppDbContextModels;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Authentication;
using CommunityLink.Shared.Features.Chat;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CommunityLink.Api.Tests.Features;

public class DirectChatBlockAndDeleteTests : IClassFixture<CommunityApiFactory>
{
    private readonly CommunityApiFactory _factory;
    private readonly HttpClient _client;

    public DirectChatBlockAndDeleteTests(CommunityApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<(string Token, int UserId)> CreateTestUserAsync(string username, string email)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var role = await db.TblRoles.FirstOrDefaultAsync(r => r.RoleCode == "MEMBER");
        if (role == null)
        {
            role = new TblRole
            {
                RoleCode = "MEMBER",
                RoleName = "Member",
                Description = "Member role",
                IsSystemRole = false,
                CreatedAt = DateTime.UtcNow
            };
            db.TblRoles.Add(role);
            await db.SaveChangesAsync();
        }

        var user = await db.TblUsers.FirstOrDefaultAsync(u => u.NormalizedEmail == email.ToUpperInvariant());
        if (user == null)
        {
            user = new TblUser
            {
                UserName = username,
                NormalizedUserName = username.ToUpperInvariant(),
                Email = email,
                NormalizedEmail = email.ToUpperInvariant(),
                DisplayName = username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                IsActive = true,
                IsVerified = true,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };
            db.TblUsers.Add(user);
            await db.SaveChangesAsync();

            db.TblUserRoles.Add(new TblUserRole
            {
                UserId = user.UserId,
                RoleId = role.RoleId,
                CreatedAt = DateTime.UtcNow
            });

            var accessPerm = await db.TblPermissions.FirstOrDefaultAsync(p => p.PermissionCode == "CHAT_ACCESS");
            if (accessPerm != null)
            {
                db.TblRolePermissions.Add(new TblRolePermission { RoleId = role.RoleId, PermissionId = accessPerm.PermissionId });
            }
            var sendPerm = await db.TblPermissions.FirstOrDefaultAsync(p => p.PermissionCode == "CHAT_SEND");
            if (sendPerm != null)
            {
                db.TblRolePermissions.Add(new TblRolePermission { RoleId = role.RoleId, PermissionId = sendPerm.PermissionId });
            }

            await db.SaveChangesAsync();
        }

        var loginResp = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequestModel(email, "Password123!"));
        var result = await loginResp.Content.ReadFromJsonAsync<Result<LoginResponseModel>>();
        Assert.True(result?.IsSuccess, result?.Message);

        return (result.Data!.AccessToken, user.UserId);
    }

    [Fact]
    public async Task BlockUser_BlocksSending_And_UnblockUser_RestoresSending()
    {
        var (tokenA, userAId) = await CreateTestUserAsync("BlockerAlice", "blocker_alice@example.com");
        var (tokenB, userBId) = await CreateTestUserAsync("BlockedBob", "blocked_bob@example.com");

        // User A blocks User B
        using var requestABlock = new HttpRequestMessage(HttpMethod.Post, $"/api/chat/block/{userBId}");
        requestABlock.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);
        var blockResp = await _client.SendAsync(requestABlock);
        Assert.Equal(HttpStatusCode.OK, blockResp.StatusCode);

        // User A attempts to send message to User B -> should fail (validation error: blocked)
        using var requestASend = new HttpRequestMessage(HttpMethod.Post, "/api/chat/messages");
        requestASend.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);
        requestASend.Content = JsonContent.Create(new SendMessageRequestModel(null, userBId, "Hello Bob"));
        var sendFailResp = await _client.SendAsync(requestASend);
        Assert.False(sendFailResp.IsSuccessStatusCode);

        // User B attempts to send message to User A -> should fail (forbidden: blocked by recipient)
        using var requestBSend = new HttpRequestMessage(HttpMethod.Post, "/api/chat/messages");
        requestBSend.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenB);
        requestBSend.Content = JsonContent.Create(new SendMessageRequestModel(null, userAId, "Hello Alice"));
        var bSendFailResp = await _client.SendAsync(requestBSend);
        Assert.Equal(HttpStatusCode.Forbidden, bSendFailResp.StatusCode);

        // User A unblocks User B
        using var requestAUnblock = new HttpRequestMessage(HttpMethod.Delete, $"/api/chat/block/{userBId}");
        requestAUnblock.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);
        var unblockResp = await _client.SendAsync(requestAUnblock);
        Assert.Equal(HttpStatusCode.OK, unblockResp.StatusCode);

        // User A sends message to User B -> should succeed now
        using var requestASendSuccess = new HttpRequestMessage(HttpMethod.Post, "/api/chat/messages");
        requestASendSuccess.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);
        requestASendSuccess.Content = JsonContent.Create(new SendMessageRequestModel(null, userBId, "Hello Bob again"));
        var sendSuccessResp = await _client.SendAsync(requestASendSuccess);
        Assert.True(sendSuccessResp.IsSuccessStatusCode);
    }

    [Fact]
    public async Task DeleteConversationForSelf_HidesMessagesForCaller_KeepsForPeer()
    {
        var (token1, user1Id) = await CreateTestUserAsync("UserDel1", "userdel1@example.com");
        var (token2, user2Id) = await CreateTestUserAsync("UserDel2", "userdel2@example.com");

        // Send a message from User 1 to User 2
        using var sendReq = new HttpRequestMessage(HttpMethod.Post, "/api/chat/messages");
        sendReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token1);
        sendReq.Content = JsonContent.Create(new SendMessageRequestModel(null, user2Id, "Secret message"));
        var sendResp = await _client.SendAsync(sendReq);
        Assert.True(sendResp.IsSuccessStatusCode);
        var sendResult = await sendResp.Content.ReadFromJsonAsync<Result<ChatMessageModel>>();
        var convId = sendResult!.Data!.ConversationId;

        // User 1 deletes conversation for self
        using var delReq = new HttpRequestMessage(HttpMethod.Delete, $"/api/chat/conversations/{convId}");
        delReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token1);
        var delResp = await _client.SendAsync(delReq);
        Assert.Equal(HttpStatusCode.OK, delResp.StatusCode);

        // User 1 fetches messages -> empty list
        using var get1Req = new HttpRequestMessage(HttpMethod.Get, $"/api/chat/conversations/{convId}/messages");
        get1Req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token1);
        var get1Resp = await _client.SendAsync(get1Req);
        var msgList1 = await get1Resp.Content.ReadFromJsonAsync<Result<IReadOnlyList<ChatMessageModel>>>();
        Assert.True(msgList1?.IsSuccess);
        Assert.Empty(msgList1.Data!);

        // User 2 fetches messages -> still sees "Secret message"
        using var get2Req = new HttpRequestMessage(HttpMethod.Get, $"/api/chat/conversations/{convId}/messages");
        get2Req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token2);
        var get2Resp = await _client.SendAsync(get2Req);
        var msgList2 = await get2Resp.Content.ReadFromJsonAsync<Result<IReadOnlyList<ChatMessageModel>>>();
        Assert.True(msgList2?.IsSuccess);
        Assert.Single(msgList2.Data!);
        Assert.Equal("Secret message", msgList2.Data![0].MessageText);
    }
}
