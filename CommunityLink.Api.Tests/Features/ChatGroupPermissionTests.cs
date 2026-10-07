using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CommunityLink.Api.Tests.Infrastructure;
using CommunityLink.Database.AppDbContextModels;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Authentication;
using CommunityLink.Shared.Features.ChatGroup;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CommunityLink.Api.Tests.Features;

/// <summary>
/// Covers the per-admin permission matrix and the single pinned message: that each moderation
/// action is gated by its own flag rather than by role alone, that only the owner may edit the
/// matrix, and that pinning replaces the previous pin and survives deletion only as far as the
/// message does.
/// </summary>
public class ChatGroupPermissionTests : IClassFixture<CommunityApiFactory>
{
    private readonly CommunityApiFactory _factory;
    private readonly HttpClient _client;

    public ChatGroupPermissionTests(CommunityApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private const string Password = "Password@123";

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

    private void Authenticate(string token) =>
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

    private async Task<ChatGroupModel> CreateGroupAsync(string token, string name)
    {
        Authenticate(token);
        var response = await _client.PostAsJsonAsync(
            "/api/chat-groups",
            new CreateChatGroupRequestModel(name, "Created by test", null, "FREE", 0));
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

    private async Task PromoteAsync(string token, int groupId, int targetUserId)
    {
        Authenticate(token);
        var response = await _client.PostAsync(
            $"/api/chat-groups/{groupId}/members/{targetUserId}/promote", null);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private async Task<HttpResponseMessage> SetPermissionsAsync(
        string token, int groupId, int targetUserId, ChatGroupPermissionSet permissions)
    {
        Authenticate(token);
        return await _client.PutAsJsonAsync(
            $"/api/chat-groups/{groupId}/members/{targetUserId}/permissions", permissions);
    }

    private async Task<ChatGroupMemberModel> GetMemberAsync(string token, int groupId, int userId)
    {
        Authenticate(token);
        var response = await _client.GetAsync($"/api/chat-groups/{groupId}/members");
        var members = await response.Content.ReadFromJsonAsync<Result<IReadOnlyList<ChatGroupMemberModel>>>();
        return members!.Data!.First(m => m.UserId == userId);
    }

    private async Task<ChatGroupMessageModel> SendMessageAsync(string token, int groupId, string content)
    {
        Authenticate(token);
        var response = await _client.PostAsJsonAsync(
            $"/api/chat-groups/{groupId}/messages",
            new SendChatGroupMessageRequestModel(content));
        var result = await response.Content.ReadFromJsonAsync<Result<ChatGroupMessageModel>>();
        Assert.NotNull(result?.Data);
        return result.Data;
    }

    private async Task<HttpResponseMessage> PinAsync(string token, int groupId, int messageId)
    {
        Authenticate(token);
        return await _client.PostAsync($"/api/chat-groups/{groupId}/messages/{messageId}/pin", null);
    }

    private async Task<Result<ChatGroupPinnedMessageModel?>> GetPinnedAsync(string token, int groupId)
    {
        Authenticate(token);
        var response = await _client.GetAsync($"/api/chat-groups/{groupId}/pinned-message");
        return (await response.Content.ReadFromJsonAsync<Result<ChatGroupPinnedMessageModel?>>())!;
    }

    // ------------------------------------------------------------------
    // Permission matrix
    // ------------------------------------------------------------------

    [Fact]
    public async Task Promotion_GrantsEveryPermission()
    {
        var ownerToken = await LoginAsAsync("DOMAIN_PROFESSIONAL", "perm_prom_owner", "perm_prom_owner@test.com");
        var group = await CreateGroupAsync(ownerToken, "Promotion Grant Test");

        var adminToken = await LoginAsAsync("MEMBER", "perm_prom_admin", "perm_prom_admin@test.com");
        await JoinAsync(adminToken, group.ChatGroupId);
        var adminId = GetUserIdByName("perm_prom_admin");
        await PromoteAsync(ownerToken, group.ChatGroupId, adminId);

        var admin = await GetMemberAsync(ownerToken, group.ChatGroupId, adminId);
        Assert.Equal(ChatGroupPermissionSet.All, admin.Permissions);
    }

    [Fact]
    public async Task Owner_CanSetPermissions_OnAdmin()
    {
        var ownerToken = await LoginAsAsync("DOMAIN_PROFESSIONAL", "perm_set_owner", "perm_set_owner@test.com");
        var group = await CreateGroupAsync(ownerToken, "Set Permissions Test");

        var adminToken = await LoginAsAsync("MEMBER", "perm_set_admin", "perm_set_admin@test.com");
        await JoinAsync(adminToken, group.ChatGroupId);
        var adminId = GetUserIdByName("perm_set_admin");
        await PromoteAsync(ownerToken, group.ChatGroupId, adminId);

        var narrowed = new ChatGroupPermissionSet(CanBanMembers: true);
        var response = await SetPermissionsAsync(ownerToken, group.ChatGroupId, adminId, narrowed);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var admin = await GetMemberAsync(ownerToken, group.ChatGroupId, adminId);
        Assert.Equal(narrowed, admin.Permissions);
    }

    [Fact]
    public async Task UpdatePermissions_OnPlainMember_IsRejected()
    {
        var ownerToken = await LoginAsAsync("DOMAIN_PROFESSIONAL", "perm_plain_owner", "perm_plain_owner@test.com");
        var group = await CreateGroupAsync(ownerToken, "Plain Member Permissions Test");

        var memberToken = await LoginAsAsync("MEMBER", "perm_plain_member", "perm_plain_member@test.com");
        await JoinAsync(memberToken, group.ChatGroupId);
        var memberId = GetUserIdByName("perm_plain_member");

        // The matrix only means something for an ADMIN, so writing it for a MEMBER is refused rather
        // than silently stored and forgotten.
        var response = await SetPermissionsAsync(ownerToken, group.ChatGroupId, memberId, ChatGroupPermissionSet.All);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Admin_CannotEdit_Permissions()
    {
        var ownerToken = await LoginAsAsync("DOMAIN_PROFESSIONAL", "perm_edit_owner", "perm_edit_owner@test.com");
        var group = await CreateGroupAsync(ownerToken, "Admin Edit Permissions Test");

        var adminToken = await LoginAsAsync("MEMBER", "perm_edit_admin", "perm_edit_admin@test.com");
        await JoinAsync(adminToken, group.ChatGroupId);
        var adminId = GetUserIdByName("perm_edit_admin");
        await PromoteAsync(ownerToken, group.ChatGroupId, adminId);

        var otherToken = await LoginAsAsync("MEMBER", "perm_edit_other", "perm_edit_other@test.com");
        await JoinAsync(otherToken, group.ChatGroupId);
        var otherId = GetUserIdByName("perm_edit_other");
        await PromoteAsync(ownerToken, group.ChatGroupId, otherId);

        // An admin editing the matrix could grant itself or a peer powers the owner withheld.
        var response = await SetPermissionsAsync(adminToken, group.ChatGroupId, otherId, ChatGroupPermissionSet.All);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // ------------------------------------------------------------------
    // Flag-gated moderation
    // ------------------------------------------------------------------

    [Fact]
    public async Task Admin_WithoutRemovePermission_CannotRemoveMember()
    {
        var ownerToken = await LoginAsAsync("DOMAIN_PROFESSIONAL", "grm_owner", "grm_owner@test.com");
        var group = await CreateGroupAsync(ownerToken, "Gated Remove Test");

        var adminToken = await LoginAsAsync("MEMBER", "grm_admin", "grm_admin@test.com");
        await JoinAsync(adminToken, group.ChatGroupId);
        var adminId = GetUserIdByName("grm_admin");
        await PromoteAsync(ownerToken, group.ChatGroupId, adminId);

        var plainToken = await LoginAsAsync("MEMBER", "grm_plain", "grm_plain@test.com");
        await JoinAsync(plainToken, group.ChatGroupId);
        var plainId = GetUserIdByName("grm_plain");

        // Revoke only the remove flag; the admin keeps the others.
        await SetPermissionsAsync(
            ownerToken, group.ChatGroupId, adminId,
            new ChatGroupPermissionSet(CanBanMembers: true, CanManageInviteLinks: true, CanPinMessages: true));

        Authenticate(adminToken);
        var remove = await _client.DeleteAsync($"/api/chat-groups/{group.ChatGroupId}/members/{plainId}");
        Assert.Equal(HttpStatusCode.Forbidden, remove.StatusCode);
    }

    [Fact]
    public async Task Admin_WithRemovePermission_CanRemoveMember()
    {
        var ownerToken = await LoginAsAsync("DOMAIN_PROFESSIONAL", "grm2_owner", "grm2_owner@test.com");
        var group = await CreateGroupAsync(ownerToken, "Allowed Remove Test");

        var adminToken = await LoginAsAsync("MEMBER", "grm2_admin", "grm2_admin@test.com");
        await JoinAsync(adminToken, group.ChatGroupId);
        var adminId = GetUserIdByName("grm2_admin");
        await PromoteAsync(ownerToken, group.ChatGroupId, adminId);

        var plainToken = await LoginAsAsync("MEMBER", "grm2_plain", "grm2_plain@test.com");
        await JoinAsync(plainToken, group.ChatGroupId);
        var plainId = GetUserIdByName("grm2_plain");

        Authenticate(adminToken);
        var remove = await _client.DeleteAsync($"/api/chat-groups/{group.ChatGroupId}/members/{plainId}");
        Assert.Equal(HttpStatusCode.OK, remove.StatusCode);
    }

    [Fact]
    public async Task Admin_WithoutBanPermission_CannotBanMember()
    {
        var ownerToken = await LoginAsAsync("DOMAIN_PROFESSIONAL", "gban_owner", "gban_owner@test.com");
        var group = await CreateGroupAsync(ownerToken, "Gated Ban Test");

        var adminToken = await LoginAsAsync("MEMBER", "gban_admin", "gban_admin@test.com");
        await JoinAsync(adminToken, group.ChatGroupId);
        var adminId = GetUserIdByName("gban_admin");
        await PromoteAsync(ownerToken, group.ChatGroupId, adminId);

        var plainToken = await LoginAsAsync("MEMBER", "gban_plain", "gban_plain@test.com");
        await JoinAsync(plainToken, group.ChatGroupId);
        var plainId = GetUserIdByName("gban_plain");

        await SetPermissionsAsync(
            ownerToken, group.ChatGroupId, adminId,
            new ChatGroupPermissionSet(CanRemoveMembers: true));

        Authenticate(adminToken);
        var ban = await _client.PostAsync(
            $"/api/chat-groups/{group.ChatGroupId}/members/{plainId}/ban", null);
        Assert.Equal(HttpStatusCode.Forbidden, ban.StatusCode);
    }

    [Fact]
    public async Task Admin_WithoutDeletePermission_CannotDeleteOthersMessage()
    {
        var ownerToken = await LoginAsAsync("DOMAIN_PROFESSIONAL", "gdel_owner", "gdel_owner@test.com");
        var group = await CreateGroupAsync(ownerToken, "Gated Delete Test");

        var adminToken = await LoginAsAsync("MEMBER", "gdel_admin", "gdel_admin@test.com");
        await JoinAsync(adminToken, group.ChatGroupId);
        var adminId = GetUserIdByName("gdel_admin");
        await PromoteAsync(ownerToken, group.ChatGroupId, adminId);

        var plainToken = await LoginAsAsync("MEMBER", "gdel_plain", "gdel_plain@test.com");
        await JoinAsync(plainToken, group.ChatGroupId);

        var message = await SendMessageAsync(plainToken, group.ChatGroupId, "delete me if you can");

        // Every flag except message deletion.
        await SetPermissionsAsync(
            ownerToken, group.ChatGroupId, adminId,
            new ChatGroupPermissionSet(CanBanMembers: true, CanRemoveMembers: true, CanManageInviteLinks: true, CanPinMessages: true));

        Authenticate(adminToken);
        var delete = await _client.DeleteAsync(
            $"/api/chat-groups/{group.ChatGroupId}/messages/{message.ChatGroupMessageId}");
        Assert.Equal(HttpStatusCode.Forbidden, delete.StatusCode);
    }

    // ------------------------------------------------------------------
    // Pinned message
    // ------------------------------------------------------------------

    [Fact]
    public async Task Owner_CanPin_And_GetPinned_ReturnsIt()
    {
        var ownerToken = await LoginAsAsync("DOMAIN_PROFESSIONAL", "pin_owner", "pin_owner@test.com");
        var group = await CreateGroupAsync(ownerToken, "Pin Test");

        var memberToken = await LoginAsAsync("MEMBER", "pin_member", "pin_member@test.com");
        await JoinAsync(memberToken, group.ChatGroupId);

        var message = await SendMessageAsync(memberToken, group.ChatGroupId, "important announcement");

        Authenticate(ownerToken);
        var pin = await _client.PostAsync(
            $"/api/chat-groups/{group.ChatGroupId}/messages/{message.ChatGroupMessageId}/pin", null);
        Assert.Equal(HttpStatusCode.OK, pin.StatusCode);

        var pinned = await GetPinnedAsync(ownerToken, group.ChatGroupId);
        Assert.True(pinned.IsSuccess);
        Assert.NotNull(pinned.Data);
        Assert.Equal(message.ChatGroupMessageId, pinned.Data!.ChatGroupMessageId);
        Assert.Equal(group.ChatGroupId, pinned.Data.ChatGroupId);
    }

    [Fact]
    public async Task PinningSecondMessage_ReplacesFirst()
    {
        var ownerToken = await LoginAsAsync("DOMAIN_PROFESSIONAL", "pin2_owner", "pin2_owner@test.com");
        var group = await CreateGroupAsync(ownerToken, "Replace Pin Test");

        var first = await SendMessageAsync(ownerToken, group.ChatGroupId, "first");
        var second = await SendMessageAsync(ownerToken, group.ChatGroupId, "second");

        await PinAsync(ownerToken, group.ChatGroupId, first.ChatGroupMessageId);
        await PinAsync(ownerToken, group.ChatGroupId, second.ChatGroupMessageId);

        var pinned = await GetPinnedAsync(ownerToken, group.ChatGroupId);
        Assert.Equal(second.ChatGroupMessageId, pinned.Data!.ChatGroupMessageId);
    }

    [Fact]
    public async Task Author_CanPin_OwnMessage()
    {
        var ownerToken = await LoginAsAsync("DOMAIN_PROFESSIONAL", "pinau_owner", "pinau_owner@test.com");
        var group = await CreateGroupAsync(ownerToken, "Author Pin Test");

        var memberToken = await LoginAsAsync("MEMBER", "pinau_member", "pinau_member@test.com");
        await JoinAsync(memberToken, group.ChatGroupId);

        var message = await SendMessageAsync(memberToken, group.ChatGroupId, "mine");

        // A plain member may always pin their own message even without CanPinMessages.
        var pin = await PinAsync(memberToken, group.ChatGroupId, message.ChatGroupMessageId);
        Assert.Equal(HttpStatusCode.OK, pin.StatusCode);
    }

    [Fact]
    public async Task Admin_WithoutPinPermission_CannotPin_OthersMessage()
    {
        var ownerToken = await LoginAsAsync("DOMAIN_PROFESSIONAL", "gpin_owner", "gpin_owner@test.com");
        var group = await CreateGroupAsync(ownerToken, "Gated Pin Test");

        var adminToken = await LoginAsAsync("MEMBER", "gpin_admin", "gpin_admin@test.com");
        await JoinAsync(adminToken, group.ChatGroupId);
        var adminId = GetUserIdByName("gpin_admin");
        await PromoteAsync(ownerToken, group.ChatGroupId, adminId);

        var plainToken = await LoginAsAsync("MEMBER", "gpin_plain", "gpin_plain@test.com");
        await JoinAsync(plainToken, group.ChatGroupId);

        var message = await SendMessageAsync(plainToken, group.ChatGroupId, "not the admin's");

        await SetPermissionsAsync(
            ownerToken, group.ChatGroupId, adminId,
            new ChatGroupPermissionSet(CanRemoveMembers: true));

        var pin = await PinAsync(adminToken, group.ChatGroupId, message.ChatGroupMessageId);
        Assert.Equal(HttpStatusCode.Forbidden, pin.StatusCode);
    }

    [Fact]
    public async Task Unpin_ClearsPinnedMessage()
    {
        var ownerToken = await LoginAsAsync("DOMAIN_PROFESSIONAL", "unpin_owner", "unpin_owner@test.com");
        var group = await CreateGroupAsync(ownerToken, "Unpin Test");

        var message = await SendMessageAsync(ownerToken, group.ChatGroupId, "pin then unpin");
        await PinAsync(ownerToken, group.ChatGroupId, message.ChatGroupMessageId);

        Authenticate(ownerToken);
        var unpin = await _client.DeleteAsync($"/api/chat-groups/{group.ChatGroupId}/pinned-message");
        Assert.Equal(HttpStatusCode.OK, unpin.StatusCode);

        var pinned = await GetPinnedAsync(ownerToken, group.ChatGroupId);
        Assert.True(pinned.IsSuccess);
        Assert.Null(pinned.Data);
    }

    [Fact]
    public async Task DeletedPinnedMessage_IsNoLongerReturned()
    {
        var ownerToken = await LoginAsAsync("DOMAIN_PROFESSIONAL", "delpin_owner", "delpin_owner@test.com");
        var group = await CreateGroupAsync(ownerToken, "Deleted Pin Test");

        var message = await SendMessageAsync(ownerToken, group.ChatGroupId, "temporary pin");
        await PinAsync(ownerToken, group.ChatGroupId, message.ChatGroupMessageId);

        Authenticate(ownerToken);
        var delete = await _client.DeleteAsync(
            $"/api/chat-groups/{group.ChatGroupId}/messages/{message.ChatGroupMessageId}");
        Assert.Equal(HttpStatusCode.OK, delete.StatusCode);

        // The IsDeleted filter is what retires a pin when its message is removed.
        var pinned = await GetPinnedAsync(ownerToken, group.ChatGroupId);
        Assert.Null(pinned.Data);
    }

    [Fact]
    public async Task NonMember_CannotRead_PinnedMessage()
    {
        var ownerToken = await LoginAsAsync("DOMAIN_PROFESSIONAL", "pinnm_owner", "pinnm_owner@test.com");
        var group = await CreateGroupAsync(ownerToken, "Non Member Pin Test");

        var outsiderToken = await LoginAsAsync("MEMBER", "pinnm_outsider", "pinnm_outsider@test.com");

        Authenticate(outsiderToken);
        var response = await _client.GetAsync($"/api/chat-groups/{group.ChatGroupId}/pinned-message");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
