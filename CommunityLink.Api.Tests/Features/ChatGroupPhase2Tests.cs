using System;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using CommunityLink.Api.Tests.Infrastructure;
using CommunityLink.Database.AppDbContextModels;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Authentication;
using CommunityLink.Shared.Features.ChatGroup;
using CommunityLink.Shared.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CommunityLink.Api.Tests.Features;

public class ChatGroupPhase2Tests : IClassFixture<CommunityApiFactory>
{
    private readonly CommunityApiFactory _factory;
    private readonly HttpClient _client;

    public ChatGroupPhase2Tests(CommunityApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public void FileSizeFormatter_FormatsByteSizesCorrectly()
    {
        Assert.Equal("0 B", FileSizeFormatter.FormatFileSize(null));
        Assert.Equal("0 B", FileSizeFormatter.FormatFileSize(0));
        Assert.Equal("512 B", FileSizeFormatter.FormatFileSize(512));
        Assert.Equal("1.5 KB", FileSizeFormatter.FormatFileSize(1536));
        Assert.Equal("12.4 MB", FileSizeFormatter.FormatFileSize(13002342));
        Assert.Equal("1.2 GB", FileSizeFormatter.FormatFileSize(1288490188));
    }

    private async Task<(string Token, int UserId)> GetUserTokenAsync(string username, string roleCode = "MEMBER")
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var email = $"{username.ToLowerInvariant()}@test.com";
        var user = await db.TblUsers.FirstOrDefaultAsync(u => u.UserName == username);
        if (user == null)
        {
            user = new TblUser
            {
                UserName = username,
                NormalizedUserName = username.ToUpperInvariant(),
                DisplayName = username,
                Email = email,
                NormalizedEmail = email.ToUpperInvariant(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("TestPassword123!", 12),
                IsActive = true,
                IsVerified = true,
                CreatedAt = DateTime.UtcNow
            };
            db.TblUsers.Add(user);
            await db.SaveChangesAsync();

            var role = await db.TblRoles.FirstOrDefaultAsync(r => r.RoleCode == roleCode);
            if (role == null)
            {
                role = new TblRole
                {
                    RoleCode = roleCode,
                    RoleName = roleCode,
                    Description = $"Role {roleCode}",
                    IsSystemRole = false,
                    CreatedAt = DateTime.UtcNow
                };
                db.TblRoles.Add(role);
                await db.SaveChangesAsync();
            }

            db.TblUserRoles.Add(new TblUserRole
            {
                UserId = user.UserId,
                RoleId = role.RoleId,
                CreatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }

        var loginRes = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequestModel(email, "TestPassword123!"));
        loginRes.EnsureSuccessStatusCode();

        var body = await loginRes.Content.ReadFromJsonAsync<Result<LoginResponseModel>>();
        Assert.NotNull(body?.Data?.AccessToken);

        return (body.Data.AccessToken, user.UserId);
    }

    [Fact]
    public async Task GroupMuteAndMemberManagement_WorkAsExpected()
    {
        var owner = await GetUserTokenAsync("GroupOwnerP2", "DOMAIN_PROFESSIONAL");
        var member1 = await GetUserTokenAsync("GroupMemberP2A");
        var member2 = await GetUserTokenAsync("GroupMemberP2B");

        // 1. Owner creates group
        var req = new HttpRequestMessage(HttpMethod.Post, "/api/chat-groups");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", owner.Token);
        req.Content = JsonContent.Create(new CreateChatGroupRequestModel("Phase 2 Test Group", "Test Desc", null));
        var createRes = await _client.SendAsync(req);
        createRes.EnsureSuccessStatusCode();

        var groupBody = await createRes.Content.ReadFromJsonAsync<Result<ChatGroupModel>>();
        Assert.NotNull(groupBody?.Data);
        var groupId = groupBody.Data.ChatGroupId;

        // 2. Member 1 joins group
        var join1 = new HttpRequestMessage(HttpMethod.Post, $"/api/chat-groups/{groupId}/join");
        join1.Headers.Authorization = new AuthenticationHeaderValue("Bearer", member1.Token);
        var join1Res = await _client.SendAsync(join1);
        join1Res.EnsureSuccessStatusCode();

        // 3. Member 2 joins group
        var join2 = new HttpRequestMessage(HttpMethod.Post, $"/api/chat-groups/{groupId}/join");
        join2.Headers.Authorization = new AuthenticationHeaderValue("Bearer", member2.Token);
        var join2Res = await _client.SendAsync(join2);
        join2Res.EnsureSuccessStatusCode();

        // 4. Member 1 toggles mute
        var muteReq = new HttpRequestMessage(HttpMethod.Post, $"/api/chat-groups/{groupId}/mute");
        muteReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", member1.Token);
        var muteRes = await _client.SendAsync(muteReq);
        muteRes.EnsureSuccessStatusCode();

        // 5. Owner sends image attachment message
        var msgReq = new HttpRequestMessage(HttpMethod.Post, $"/api/chat-groups/{groupId}/messages");
        msgReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", owner.Token);
        msgReq.Content = JsonContent.Create(new SendChatGroupMessageRequestModel(
            Content: "Check out this screenshot",
            MessageType: "IMAGE",
            AttachmentUrl: "https://example.com/image.png",
            FileName: "image.png",
            FileSizeByte: 1536000
        ));
        var msgRes = await _client.SendAsync(msgReq);
        msgRes.EnsureSuccessStatusCode();

        var msgBody = await msgRes.Content.ReadFromJsonAsync<Result<ChatGroupMessageModel>>();
        Assert.NotNull(msgBody?.Data);
        Assert.Equal("IMAGE", msgBody.Data.MessageType);
        Assert.Equal("https://example.com/image.png", msgBody.Data.AttachmentUrl);
        Assert.Equal("1.5 MB", msgBody.Data.FormattedFileSize);

        // Verify notifications: Member 2 (not muted) should receive notification, Member 1 (muted) should not.
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var m1NotifCount = await db.TblNotifications.CountAsync(n => n.RecipientUserId == member1.UserId && n.NotificationType == "GROUP_CHAT_MESSAGE");
            var m2NotifCount = await db.TblNotifications.CountAsync(n => n.RecipientUserId == member2.UserId && n.NotificationType == "GROUP_CHAT_MESSAGE");

            Assert.Equal(0, m1NotifCount);
            Assert.True(m2NotifCount >= 1);
        }

        // 6. Owner promotes Member 2 to Admin
        var promoteReq = new HttpRequestMessage(HttpMethod.Post, $"/api/chat-groups/{groupId}/members/{member2.UserId}/promote");
        promoteReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", owner.Token);
        var promoteRes = await _client.SendAsync(promoteReq);
        promoteRes.EnsureSuccessStatusCode();

        // 7. Admin (Member 2) removes Member 1
        var removeReq = new HttpRequestMessage(HttpMethod.Delete, $"/api/chat-groups/{groupId}/members/{member1.UserId}");
        removeReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", member2.Token);
        var removeRes = await _client.SendAsync(removeReq);
        removeRes.EnsureSuccessStatusCode();
    }
}
