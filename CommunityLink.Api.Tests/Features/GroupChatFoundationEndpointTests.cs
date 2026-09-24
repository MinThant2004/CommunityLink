using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityLink.Api.Tests.Infrastructure;
using CommunityLink.Database.AppDbContextModels;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Authentication;
using CommunityLink.Shared.Features.Chat;
using CommunityLink.Shared.Features.Community;
using CommunityLink.Shared.Features.Group;
using CommunityLink.Shared.Features.GroupChat;
using CommunityLink.Shared.Security;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CommunityLink.Api.Tests.Features;

public class GroupChatFoundationEndpointTests : IClassFixture<CommunityApiFactory>
{
    private readonly CommunityApiFactory _factory;
    private readonly HttpClient _client;

    public GroupChatFoundationEndpointTests(CommunityApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<string> GetDomainProfessionalTokenAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var role = await EnsureRoleAsync(db, "DOMAIN_PROFESSIONAL", "Domain Professional");
        await EnsureUserAsync(db, "dpuser_gc", "dp_gc@communitylink.local", "PremiumDP@123", role.RoleId);

        var login = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginRequestModel("dp_gc@communitylink.local", "PremiumDP@123"));
        var loginResult = await login.Content.ReadFromJsonAsync<Result<LoginResponseModel>>();
        Assert.NotNull(loginResult?.Data?.AccessToken);
        return loginResult.Data.AccessToken;
    }

    private async Task<string> GetMemberTokenAsync(string username = "member_gc", string email = "member_gc@communitylink.local")
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var role = db.TblRoles.First(r => r.RoleCode == "MEMBER");
        await EnsureUserAsync(db, username, email, "Password@123", role.RoleId);

        var login = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginRequestModel(email, "Password@123"));
        var loginResult = await login.Content.ReadFromJsonAsync<Result<LoginResponseModel>>();
        Assert.NotNull(loginResult?.Data?.AccessToken);
        return loginResult.Data.AccessToken;
    }

    private async Task<string> GetAdminTokenAsync()
    {
        var login = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginRequestModel("admin@communitylink.local", "Admin@123"));
        var loginResult = await login.Content.ReadFromJsonAsync<Result<LoginResponseModel>>();
        Assert.NotNull(loginResult?.Data?.AccessToken);
        return loginResult.Data.AccessToken;
    }

    private async Task<int> CreateSubCommunityAndGroupAsync(string creatorToken)
    {
        var adminToken = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var commRes = await _client.PostAsJsonAsync("/api/communities", new CreateCommunityRequestModel(
            "GC_Comm_" + Guid.NewGuid().ToString("N")[..6],
            null,
            "Group chat test community",
            null, null,
            "PUBLIC",
            "INSTANT",
            null
        ));

        Assert.Equal(HttpStatusCode.OK, commRes.StatusCode);
        var commResult = await commRes.Content.ReadFromJsonAsync<Result<CommunityModel>>();
        Assert.NotNull(commResult?.Data);
        int subCommunityId = commResult.Data.CommunityId;

        // Admin joins subcommunity
        await _client.PostAsJsonAsync($"/api/communities/{subCommunityId}/join", "");

        // User joins subcommunity if user is not admin
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", creatorToken);
        await _client.PostAsJsonAsync($"/api/communities/{subCommunityId}/join", "");

        // Create group
        var groupRes = await _client.PostAsJsonAsync("/api/groups", new CreateGroupRequestModel(
            subCommunityId,
            "GC_Group_" + Guid.NewGuid().ToString("N")[..6],
            "Group for chat tests",
            null, null, "PUBLIC", "INSTANT"
        ));

        Assert.Equal(HttpStatusCode.OK, groupRes.StatusCode);
        var groupResult = await groupRes.Content.ReadFromJsonAsync<Result<GroupModel>>();
        Assert.NotNull(groupResult?.Data);
        return groupResult.Data.GroupId;
    }

    // 1. Premium user can create Group Chat & 4. Linked to existing group
    [Fact]
    public async Task CreateGroupChat_AsPremiumUser_SucceedsAndLinksToGroup()
    {
        var dpToken = await GetDomainProfessionalTokenAsync();
        int groupId = await CreateSubCommunityAndGroupAsync(dpToken);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", dpToken);

        var createRes = await _client.PostAsJsonAsync($"/api/groups/{groupId}/chatroom", new CreateGroupChatRoomRequestModel(
            groupId, "FREE", 0
        ));

        Assert.Equal(HttpStatusCode.OK, createRes.StatusCode);
        var result = await createRes.Content.ReadFromJsonAsync<Result<GroupChatRoomModel>>();
        Assert.NotNull(result?.Data);
        Assert.True(result.IsSuccess);
        Assert.Equal(groupId, result.Data.GroupId);
        Assert.Equal("FREE", result.Data.ChatType);
    }

    // 2. Normal user cannot create Group Chat
    [Fact]
    public async Task CreateGroupChat_AsNormalUser_ReturnsForbidden()
    {
        var memberToken = await GetMemberTokenAsync();
        int groupId = await CreateSubCommunityAndGroupAsync(memberToken);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", memberToken);

        var createRes = await _client.PostAsJsonAsync($"/api/groups/{groupId}/chatroom", new CreateGroupChatRoomRequestModel(
            groupId, "FREE", 0
        ));

        Assert.Equal(HttpStatusCode.Forbidden, createRes.StatusCode);
    }

    // 3. Admin cannot create Group Chat (unless premium role)
    [Fact]
    public async Task CreateGroupChat_AsAdmin_ReturnsForbidden()
    {
        var adminToken = await GetAdminTokenAsync();
        int groupId = await CreateSubCommunityAndGroupAsync(adminToken);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var createRes = await _client.PostAsJsonAsync($"/api/groups/{groupId}/chatroom", new CreateGroupChatRoomRequestModel(
            groupId, "FREE", 0
        ));

        Assert.Equal(HttpStatusCode.Forbidden, createRes.StatusCode);
    }

    // 5. Group Chat membership handled & 6. Messages can be created & 7. Messages can be retrieved
    [Fact]
    public async Task GroupChatMessage_SendAndRetrieve_SucceedsForGroupMember()
    {
        var dpToken = await GetDomainProfessionalTokenAsync();
        int groupId = await CreateSubCommunityAndGroupAsync(dpToken);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", dpToken);

        // Create Group Chat Room
        await _client.PostAsJsonAsync($"/api/groups/{groupId}/chatroom", new CreateGroupChatRoomRequestModel(groupId, "FREE", 0));

        // Send Message
        var sendRes = await _client.PostAsJsonAsync($"/api/groups/{groupId}/chatroom/messages", new SendGroupChatMessageRequestModel("Hello Group Chat!"));
        Assert.Equal(HttpStatusCode.OK, sendRes.StatusCode);

        var sendResult = await sendRes.Content.ReadFromJsonAsync<Result<GroupChatMessageModel>>();
        Assert.NotNull(sendResult?.Data);
        Assert.Equal("Hello Group Chat!", sendResult.Data.Content);

        // Get Messages
        var getRes = await _client.GetAsync($"/api/groups/{groupId}/chatroom/messages");
        Assert.Equal(HttpStatusCode.OK, getRes.StatusCode);

        var getResult = await getRes.Content.ReadFromJsonAsync<Result<IReadOnlyList<GroupChatMessageModel>>>();
        Assert.NotNull(getResult?.Data);
        Assert.Single(getResult.Data);
        Assert.Equal("Hello Group Chat!", getResult.Data[0].Content);
    }

    // 8. Multiple members can receive messages
    [Fact]
    public async Task GroupChatMessage_MultipleMembersCanSendAndReceive()
    {
        var dpToken = await GetDomainProfessionalTokenAsync();
        int groupId = await CreateSubCommunityAndGroupAsync(dpToken);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", dpToken);
        await _client.PostAsJsonAsync($"/api/groups/{groupId}/chatroom", new CreateGroupChatRoomRequestModel(groupId, "FREE", 0));
        await _client.PostAsJsonAsync($"/api/groups/{groupId}/chatroom/messages", new SendGroupChatMessageRequestModel("Message from DP"));

        // Second member joins group
        var member2Token = await GetMemberTokenAsync("member2_gc", "member2_gc@communitylink.local");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", member2Token);
        await _client.PostAsJsonAsync($"/api/groups/{groupId}/join", "");

        // Second member sends message
        await _client.PostAsJsonAsync($"/api/groups/{groupId}/chatroom/messages", new SendGroupChatMessageRequestModel("Message from Member 2"));

        // Second member gets all messages
        var getRes = await _client.GetAsync($"/api/groups/{groupId}/chatroom/messages");
        Assert.Equal(HttpStatusCode.OK, getRes.StatusCode);

        var getResult = await getRes.Content.ReadFromJsonAsync<Result<IReadOnlyList<GroupChatMessageModel>>>();
        Assert.NotNull(getResult?.Data);
        Assert.Equal(2, getResult.Data.Count);
        Assert.Equal("Message from DP", getResult.Data[0].Content);
        Assert.Equal("Message from Member 2", getResult.Data[1].Content);
    }

    // Non-member cannot read or send group chat messages
    [Fact]
    public async Task GroupChatMessage_NonMember_ReturnsForbidden()
    {
        var dpToken = await GetDomainProfessionalTokenAsync();
        int groupId = await CreateSubCommunityAndGroupAsync(dpToken);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", dpToken);
        await _client.PostAsJsonAsync($"/api/groups/{groupId}/chatroom", new CreateGroupChatRoomRequestModel(groupId, "FREE", 0));

        // Non member tries to access
        var outsiderToken = await GetMemberTokenAsync("outsider_gc", "outsider_gc@communitylink.local");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", outsiderToken);

        var getRes = await _client.GetAsync($"/api/groups/{groupId}/chatroom/messages");
        Assert.Equal(HttpStatusCode.Forbidden, getRes.StatusCode);

        var sendRes = await _client.PostAsJsonAsync($"/api/groups/{groupId}/chatroom/messages", new SendGroupChatMessageRequestModel("Unauthorized message"));
        Assert.Equal(HttpStatusCode.Forbidden, sendRes.StatusCode);
    }

    // 9. Existing 1-to-1 DM functionality still works
    [Fact]
    public async Task DirectMessaging_ExistingDM_StillWorks()
    {
        var dpToken = await GetDomainProfessionalTokenAsync();
        var memberToken = await GetMemberTokenAsync("dm_member_gc", "dm_member_gc@communitylink.local");

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", dpToken);

        var conversationsRes = await _client.GetAsync("/api/chat/conversations");
        Assert.Equal(HttpStatusCode.OK, conversationsRes.StatusCode);
    }

    // 10. Existing normal Group creation still works
    [Fact]
    public async Task ExistingNormalGroupCreation_StillWorks()
    {
        var memberToken = await GetMemberTokenAsync("normal_grp_user", "normal_grp_user@communitylink.local");
        int groupId = await CreateSubCommunityAndGroupAsync(memberToken);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", memberToken);
        var groupRes = await _client.GetAsync($"/api/groups/{groupId}");
        Assert.Equal(HttpStatusCode.OK, groupRes.StatusCode);
    }

    private static async Task<TblRole> EnsureRoleAsync(AppDbContext db, string roleCode, string roleName)
    {
        var role = db.TblRoles.FirstOrDefault(r => r.RoleCode == roleCode);
        if (role is null)
        {
            role = new TblRole
            {
                RoleCode = roleCode,
                RoleName = roleName,
                Description = $"Role {roleName}",
                IsSystemRole = false,
                CreatedAt = DateTime.UtcNow
            };
            db.TblRoles.Add(role);
            await db.SaveChangesAsync();

            var defaultCodes = PermissionCatalog.DefaultForRole(roleCode);
            var permissions = db.TblPermissions.Where(p => defaultCodes.Contains(p.PermissionCode)).ToList();
            foreach (var perm in permissions)
            {
                if (!db.TblRolePermissions.Any(rp => rp.RoleId == role.RoleId && rp.PermissionId == perm.PermissionId))
                {
                    db.TblRolePermissions.Add(new TblRolePermission
                    {
                        RoleId = role.RoleId,
                        PermissionId = perm.PermissionId,
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
            await db.SaveChangesAsync();
        }
        return role;
    }

    private static async Task EnsureUserAsync(AppDbContext db, string userName, string email, string password, int roleId)
    {
        var existingUser = db.TblUsers.FirstOrDefault(u => u.NormalizedEmail == email.ToUpperInvariant());
        if (existingUser is null)
        {
            var user = new TblUser
            {
                UserName = userName,
                NormalizedUserName = userName.ToUpperInvariant(),
                DisplayName = userName,
                Email = email,
                NormalizedEmail = email.ToUpperInvariant(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password, 12),
                IsActive = true,
                IsVerified = true,
                CreatedAt = DateTime.UtcNow
            };
            db.TblUsers.Add(user);
            await db.SaveChangesAsync();

            db.TblUserRoles.Add(new TblUserRole { UserId = user.UserId, RoleId = roleId, CreatedAt = DateTime.UtcNow });
            await db.SaveChangesAsync();
        }
    }
}
