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

public class ChatGroupMembershipTests : IClassFixture<CommunityApiFactory>
{
    private readonly CommunityApiFactory _factory;
    private readonly HttpClient _client;

    public ChatGroupMembershipTests(CommunityApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<string> GetTokenForRoleAsync(string roleCode, string username, string email, string password)
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
        return loginResult.Data.AccessToken;
    }

    [Fact]
    public async Task User_CanJoin_FreeChatGroup()
    {
        // 1. Creator creates FREE group
        var creatorToken = await GetTokenForRoleAsync("DOMAIN_PROFESSIONAL", "creator_free1", "creator_free1@test.com", "Password@123");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", creatorToken);

        var createResp = await _client.PostAsJsonAsync("/api/chat-groups", new CreateChatGroupRequestModel("Free Join Group", "Join for free", null, "FREE", 0));
        var createResult = await createResp.Content.ReadFromJsonAsync<Result<ChatGroupModel>>();
        Assert.NotNull(createResult?.Data);
        var groupId = createResult.Data.ChatGroupId;

        // 2. Member logs in and joins
        var memberToken = await GetTokenForRoleAsync("MEMBER", "joiner_user1", "joiner_user1@test.com", "Password@123");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", memberToken);

        var joinResp = await _client.PostAsJsonAsync($"/api/chat-groups/{groupId}/join", "");
        Assert.Equal(HttpStatusCode.OK, joinResp.StatusCode);
        var joinResult = await joinResp.Content.ReadFromJsonAsync<Result>();
        Assert.True(joinResult?.IsSuccess);

        // 3. Verify membership info
        var getResp = await _client.GetAsync($"/api/chat-groups/{groupId}");
        var getResult = await getResp.Content.ReadFromJsonAsync<Result<ChatGroupModel>>();
        Assert.NotNull(getResult?.Data);
        Assert.True(getResult.Data.IsJoined);
        Assert.Equal("MEMBER", getResult.Data.UserRole);
    }

    [Fact]
    public async Task User_CannotJoin_InactiveChatGroup()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var user = new TblUser
        {
            UserName = "inactive_creator",
            NormalizedUserName = "INACTIVE_CREATOR",
            Email = "inactive@test.com",
            NormalizedEmail = "INACTIVE@TEST.COM",
            DisplayName = "Inactive Creator",
            PasswordHash = "hash",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        db.TblUsers.Add(user);
        await db.SaveChangesAsync();

        var inactiveGroup = new TblChatGroup
        {
            Name = "Inactive Group",
            CreatorId = user.UserId,
            ChatType = "FREE",
            IsActive = false,
            CreatedAt = DateTime.UtcNow
        };
        db.TblChatGroups.Add(inactiveGroup);
        await db.SaveChangesAsync();

        var memberToken = await GetTokenForRoleAsync("MEMBER", "joiner_user2", "joiner_user2@test.com", "Password@123");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", memberToken);

        var joinResp = await _client.PostAsJsonAsync($"/api/chat-groups/{inactiveGroup.ChatGroupId}/join", "");
        Assert.Equal(HttpStatusCode.BadRequest, joinResp.StatusCode);
        var joinResult = await joinResp.Content.ReadFromJsonAsync<Result>();
        Assert.NotNull(joinResult);
        Assert.False(joinResult.IsSuccess);
        Assert.Contains("Chat Group is not active.", joinResult.Message);
    }

