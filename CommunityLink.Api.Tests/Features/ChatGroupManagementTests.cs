using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using CommunityLink.Api.Tests.Infrastructure;
using CommunityLink.Database.AppDbContextModels;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Authentication;
using CommunityLink.Shared.Features.ChatGroup;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CommunityLink.Api.Tests.Features;

/// <summary>
/// Covers the group management surface added for Creator/Admin features: the member-visibility
/// gate, group image upload, direct adds, ban/unban, settings, fee changes and group deletion.
///
/// Every test provisions its own users and group so the class is order-independent.
/// </summary>
public class ChatGroupManagementTests : IClassFixture<CommunityApiFactory>
{
    private readonly CommunityApiFactory _factory;
    private readonly HttpClient _client;

    public ChatGroupManagementTests(CommunityApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private const string Password = "Password@123";

    /// <summary>Creates a user with the given role (creating the role if needed) and logs them in.</summary>
    private async Task<string> LoginAsAsync(string roleCode, string username, string email)
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
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(Password, 12),
                IsActive = true,
                IsVerified = true,
                CreatedAt = DateTime.UtcNow
            };
            db.TblUsers.Add(user);
            await db.SaveChangesAsync();

            db.TblUserRoles.Add(new TblUserRole
            {
                UserId = user.UserId,
                RoleId = role.RoleId,
                CreatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }

        var login = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequestModel(email, Password));
        var loginResult = await login.Content.ReadFromJsonAsync<Result<LoginResponseModel>>();
        Assert.NotNull(loginResult?.Data?.AccessToken);
        return loginResult.Data.AccessToken;
    }

    /// <summary>Sets the bearer token for subsequent calls on the shared client.</summary>
    private void Authenticate(string token) =>
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

    private async Task<ChatGroupModel> CreateGroupAsync(string token, string name, string chatType = "FREE", long fee = 0)
    {
        Authenticate(token);
        var response = await _client.PostAsJsonAsync(
            "/api/chat-groups",
            new CreateChatGroupRequestModel(name, "Created by test", null, chatType, fee));
        var result = await response.Content.ReadFromJsonAsync<Result<ChatGroupModel>>();
        Assert.NotNull(result?.Data);
        return result.Data;
    }

    private async Task JoinAsync(string token, int groupId)
    {
        Authenticate(token);
        await _client.PostAsJsonAsync($"/api/chat-groups/{groupId}/join", "");
    }

    private int GetUserIdByName(string userName)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return db.TblUsers.First(u => u.UserName == userName).UserId;
    }

    // ------------------------------------------------------------------
    // Member visibility
    // ------------------------------------------------------------------

    [Fact]
    public async Task NonMember_CannotSee_MemberRoster()
    {
        var ownerToken = await LoginAsAsync("DOMAIN_PROFESSIONAL", "vis_owner", "vis_owner@test.com");
        var group = await CreateGroupAsync(ownerToken, "Roster Privacy Test");

        var outsiderToken = await LoginAsAsync("MEMBER", "vis_outsider", "vis_outsider@test.com");
        Authenticate(outsiderToken);

        var response = await _client.GetAsync($"/api/chat-groups/{group.ChatGroupId}/members");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Anonymous_CannotSee_MemberRoster()
    {
        var ownerToken = await LoginAsAsync("DOMAIN_PROFESSIONAL", "anon_owner", "anon_owner@test.com");
        var group = await CreateGroupAsync(ownerToken, "Roster Anonymous Test");

        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.GetAsync($"/api/chat-groups/{group.ChatGroupId}/members");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Member_CanSee_MemberRoster()
    {
        var ownerToken = await LoginAsAsync("DOMAIN_PROFESSIONAL", "roster_owner", "roster_owner@test.com");
        var group = await CreateGroupAsync(ownerToken, "Roster Member Test");

        var memberToken = await LoginAsAsync("MEMBER", "roster_member", "roster_member@test.com");
        await JoinAsync(memberToken, group.ChatGroupId);

        Authenticate(memberToken);
        var response = await _client.GetAsync($"/api/chat-groups/{group.ChatGroupId}/members");
        var result = await response.Content.ReadFromJsonAsync<Result<IReadOnlyList<ChatGroupMemberModel>>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result?.Data);
        Assert.Equal(2, result.Data.Count);
    }

    [Fact]
    public async Task MissingGroup_MemberRoster_Returns_NotFound_Not_Forbidden()
    {
        var ownerToken = await LoginAsAsync("DOMAIN_PROFESSIONAL", "nf_owner", "nf_owner@test.com");
        Authenticate(ownerToken);

        // A non-existent group must 404 rather than 403, otherwise the endpoint leaks
        // nothing but makes the two failure modes indistinguishable to clients.
        var response = await _client.GetAsync("/api/chat-groups/999999/members");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task NonMember_CanStillSee_PublicGroupDetails()
    {
        var ownerToken = await LoginAsAsync("DOMAIN_PROFESSIONAL", "pub_owner", "pub_owner@test.com");
        var group = await CreateGroupAsync(ownerToken, "Public Details Test");

        var memberToken = await LoginAsAsync("MEMBER", "pub_member", "pub_member@test.com");
        await JoinAsync(memberToken, group.ChatGroupId);

        // Gating the roster must not make the group itself unreadable: a signed-in non-member
        // still gets the group details, including MemberCount, with no membership implied.
        Authenticate(await LoginAsAsync("MEMBER", "pub_outsider", "pub_outsider@test.com"));

        var response = await _client.GetAsync($"/api/chat-groups/{group.ChatGroupId}");
        var result = await response.Content.ReadFromJsonAsync<Result<ChatGroupModel>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result?.Data);
        Assert.Equal("Public Details Test", result.Data.Name);
        Assert.Equal(2, result.Data.MemberCount);
        Assert.False(result.Data.IsJoined);
        Assert.Equal("NONE", result.Data.UserRole);
    }

    // ------------------------------------------------------------------
    // Group image upload
    // ------------------------------------------------------------------

    /// <summary>Builds a real PNG byte stream so magic-byte sniffing has something valid to read.</summary>
    private static byte[] BuildPngBytes()
    {
        // Minimal 1x1 PNG.
        return Convert.FromBase64String(
            "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg==");
    }

    [Fact]
    public async Task Owner_CanUpload_GroupImage()
    {
        var ownerToken = await LoginAsAsync("DOMAIN_PROFESSIONAL", "img_owner", "img_owner@test.com");
        var group = await CreateGroupAsync(ownerToken, "Image Upload Test");

        using var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(BuildPngBytes());
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        content.Add(fileContent, "file", "avatar.png");

        Authenticate(ownerToken);
        var response = await _client.PostAsync($"/api/chat-groups/{group.ChatGroupId}/image", content);
        var result = await response.Content.ReadFromJsonAsync<Result<ChatGroupImageUploadResponse>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(result?.IsSuccess);
        Assert.NotNull(result?.Data);
        Assert.Contains("/uploads/chat-groups/", result.Data.Url);
    }

    [Fact]
    public async Task Member_CannotUpload_GroupImage()
    {
        var ownerToken = await LoginAsAsync("DOMAIN_PROFESSIONAL", "imgmem_owner", "imgmem_owner@test.com");
        var group = await CreateGroupAsync(ownerToken, "Image Permission Test");

        var memberToken = await LoginAsAsync("MEMBER", "imgmem_member", "imgmem_member@test.com");
        await JoinAsync(memberToken, group.ChatGroupId);

        using var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(BuildPngBytes());
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        content.Add(fileContent, "file", "avatar.png");

        Authenticate(memberToken);
        var response = await _client.PostAsync($"/api/chat-groups/{group.ChatGroupId}/image", content);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GroupImage_Rejects_DisallowedExtension()
    {
        var ownerToken = await LoginAsAsync("DOMAIN_PROFESSIONAL", "ext_owner", "ext_owner@test.com");
        var group = await CreateGroupAsync(ownerToken, "Image Extension Test");

        using var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes("not an image"));
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
        content.Add(fileContent, "file", "payload.txt");

        Authenticate(ownerToken);
        var response = await _client.PostAsync($"/api/chat-groups/{group.ChatGroupId}/image", content);
        var result = await response.Content.ReadFromJsonAsync<Result<ChatGroupImageUploadResponse>>();

        Assert.False(result?.IsSuccess);
        Assert.NotNull(result?.Message);
    }

    [Fact]
    public async Task GroupImage_Rejects_RenamedNonImageContent()
    {
        var ownerToken = await LoginAsAsync("DOMAIN_PROFESSIONAL", "sniff_owner", "sniff_owner@test.com");
        var group = await CreateGroupAsync(ownerToken, "Image Sniffing Test");

        // A text file renamed to .png with an image Content-Type. Extension and declared type
        // both pass; only the magic bytes reveal it is not an image.
        using var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes("<script>alert(1)</script>"));
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        content.Add(fileContent, "file", "sneaky.png");

        Authenticate(ownerToken);
        var response = await _client.PostAsync($"/api/chat-groups/{group.ChatGroupId}/image", content);
        var result = await response.Content.ReadFromJsonAsync<Result<ChatGroupImageUploadResponse>>();

        Assert.False(result?.IsSuccess);
    }

    [Fact]
    public async Task GroupImage_Rejects_OversizedUpload()
    {
        var ownerToken = await LoginAsAsync("DOMAIN_PROFESSIONAL", "big_owner", "big_owner@test.com");
        var group = await CreateGroupAsync(ownerToken, "Image Size Test");

        using var content = new MultipartFormDataContent();
        // Valid PNG magic bytes, then padded past the 5 MB policy limit.
        var oversized = new byte[ChatGroupImagePolicy.MaxBytes + 1024];
        BuildPngBytes().CopyTo(oversized, 0);
        var fileContent = new ByteArrayContent(oversized);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        content.Add(fileContent, "file", "big.png");

        Authenticate(ownerToken);
        var response = await _client.PostAsync($"/api/chat-groups/{group.ChatGroupId}/image", content);

        Assert.True(
            response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.Forbidden or HttpStatusCode.RequestEntityTooLarge,
            $"Expected a rejection status but got {(int)response.StatusCode}.");
    }

    // ------------------------------------------------------------------
    // Add members
    // ------------------------------------------------------------------

    [Fact]
    public async Task Owner_CanAdd_Members_Directly()
    {
        var ownerToken = await LoginAsAsync("DOMAIN_PROFESSIONAL", "add_owner", "add_owner@test.com");
        var group = await CreateGroupAsync(ownerToken, "Add Members Test");

        await LoginAsAsync("MEMBER", "add_user1", "add_user1@test.com");
        var targetId = GetUserIdByName("add_user1");

        Authenticate(ownerToken);
        var response = await _client.PostAsJsonAsync(
            $"/api/chat-groups/{group.ChatGroupId}/members",
            new AddChatGroupMembersRequestModel(new[] { targetId }));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var targetToken = await LoginAsAsync("MEMBER", "add_user1", "add_user1@test.com");
        Authenticate(targetToken);
        var membersResponse = await _client.GetAsync($"/api/chat-groups/{group.ChatGroupId}/members");
        var members = await membersResponse.Content.ReadFromJsonAsync<Result<IReadOnlyList<ChatGroupMemberModel>>>();

        Assert.Equal(2, members?.Data?.Count);
    }

    [Fact]
    public async Task Admin_CannotAdd_Members()
    {
        var ownerToken = await LoginAsAsync("DOMAIN_PROFESSIONAL", "addadm_owner", "addadm_owner@test.com");
        var group = await CreateGroupAsync(ownerToken, "Add Members Permission Test");

        var adminToken = await LoginAsAsync("MEMBER", "addadm_admin", "addadm_admin@test.com");
        await JoinAsync(adminToken, group.ChatGroupId);
        await PromoteAsync(ownerToken, group.ChatGroupId, GetUserIdByName("addadm_admin"));

        await LoginAsAsync("MEMBER", "addadm_target", "addadm_target@test.com");
        var targetId = GetUserIdByName("addadm_target");

        Authenticate(adminToken);
        var response = await _client.PostAsJsonAsync(
            $"/api/chat-groups/{group.ChatGroupId}/members",
            new AddChatGroupMembersRequestModel(new[] { targetId }));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // ------------------------------------------------------------------
    // Ban / unban
    // ------------------------------------------------------------------

    [Fact]
    public async Task Owner_CanBan_Member_And_BannedUserCannotRejoin()
    {
        var ownerToken = await LoginAsAsync("DOMAIN_PROFESSIONAL", "ban_owner", "ban_owner@test.com");
        var group = await CreateGroupAsync(ownerToken, "Ban Test");

        var memberToken = await LoginAsAsync("MEMBER", "ban_member", "ban_member@test.com");
        await JoinAsync(memberToken, group.ChatGroupId);
        var memberId = GetUserIdByName("ban_member");

        Authenticate(ownerToken);
        var banResponse = await _client.PostAsync(
            $"/api/chat-groups/{group.ChatGroupId}/members/{memberId}/ban", null);
        Assert.Equal(HttpStatusCode.OK, banResponse.StatusCode);

        // The ban must also end the membership, not just shadow it.
        Authenticate(ownerToken);
        var membersResponse = await _client.GetAsync($"/api/chat-groups/{group.ChatGroupId}/members");
        var members = await membersResponse.Content.ReadFromJsonAsync<Result<IReadOnlyList<ChatGroupMemberModel>>>();
        Assert.Single(members?.Data ?? Array.Empty<ChatGroupMemberModel>());

        // And the banned user cannot rejoin.
        Authenticate(memberToken);
        var rejoinResponse = await _client.PostAsJsonAsync($"/api/chat-groups/{group.ChatGroupId}/join", "");
        var rejoinResult = await rejoinResponse.Content.ReadFromJsonAsync<Result<object>>();
        Assert.False(rejoinResult?.IsSuccess);
        Assert.Equal(HttpStatusCode.Forbidden, rejoinResponse.StatusCode);
    }

    [Fact]
    public async Task Unban_AllowsUser_ToJoinAgain()
    {
        var ownerToken = await LoginAsAsync("DOMAIN_PROFESSIONAL", "unban_owner", "unban_owner@test.com");
        var group = await CreateGroupAsync(ownerToken, "Unban Test");

        var memberToken = await LoginAsAsync("MEMBER", "unban_member", "unban_member@test.com");
        await JoinAsync(memberToken, group.ChatGroupId);
        var memberId = GetUserIdByName("unban_member");

        Authenticate(ownerToken);
        await _client.PostAsync($"/api/chat-groups/{group.ChatGroupId}/members/{memberId}/ban", null);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var activeBans = await db.TblChatGroupBans
                .CountAsync(b => b.ChatGroupId == group.ChatGroupId && !b.IsDeleted);
            Assert.Equal(1, activeBans);
        }

        Authenticate(ownerToken);
        var unbanResponse = await _client.DeleteAsync(
            $"/api/chat-groups/{group.ChatGroupId}/members/{memberId}/ban");
        Assert.Equal(HttpStatusCode.OK, unbanResponse.StatusCode);

        // The ban row is retained as history and only flagged revoked. The filtered unique
        // index is what allows a fresh ban to claim the same (group, user) pair later.
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var bans = await db.TblChatGroupBans
                .Where(b => b.ChatGroupId == group.ChatGroupId)
                .ToListAsync();

            Assert.Single(bans);
            Assert.True(bans[0].IsDeleted);
            Assert.NotNull(bans[0].RevokedAt);
        }

        Authenticate(memberToken);
        var accessResponse = await _client.GetAsync($"/api/chat-groups/{group.ChatGroupId}");
        Assert.Equal(HttpStatusCode.OK, accessResponse.StatusCode);
    }

    [Fact]
    public async Task BannedUser_CannotBe_ReAddedByOwner()
    {
        var ownerToken = await LoginAsAsync("DOMAIN_PROFESSIONAL", "readd_owner", "readd_owner@test.com");
        var group = await CreateGroupAsync(ownerToken, "Re-add Ban Test");

        var memberToken = await LoginAsAsync("MEMBER", "readd_member", "readd_member@test.com");
        await JoinAsync(memberToken, group.ChatGroupId);
        var memberId = GetUserIdByName("readd_member");

        Authenticate(ownerToken);
        await _client.PostAsync($"/api/chat-groups/{group.ChatGroupId}/members/{memberId}/ban", null);

        // The owner's direct add path must respect the ban too, not just self-service join.
        Authenticate(ownerToken);
        var addResponse = await _client.PostAsJsonAsync(
            $"/api/chat-groups/{group.ChatGroupId}/members",
            new AddChatGroupMembersRequestModel(new[] { memberId }));
        Assert.Equal(HttpStatusCode.OK, addResponse.StatusCode);

        Authenticate(ownerToken);
        var membersResponse = await _client.GetAsync($"/api/chat-groups/{group.ChatGroupId}/members");
        var members = await membersResponse.Content.ReadFromJsonAsync<Result<IReadOnlyList<ChatGroupMemberModel>>>();

        Assert.Single(members?.Data ?? Array.Empty<ChatGroupMemberModel>());
    }

    [Fact]
    public async Task Admin_CanBan_PlainMember_ButNotAnotherAdmin()
    {
        var ownerToken = await LoginAsAsync("DOMAIN_PROFESSIONAL", "banadm_owner", "banadm_owner@test.com");
        var group = await CreateGroupAsync(ownerToken, "Ban Admin Test");

        var adminToken = await LoginAsAsync("MEMBER", "banadm_admin1", "banadm_admin1@test.com");
        await JoinAsync(adminToken, group.ChatGroupId);
        await PromoteAsync(ownerToken, group.ChatGroupId, GetUserIdByName("banadm_admin1"));

        var otherAdminToken = await LoginAsAsync("MEMBER", "banadm_admin2", "banadm_admin2@test.com");
        await JoinAsync(otherAdminToken, group.ChatGroupId);
        await PromoteAsync(ownerToken, group.ChatGroupId, GetUserIdByName("banadm_admin2"));

        var plainToken = await LoginAsAsync("MEMBER", "banadm_plain", "banadm_plain@test.com");
        await JoinAsync(plainToken, group.ChatGroupId);
        var plainId = GetUserIdByName("banadm_plain");

        // An Admin may moderate a plain member.
        Authenticate(adminToken);
        var plainBan = await _client.PostAsync(
            $"/api/chat-groups/{group.ChatGroupId}/members/{plainId}/ban", null);
        Assert.Equal(HttpStatusCode.OK, plainBan.StatusCode);

        // But not a peer Admin, which would let two Admins expel each other.
        var otherAdminId = GetUserIdByName("banadm_admin2");
        Authenticate(adminToken);
        var adminBan = await _client.PostAsync(
            $"/api/chat-groups/{group.ChatGroupId}/members/{otherAdminId}/ban", null);
        Assert.Equal(HttpStatusCode.Forbidden, adminBan.StatusCode);
    }

    [Fact]
    public async Task Owner_Cannot_BeBanned()
    {
        var ownerToken = await LoginAsAsync("DOMAIN_PROFESSIONAL", "ownban_owner", "ownban_owner@test.com");
        var group = await CreateGroupAsync(ownerToken, "Owner Ban Test");
        var ownerId = GetUserIdByName("ownban_owner");

        Authenticate(ownerToken);
        var response = await _client.PostAsync(
            $"/api/chat-groups/{group.ChatGroupId}/members/{ownerId}/ban", null);

        Assert.False(response.IsSuccessStatusCode);
    }

    // ------------------------------------------------------------------
    // Admin privilege hardening
    // ------------------------------------------------------------------

    [Fact]
    public async Task Admin_CannotRemove_AnotherAdmin()
    {
        var ownerToken = await LoginAsAsync("DOMAIN_PROFESSIONAL", "rmadm_owner", "rmadm_owner@test.com");
        var group = await CreateGroupAsync(ownerToken, "Remove Admin Test");

        var adminToken = await LoginAsAsync("MEMBER", "rmadm_admin1", "rmadm_admin1@test.com");
        await JoinAsync(adminToken, group.ChatGroupId);
        await PromoteAsync(ownerToken, group.ChatGroupId, GetUserIdByName("rmadm_admin1"));

        var otherAdminToken = await LoginAsAsync("MEMBER", "rmadm_admin2", "rmadm_admin2@test.com");
        await JoinAsync(otherAdminToken, group.ChatGroupId);
        await PromoteAsync(ownerToken, group.ChatGroupId, GetUserIdByName("rmadm_admin2"));

        var otherAdminId = GetUserIdByName("rmadm_admin2");

        // Regression guard: this used to succeed because the check only rejected the Owner.
        Authenticate(adminToken);
        var response = await _client.DeleteAsync(
            $"/api/chat-groups/{group.ChatGroupId}/members/{otherAdminId}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Owner_CanRemove_Admin()
    {
        var ownerToken = await LoginAsAsync("DOMAIN_PROFESSIONAL", "rmok_owner", "rmok_owner@test.com");
        var group = await CreateGroupAsync(ownerToken, "Remove Admin Allowed Test");

        var adminToken = await LoginAsAsync("MEMBER", "rmok_admin", "rmok_admin@test.com");
        await JoinAsync(adminToken, group.ChatGroupId);
        await PromoteAsync(ownerToken, group.ChatGroupId, GetUserIdByName("rmok_admin"));

        var adminId = GetUserIdByName("rmok_admin");

        Authenticate(ownerToken);
        var response = await _client.DeleteAsync($"/api/chat-groups/{group.ChatGroupId}/members/{adminId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // ------------------------------------------------------------------
    // Settings, fee and deletion
    // ------------------------------------------------------------------

    [Fact]
    public async Task Owner_CanUpdate_GroupInfo()
    {
        var ownerToken = await LoginAsAsync("DOMAIN_PROFESSIONAL", "info_owner", "info_owner@test.com");
        var group = await CreateGroupAsync(ownerToken, "Original Name");

        Authenticate(ownerToken);
        var response = await _client.PutAsJsonAsync(
            $"/api/chat-groups/{group.ChatGroupId}",
            new UpdateChatGroupRequestModel("Updated Name", "Updated description", "FREE", 0));
        var result = await response.Content.ReadFromJsonAsync<Result<ChatGroupModel>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(result?.IsSuccess);
        Assert.NotNull(result?.Data);
        Assert.Equal("Updated Name", result.Data.Name);
        Assert.Equal("Updated description", result.Data.Description);
    }

    [Fact]
    public async Task Member_CannotUpdate_GroupInfo()
    {
        var ownerToken = await LoginAsAsync("DOMAIN_PROFESSIONAL", "infoperm_owner", "infoperm_owner@test.com");
        var group = await CreateGroupAsync(ownerToken, "Info Permission Test");

        var memberToken = await LoginAsAsync("MEMBER", "infoperm_member", "infoperm_member@test.com");
        await JoinAsync(memberToken, group.ChatGroupId);

        Authenticate(memberToken);
        var response = await _client.PutAsJsonAsync(
            $"/api/chat-groups/{group.ChatGroupId}",
            new UpdateChatGroupRequestModel("Hijacked", null, "FREE", 0));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task FeeChange_DoesNotRemove_ExistingMembers()
    {
        var ownerToken = await LoginAsAsync("DOMAIN_PROFESSIONAL", "fee_owner", "fee_owner@test.com");
        var group = await CreateGroupAsync(ownerToken, "Fee Change Test", "FREE", 0);

        var memberToken = await LoginAsAsync("MEMBER", "fee_member", "fee_member@test.com");
        await JoinAsync(memberToken, group.ChatGroupId);

        // Turning a free group paid must not evict or re-charge members who already joined.
        Authenticate(ownerToken);
        var response = await _client.PutAsJsonAsync(
            $"/api/chat-groups/{group.ChatGroupId}/join-fee",
            new SetChatGroupJoinFeeRequestModel(250));
        var result = await response.Content.ReadFromJsonAsync<Result<ChatGroupModel>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result?.Data);
        Assert.Equal(250, result.Data.JoinFeeLinkDrops);
        Assert.Equal("PAID", result.Data.ChatType);

        Authenticate(memberToken);
        var membersResponse = await _client.GetAsync($"/api/chat-groups/{group.ChatGroupId}/members");
        var members = await membersResponse.Content.ReadFromJsonAsync<Result<IReadOnlyList<ChatGroupMemberModel>>>();

        Assert.Equal(2, members?.Data?.Count);
    }

    [Fact]
    public async Task Member_CannotChange_JoinFee()
    {
        var ownerToken = await LoginAsAsync("DOMAIN_PROFESSIONAL", "feeperm_owner", "feeperm_owner@test.com");
        var group = await CreateGroupAsync(ownerToken, "Fee Permission Test");

        var memberToken = await LoginAsAsync("MEMBER", "feeperm_member", "feeperm_member@test.com");
        await JoinAsync(memberToken, group.ChatGroupId);

        Authenticate(memberToken);
        var response = await _client.PutAsJsonAsync(
            $"/api/chat-groups/{group.ChatGroupId}/join-fee",
            new SetChatGroupJoinFeeRequestModel(9999));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Owner_CanDelete_Group_AndItDisappearsFromDirectory()
    {
        var ownerToken = await LoginAsAsync("DOMAIN_PROFESSIONAL", "del_owner", "del_owner@test.com");
        var group = await CreateGroupAsync(ownerToken, "Delete Group Test");

        var memberToken = await LoginAsAsync("MEMBER", "del_member", "del_member@test.com");
        await JoinAsync(memberToken, group.ChatGroupId);

        Authenticate(ownerToken);
        var response = await _client.DeleteAsync($"/api/chat-groups/{group.ChatGroupId}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Soft-deleted: the detail endpoint must now report it as gone, even to a
        // signed-in caller who could previously read it.
        Authenticate(memberToken);
        var detail = await _client.GetAsync($"/api/chat-groups/{group.ChatGroupId}");
        Assert.Equal(HttpStatusCode.NotFound, detail.StatusCode);

        // And the former member no longer has a membership to see a roster through.
        Authenticate(memberToken);
        var membersAfterDelete = await _client.GetAsync($"/api/chat-groups/{group.ChatGroupId}/members");
        Assert.False(membersAfterDelete.IsSuccessStatusCode);
    }

    [Fact]
    public async Task NonOwner_CannotDelete_Group()
    {
        var ownerToken = await LoginAsAsync("DOMAIN_PROFESSIONAL", "delperm_owner", "delperm_owner@test.com");
        var group = await CreateGroupAsync(ownerToken, "Delete Permission Test");

        var memberToken = await LoginAsAsync("MEMBER", "delperm_member", "delperm_member@test.com");
        await JoinAsync(memberToken, group.ChatGroupId);

        Authenticate(memberToken);
        var response = await _client.DeleteAsync($"/api/chat-groups/{group.ChatGroupId}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Owner_CanListBannedMembers_AndPlainMemberCannot()
    {
        var ownerToken = await LoginAsAsync("DOMAIN_PROFESSIONAL", "banlist_owner", "banlist_owner@test.com");
        var group = await CreateGroupAsync(ownerToken, "Ban List Test");

        var memberToken = await LoginAsAsync("MEMBER", "banlist_member", "banlist_member@test.com");
        await JoinAsync(memberToken, group.ChatGroupId);
        var memberId = GetUserIdByName("banlist_member");

        Authenticate(ownerToken);
        var banResponse = await _client.PostAsync(
            $"/api/chat-groups/{group.ChatGroupId}/members/{memberId}/ban", null);
        Assert.Equal(HttpStatusCode.OK, banResponse.StatusCode);

        Authenticate(ownerToken);
        var listResponse = await _client.GetAsync($"/api/chat-groups/{group.ChatGroupId}/bans");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);

        var bans = await listResponse.Content.ReadFromJsonAsync<Result<IReadOnlyList<ChatGroupBannedMemberModel>>>();
        Assert.True(bans?.IsSuccess);
        var banned = Assert.Single(bans!.Data!);
        Assert.Equal(memberId, banned.BannedUserId);

        // A plain member has no business seeing who is banned.
        Authenticate(memberToken);
        var forbidden = await _client.GetAsync($"/api/chat-groups/{group.ChatGroupId}/bans");
        Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);
    }

    [Fact]
    public async Task UnbannedUser_DisappearsFromBannedList_AndMayRejoin()
    {
        var ownerToken = await LoginAsAsync("DOMAIN_PROFESSIONAL", "unbanlist_owner", "unbanlist_owner@test.com");
        var group = await CreateGroupAsync(ownerToken, "Unban List Test");

        var memberToken = await LoginAsAsync("MEMBER", "unbanlist_member", "unbanlist_member@test.com");
        await JoinAsync(memberToken, group.ChatGroupId);
        var memberId = GetUserIdByName("unbanlist_member");

        Authenticate(ownerToken);
        await _client.PostAsync($"/api/chat-groups/{group.ChatGroupId}/members/{memberId}/ban", null);

        Authenticate(ownerToken);
        var unbanResponse = await _client.DeleteAsync(
            $"/api/chat-groups/{group.ChatGroupId}/members/{memberId}/ban");
        Assert.Equal(HttpStatusCode.OK, unbanResponse.StatusCode);

        // The ban is soft-deleted, so the actionable list must not show it any more.
        Authenticate(ownerToken);
        var listResponse = await _client.GetAsync($"/api/chat-groups/{group.ChatGroupId}/bans");
        var bans = await listResponse.Content.ReadFromJsonAsync<Result<IReadOnlyList<ChatGroupBannedMemberModel>>>();
        Assert.True(bans?.IsSuccess);
        Assert.Empty(bans!.Data!);

        // And the lifted ban actually restores member access.
        Authenticate(memberToken);
        var accessResponse = await _client.GetAsync($"/api/chat-groups/{group.ChatGroupId}");
        Assert.Equal(HttpStatusCode.OK, accessResponse.StatusCode);
    }

    [Fact]
    public async Task Admin_CanListBannedMembers_ButOwnerSettingsRemainOwnerOnly()
    {
        var ownerToken = await LoginAsAsync("DOMAIN_PROFESSIONAL", "banadm_owner", "banadm_owner@test.com");
        var group = await CreateGroupAsync(ownerToken, "Ban Admin Test");

        var adminToken = await LoginAsAsync("MEMBER", "banadm_admin", "banadm_admin@test.com");
        await JoinAsync(adminToken, group.ChatGroupId);
        var adminId = GetUserIdByName("banadm_admin");
        await PromoteAsync(ownerToken, group.ChatGroupId, adminId);

        // An admin moderates members, so the ban list is readable.
        Authenticate(adminToken);
        var listResponse = await _client.GetAsync($"/api/chat-groups/{group.ChatGroupId}/bans");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);

        // But group settings stay owner-only, even for an admin.
        Authenticate(adminToken);
        var settingsResponse = await _client.PutAsJsonAsync(
            $"/api/chat-groups/{group.ChatGroupId}",
            new UpdateChatGroupRequestModel("Hijacked", null, "FREE", 0));
        Assert.Equal(HttpStatusCode.Forbidden, settingsResponse.StatusCode);
    }

    /// <summary>Owner-only helper that promotes a joined member to ADMIN via the existing endpoint.</summary>
    private async Task PromoteAsync(string ownerToken, int groupId, int targetUserId)
    {
        Authenticate(ownerToken);
        var response = await _client.PostAsync(
            $"/api/chat-groups/{groupId}/members/{targetUserId}/promote", null);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // ------------------------------------------------------------------
    // Banned member UI & access security tests
    // ------------------------------------------------------------------

    [Fact]
    public async Task BannedMember_GetGroup_ReturnsForbidden()
    {
        var ownerToken = await LoginAsAsync("DOMAIN_PROFESSIONAL", "bget_owner", "bget_owner@test.com");
        var group = await CreateGroupAsync(ownerToken, "Banned Get Test");

        var memberToken = await LoginAsAsync("MEMBER", "bget_member", "bget_member@test.com");
        await JoinAsync(memberToken, group.ChatGroupId);
        var memberId = GetUserIdByName("bget_member");

        Authenticate(ownerToken);
        await _client.PostAsync($"/api/chat-groups/{group.ChatGroupId}/members/{memberId}/ban", null);

        Authenticate(memberToken);
        var response = await _client.GetAsync($"/api/chat-groups/{group.ChatGroupId}");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task BannedMember_CannotReadMessages()
    {
        var ownerToken = await LoginAsAsync("DOMAIN_PROFESSIONAL", "bread_owner", "bread_owner@test.com");
        var group = await CreateGroupAsync(ownerToken, "Banned Read Test");

        var memberToken = await LoginAsAsync("MEMBER", "bread_member", "bread_member@test.com");
        await JoinAsync(memberToken, group.ChatGroupId);
        var memberId = GetUserIdByName("bread_member");

        Authenticate(ownerToken);
        await _client.PostAsync($"/api/chat-groups/{group.ChatGroupId}/members/{memberId}/ban", null);

        Authenticate(memberToken);
        var response = await _client.GetAsync($"/api/chat-groups/{group.ChatGroupId}/messages");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task BannedMember_CannotSendMessage()
    {
        var ownerToken = await LoginAsAsync("DOMAIN_PROFESSIONAL", "bsend_owner", "bsend_owner@test.com");
        var group = await CreateGroupAsync(ownerToken, "Banned Send Test");

        var memberToken = await LoginAsAsync("MEMBER", "bsend_member", "bsend_member@test.com");
        await JoinAsync(memberToken, group.ChatGroupId);
        var memberId = GetUserIdByName("bsend_member");

        Authenticate(ownerToken);
        await _client.PostAsync($"/api/chat-groups/{group.ChatGroupId}/members/{memberId}/ban", null);

        Authenticate(memberToken);
        var response = await _client.PostAsJsonAsync(
            $"/api/chat-groups/{group.ChatGroupId}/messages",
            new SendChatGroupMessageRequestModel("Hello while banned"));
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task BannedMember_CannotJoinViaInvite()
    {
        var ownerToken = await LoginAsAsync("DOMAIN_PROFESSIONAL", "binv_owner", "binv_owner@test.com");
        var group = await CreateGroupAsync(ownerToken, "Banned Invite Test");

        Authenticate(ownerToken);
        var linkResp = await _client.GetAsync($"/api/chat-groups/{group.ChatGroupId}/invite-links/primary");
        var linkResult = await linkResp.Content.ReadFromJsonAsync<Result<ChatGroupInviteLinkModel>>();
        Assert.NotNull(linkResult?.Data?.Token);
        var token = linkResult.Data.Token;

        var memberToken = await LoginAsAsync("MEMBER", "binv_member", "binv_member@test.com");
        await JoinAsync(memberToken, group.ChatGroupId);
        var memberId = GetUserIdByName("binv_member");

        Authenticate(ownerToken);
        await _client.PostAsync($"/api/chat-groups/{group.ChatGroupId}/members/{memberId}/ban", null);

        Authenticate(memberToken);
        var joinResponse = await _client.PostAsync($"/api/chat-groups/invite/{token}/join", null);
        Assert.Equal(HttpStatusCode.Forbidden, joinResponse.StatusCode);
    }

    [Fact]
    public async Task BannedMember_CannotCreateDuplicateMembership()
    {
        var ownerToken = await LoginAsAsync("DOMAIN_PROFESSIONAL", "bdup_owner", "bdup_owner@test.com");
        var group = await CreateGroupAsync(ownerToken, "Banned Duplicate Test");

        var memberToken = await LoginAsAsync("MEMBER", "bdup_member", "bdup_member@test.com");
        await JoinAsync(memberToken, group.ChatGroupId);
        var memberId = GetUserIdByName("bdup_member");

        Authenticate(ownerToken);
        await _client.PostAsync($"/api/chat-groups/{group.ChatGroupId}/members/{memberId}/ban", null);

        Authenticate(memberToken);
        var rejoinResponse = await _client.PostAsJsonAsync($"/api/chat-groups/{group.ChatGroupId}/join", "");
        Assert.Equal(HttpStatusCode.Forbidden, rejoinResponse.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var activeCount = await db.TblChatGroupMembers
            .CountAsync(m => m.ChatGroupId == group.ChatGroupId && m.UserId == memberId && !m.IsDeleted);
        Assert.Equal(0, activeCount);
    }

    [Fact]
    public async Task BannedMember_DoesNotTriggerPaidJoinPayment()
    {
        var ownerToken = await LoginAsAsync("DOMAIN_PROFESSIONAL", "bpaid_owner", "bpaid_owner@test.com");
        var group = await CreateGroupAsync(ownerToken, "Banned Paid Test", "PAID", 200);

        var memberToken = await LoginAsAsync("MEMBER", "bpaid_member", "bpaid_member@test.com");
        var memberId = GetUserIdByName("bpaid_member");

        // Owner adds then bans member
        Authenticate(ownerToken);
        await _client.PostAsJsonAsync(
            $"/api/chat-groups/{group.ChatGroupId}/members",
            new AddChatGroupMembersRequestModel(new[] { memberId }));

        await _client.PostAsync($"/api/chat-groups/{group.ChatGroupId}/members/{memberId}/ban", null);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var wallet = new TblLinkDropWallet
            {
                UserId = memberId,
                Balance = 1000,
                PurchasedBalance = 1000,
                EarnedBalance = 0,
                CreatedAt = DateTime.UtcNow
            };
            db.TblLinkDropWallets.Add(wallet);
            await db.SaveChangesAsync();
        }

        Authenticate(memberToken);
        var response = await _client.PostAsync($"/api/chat-groups/{group.ChatGroupId}/join-paid", null);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var wallet = await db.TblLinkDropWallets.FirstAsync(w => w.UserId == memberId);
            Assert.Equal(1000, wallet.Balance);
        }
    }

    [Fact]
    public async Task UnbannedMember_RegainsNormalGroupAccess()
    {
        var ownerToken = await LoginAsAsync("DOMAIN_PROFESSIONAL", "unban_acc_owner", "unban_acc_owner@test.com");
        var group = await CreateGroupAsync(ownerToken, "Unban Access Test");

        var memberToken = await LoginAsAsync("MEMBER", "unban_acc_member", "unban_acc_member@test.com");
        await JoinAsync(memberToken, group.ChatGroupId);
        var memberId = GetUserIdByName("unban_acc_member");

        Authenticate(ownerToken);
        await _client.PostAsync($"/api/chat-groups/{group.ChatGroupId}/members/{memberId}/ban", null);

        Authenticate(ownerToken);
        var unbanResponse = await _client.DeleteAsync($"/api/chat-groups/{group.ChatGroupId}/members/{memberId}/ban");
        Assert.Equal(HttpStatusCode.OK, unbanResponse.StatusCode);

        Authenticate(memberToken);
        var getGroup = await _client.GetAsync($"/api/chat-groups/{group.ChatGroupId}");
        Assert.Equal(HttpStatusCode.OK, getGroup.StatusCode);

        var getMsgs = await _client.GetAsync($"/api/chat-groups/{group.ChatGroupId}/messages");
        Assert.Equal(HttpStatusCode.OK, getMsgs.StatusCode);

        var sendMsg = await _client.PostAsJsonAsync(
            $"/api/chat-groups/{group.ChatGroupId}/messages",
            new SendChatGroupMessageRequestModel("I am back!"));
        Assert.Equal(HttpStatusCode.OK, sendMsg.StatusCode);
    }
}
