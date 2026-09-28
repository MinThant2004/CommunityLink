using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CommunityLink.Api.Tests.Infrastructure;
using CommunityLink.Database.AppDbContextModels;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Authentication;
using CommunityLink.Shared.Features.ChatGroup;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CommunityLink.Api.Tests.Features;

public class ChatGroupMessageTests : IClassFixture<CommunityApiFactory>
{
    private readonly CommunityApiFactory _factory;
    private readonly HttpClient _client;

    public ChatGroupMessageTests(CommunityApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<(string Token, int UserId)> GetTokenAndUserIdForRoleAsync(string roleCode, string username, string email, string password)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var role = db.TblRoles.FirstOrDefault(r => r.RoleCode == roleCode);
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

        var user = db.TblUsers.FirstOrDefault(u => u.NormalizedEmail == email.ToUpperInvariant());
        if (user == null)
        {
            user = new TblUser
            {
                UserName = username,
                NormalizedUserName = username.ToUpperInvariant(),
                DisplayName = username,
                Email = email,
                NormalizedEmail = email.ToUpperInvariant(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password, 12),
                IsActive = true,
                IsVerified = true,
                CreatedAt = DateTime.UtcNow
            };
            db.TblUsers.Add(user);
            await db.SaveChangesAsync();

            db.TblUserRoles.Add(new TblUserRole { UserId = user.UserId, RoleId = role.RoleId, CreatedAt = DateTime.UtcNow });
            await db.SaveChangesAsync();
        }

        var login = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequestModel(email, password));
        var loginResult = await login.Content.ReadFromJsonAsync<Result<LoginResponseModel>>();
        Assert.NotNull(loginResult?.Data?.AccessToken);
        return (loginResult.Data.AccessToken, user.UserId);
    }

    [Fact]
    public async Task SendMessage_And_GetMessages_MemberFlow_Success()
    {
        // 1. Creator creates group
        var (creatorToken, _) = await GetTokenAndUserIdForRoleAsync("DOMAIN_PROFESSIONAL", "msg_creator1", "msg_creator1@test.com", "Password@123");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", creatorToken);

        var createResp = await _client.PostAsJsonAsync("/api/chat-groups", new CreateChatGroupRequestModel("Messaging Group", "Group for chat test", null, "FREE", 0));
        var createResult = await createResp.Content.ReadFromJsonAsync<Result<ChatGroupModel>>();
        Assert.NotNull(createResult?.Data);
        var groupId = createResult.Data.ChatGroupId;

        // 2. Member joins
        var (memberToken, memberId) = await GetTokenAndUserIdForRoleAsync("MEMBER", "msg_member1", "msg_member1@test.com", "Password@123");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", memberToken);
        await _client.PostAsJsonAsync($"/api/chat-groups/{groupId}/join", "");

        // 3. Member sends message
        var sendResp = await _client.PostAsJsonAsync($"/api/chat-groups/{groupId}/messages", new SendChatGroupMessageRequestModel("Hello everyone!"));
        Assert.Equal(HttpStatusCode.OK, sendResp.StatusCode);
        var sendResult = await sendResp.Content.ReadFromJsonAsync<Result<ChatGroupMessageModel>>();
        Assert.True(sendResult?.IsSuccess);
        Assert.NotNull(sendResult?.Data);
        Assert.Equal("Hello everyone!", sendResult.Data.Content);
        Assert.Equal(memberId, sendResult.Data.SenderId);
        Assert.True(sendResult.Data.IsMine);

        // 4. Get messages history
        var getResp = await _client.GetAsync($"/api/chat-groups/{groupId}/messages");
        Assert.Equal(HttpStatusCode.OK, getResp.StatusCode);
        var getResult = await getResp.Content.ReadFromJsonAsync<Result<IReadOnlyList<ChatGroupMessageModel>>>();
        Assert.True(getResult?.IsSuccess);
        Assert.NotNull(getResult?.Data);
        Assert.Single(getResult.Data);
        Assert.Equal("Hello everyone!", getResult.Data[0].Content);
    }

    [Fact]
    public async Task SendMessage_NonMember_ReturnsForbidden()
    {
        // 1. Creator creates group
        var (creatorToken, _) = await GetTokenAndUserIdForRoleAsync("DOMAIN_PROFESSIONAL", "msg_creator2", "msg_creator2@test.com", "Password@123");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", creatorToken);

        var createResp = await _client.PostAsJsonAsync("/api/chat-groups", new CreateChatGroupRequestModel("Private Group Test", "Testing non member send", null, "FREE", 0));
        var createResult = await createResp.Content.ReadFromJsonAsync<Result<ChatGroupModel>>();
        Assert.NotNull(createResult?.Data);
        var groupId = createResult.Data.ChatGroupId;

        // 2. Non-member attempts to send message
        var (nonMemberToken, _) = await GetTokenAndUserIdForRoleAsync("MEMBER", "non_member_user", "non_member_user@test.com", "Password@123");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", nonMemberToken);

        var sendResp = await _client.PostAsJsonAsync($"/api/chat-groups/{groupId}/messages", new SendChatGroupMessageRequestModel("Unauthorized message"));
        Assert.Equal(HttpStatusCode.Forbidden, sendResp.StatusCode);
    }

    [Fact]
    public async Task DeleteMessage_Sender_DeletesOwnMessage()
    {
        // 1. Creator creates group
        var (creatorToken, _) = await GetTokenAndUserIdForRoleAsync("DOMAIN_PROFESSIONAL", "msg_creator3", "msg_creator3@test.com", "Password@123");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", creatorToken);

        var createResp = await _client.PostAsJsonAsync("/api/chat-groups", new CreateChatGroupRequestModel("Delete Message Group", "Test delete", null, "FREE", 0));
        var createResult = await createResp.Content.ReadFromJsonAsync<Result<ChatGroupModel>>();
        Assert.NotNull(createResult?.Data);
        var groupId = createResult.Data.ChatGroupId;

        // 2. Member sends message
        var (memberToken, _) = await GetTokenAndUserIdForRoleAsync("MEMBER", "deleter_user", "deleter_user@test.com", "Password@123");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", memberToken);
        await _client.PostAsJsonAsync($"/api/chat-groups/{groupId}/join", "");

        var sendResp = await _client.PostAsJsonAsync($"/api/chat-groups/{groupId}/messages", new SendChatGroupMessageRequestModel("Message to delete"));
        var sendResult = await sendResp.Content.ReadFromJsonAsync<Result<ChatGroupMessageModel>>();
        Assert.NotNull(sendResult?.Data);
        var msgId = sendResult.Data.ChatGroupMessageId;

        // 3. Member deletes message
        var delResp = await _client.DeleteAsync($"/api/chat-groups/{groupId}/messages/{msgId}");
        Assert.Equal(HttpStatusCode.OK, delResp.StatusCode);

        // 4. Verify message no longer returned in GetMessages
        var getResp = await _client.GetAsync($"/api/chat-groups/{groupId}/messages");
        var getResult = await getResp.Content.ReadFromJsonAsync<Result<IReadOnlyList<ChatGroupMessageModel>>>();
        Assert.NotNull(getResult?.Data);
        Assert.Empty(getResult.Data);
    }
}