    [Fact]
    public async Task User_CannotJoin_PaidChatGroup_WithoutPayment()
    {
        var creatorToken = await GetTokenForRoleAsync("PUBLIC_FIGURE", "creator_paid1", "creator_paid1@test.com", "Password@123");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", creatorToken);

        var createResp = await _client.PostAsJsonAsync("/api/chat-groups", new CreateChatGroupRequestModel("Paid Group Test", "Requires LD", null, "PAID", 50));
        var createResult = await createResp.Content.ReadFromJsonAsync<Result<ChatGroupModel>>();
        Assert.NotNull(createResult?.Data);
        var groupId = createResult.Data.ChatGroupId;

        var memberToken = await GetTokenForRoleAsync("MEMBER", "joiner_user3", "joiner_user3@test.com", "Password@123");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", memberToken);

        var joinResp = await _client.PostAsJsonAsync($"/api/chat-groups/{groupId}/join", "");
        Assert.Equal(HttpStatusCode.BadRequest, joinResp.StatusCode);
        var joinResult = await joinResp.Content.ReadFromJsonAsync<Result>();
        Assert.NotNull(joinResult);
        Assert.False(joinResult.IsSuccess);
        Assert.Contains("Paid Chat Group requires Link Drop payment.", joinResult.Message);

        // Verify DB contains no membership record for this member
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var memberUser = await db.TblUsers.FirstAsync(u => u.Email == "joiner_user3@test.com");
        var hasMembership = await db.TblChatGroupMembers.AnyAsync(m => m.ChatGroupId == groupId && m.UserId == memberUser.UserId && !m.IsDeleted);
        Assert.False(hasMembership);
    }

    [Fact]
    public async Task DuplicateJoin_ReturnsError()
    {
        var creatorToken = await GetTokenForRoleAsync("DOMAIN_PROFESSIONAL", "creator_dup1", "creator_dup1@test.com", "Password@123");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", creatorToken);

        var createResp = await _client.PostAsJsonAsync("/api/chat-groups", new CreateChatGroupRequestModel("Dup Test Group", "Test dup join", null, "FREE", 0));
        var createResult = await createResp.Content.ReadFromJsonAsync<Result<ChatGroupModel>>();
        Assert.NotNull(createResult?.Data);
        var groupId = createResult.Data.ChatGroupId;

        var memberToken = await GetTokenForRoleAsync("MEMBER", "joiner_dup1", "joiner_dup1@test.com", "Password@123");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", memberToken);

        // 1st Join -> OK
        var join1 = await _client.PostAsJsonAsync($"/api/chat-groups/{groupId}/join", "");
        Assert.Equal(HttpStatusCode.OK, join1.StatusCode);

        // 2nd Join -> Conflict 409
        var join2 = await _client.PostAsJsonAsync($"/api/chat-groups/{groupId}/join", "");
        Assert.Equal(HttpStatusCode.Conflict, join2.StatusCode);
        var result2 = await join2.Content.ReadFromJsonAsync<Result>();
        Assert.NotNull(result2);
        Assert.False(result2.IsSuccess);
        Assert.Contains("Already joined this Chat Group.", result2.Message);
    }

    [Fact]
    public async Task Member_CanLeave_ChatGroup()
    {
        var creatorToken = await GetTokenForRoleAsync("DOMAIN_PROFESSIONAL", "creator_leave1", "creator_leave1@test.com", "Password@123");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", creatorToken);

        var createResp = await _client.PostAsJsonAsync("/api/chat-groups", new CreateChatGroupRequestModel("Leave Test Group", "Test leave", null, "FREE", 0));
        var createResult = await createResp.Content.ReadFromJsonAsync<Result<ChatGroupModel>>();
        Assert.NotNull(createResult?.Data);
        var groupId = createResult.Data.ChatGroupId;

        var memberToken = await GetTokenForRoleAsync("MEMBER", "joiner_leave1", "joiner_leave1@test.com", "Password@123");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", memberToken);

        // Join
        await _client.PostAsJsonAsync($"/api/chat-groups/{groupId}/join", "");

        // Leave
        var leaveResp = await _client.PostAsJsonAsync($"/api/chat-groups/{groupId}/leave", "");
        Assert.Equal(HttpStatusCode.OK, leaveResp.StatusCode);
        var leaveResult = await leaveResp.Content.ReadFromJsonAsync<Result>();
        Assert.True(leaveResult?.IsSuccess);

        // Verify IsJoined == false
        var getResp = await _client.GetAsync($"/api/chat-groups/{groupId}");
        var getResult = await getResp.Content.ReadFromJsonAsync<Result<ChatGroupModel>>();
        Assert.NotNull(getResult?.Data);
        Assert.False(getResult.Data.IsJoined);
    }

    [Fact]
    public async Task Owner_CannotLeave_OwnChatGroup()
    {
        var creatorToken = await GetTokenForRoleAsync("DOMAIN_PROFESSIONAL", "creator_own1", "creator_own1@test.com", "Password@123");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", creatorToken);

        var createResp = await _client.PostAsJsonAsync("/api/chat-groups", new CreateChatGroupRequestModel("Owner Leave Test", "Test owner leave", null, "FREE", 0));
        var createResult = await createResp.Content.ReadFromJsonAsync<Result<ChatGroupModel>>();
        Assert.NotNull(createResult?.Data);
        var groupId = createResult.Data.ChatGroupId;

        // Owner attempts to leave
        var leaveResp = await _client.PostAsJsonAsync($"/api/chat-groups/{groupId}/leave", "");
        Assert.Equal(HttpStatusCode.BadRequest, leaveResp.StatusCode);
        var leaveResult = await leaveResp.Content.ReadFromJsonAsync<Result>();
        Assert.NotNull(leaveResult);
        Assert.False(leaveResult.IsSuccess);
        Assert.Contains("Owner cannot leave their own Chat Group.", leaveResult.Message);
    }

    [Fact]
    public async Task Creator_CanSee_Members()
    {
        var creatorToken = await GetTokenForRoleAsync("DOMAIN_PROFESSIONAL", "creator_mem1", "creator_mem1@test.com", "Password@123");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", creatorToken);

        var createResp = await _client.PostAsJsonAsync("/api/chat-groups", new CreateChatGroupRequestModel("Members List Test", "Test member list", null, "FREE", 0));
        var createResult = await createResp.Content.ReadFromJsonAsync<Result<ChatGroupModel>>();
        Assert.NotNull(createResult?.Data);
        var groupId = createResult.Data.ChatGroupId;

        var memberToken = await GetTokenForRoleAsync("MEMBER", "joiner_mem1", "joiner_mem1@test.com", "Password@123");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", memberToken);
        await _client.PostAsJsonAsync($"/api/chat-groups/{groupId}/join", "");

        // Creator views member list
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", creatorToken);
        var membersResp = await _client.GetAsync($"/api/chat-groups/{groupId}/members");
        Assert.Equal(HttpStatusCode.OK, membersResp.StatusCode);

        var membersResult = await membersResp.Content.ReadFromJsonAsync<Result<IReadOnlyList<ChatGroupMemberModel>>>();
        Assert.NotNull(membersResult?.Data);
        Assert.True(membersResult.IsSuccess);
        Assert.Equal(2, membersResult.Data.Count);
        Assert.Contains(membersResult.Data, m => m.Role == "OWNER");
        Assert.Contains(membersResult.Data, m => m.Role == "MEMBER" && m.UserName == "joiner_mem1");
    }

    [Fact]
    public async Task ChatGroupMembership_DoesNotAffect_TblGroupMember()
    {
        var creatorToken = await GetTokenForRoleAsync("DOMAIN_PROFESSIONAL", "creator_iso1", "creator_iso1@test.com", "Password@123");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", creatorToken);

        var createResp = await _client.PostAsJsonAsync("/api/chat-groups", new CreateChatGroupRequestModel("Isolated Group", "Test separation", null, "FREE", 0));
        var createResult = await createResp.Content.ReadFromJsonAsync<Result<ChatGroupModel>>();
        Assert.NotNull(createResult?.Data);
        var chatGroupId = createResult.Data.ChatGroupId;

        var memberToken = await GetTokenForRoleAsync("MEMBER", "joiner_iso1", "joiner_iso1@test.com", "Password@123");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", memberToken);
        await _client.PostAsJsonAsync($"/api/chat-groups/{chatGroupId}/join", "");

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var memberUser = await db.TblUsers.FirstAsync(u => u.Email == "joiner_iso1@test.com");

        // Verify TblChatGroupMember exists
        var hasChatGroupMember = await db.TblChatGroupMembers.AnyAsync(m => m.ChatGroupId == chatGroupId && m.UserId == memberUser.UserId && !m.IsDeleted);
        Assert.True(hasChatGroupMember);

        // Verify TblGroupMember is untouched
        var hasGroupMember = await db.TblGroupMembers.AnyAsync(m => m.UserId == memberUser.UserId && !m.IsDeleted);
        Assert.False(hasGroupMember, "Chat Group join must NOT create or mutate TblGroupMember.");
    }
}
