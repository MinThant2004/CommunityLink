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

/// <summary>
/// Covers the paid-group invitation flow: a direct add to a PAID group must produce a PENDING
/// invite rather than a free membership, and only the payment may convert it.
/// </summary>
public class ChatGroupInviteTests : IClassFixture<CommunityApiFactory>
{
    private readonly CommunityApiFactory _factory;
    private readonly HttpClient _client;

    public ChatGroupInviteTests(CommunityApiFactory factory)
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

    private async Task SetupWalletAsync(int userId, long purchasedBalance, long earnedBalance)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var wallet = await db.TblLinkDropWallets.FirstOrDefaultAsync(w => w.UserId == userId);
        if (wallet == null)
        {
            db.TblLinkDropWallets.Add(new TblLinkDropWallet
            {
                UserId = userId,
                PurchasedBalance = purchasedBalance,
                EarnedBalance = earnedBalance,
                Balance = purchasedBalance + earnedBalance,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            });
        }
        else
        {
            wallet.PurchasedBalance = purchasedBalance;
            wallet.EarnedBalance = earnedBalance;
            wallet.Balance = purchasedBalance + earnedBalance;
            wallet.UpdatedAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync();
    }

    private async Task SetCommissionPercentageAsync(decimal percentage)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var setting = await db.TblPlatformSettings.FirstOrDefaultAsync(s => s.SettingKey == "PlatformCommissionPercentage");
        if (setting == null)
        {
            db.TblPlatformSettings.Add(new TblPlatformSetting
            {
                SettingKey = "PlatformCommissionPercentage",
                SettingValue = percentage.ToString(System.Globalization.CultureInfo.InvariantCulture),
                DataType = "DECIMAL",
                Description = "Platform commission percentage",
                UpdatedAt = DateTime.UtcNow
            });
        }
        else
        {
            setting.SettingValue = percentage.ToString(System.Globalization.CultureInfo.InvariantCulture);
            setting.UpdatedAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync();
    }

    private void UseToken(string token) =>
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

    private async Task<int> CreateGroupAsync(string token, string name, string chatType, long fee)
    {
        UseToken(token);
        var resp = await _client.PostAsJsonAsync("/api/chat-groups", new CreateChatGroupRequestModel(name, "Invite test group", null, chatType, fee));
        var result = await resp.Content.ReadFromJsonAsync<Result<ChatGroupModel>>();
        Assert.NotNull(result?.Data);
        return result.Data.ChatGroupId;
    }

    private async Task<Result> AddMembersAsync(string token, int groupId, params int[] userIds)
    {
        UseToken(token);
        var resp = await _client.PostAsJsonAsync(
            $"/api/chat-groups/{groupId}/members",
            new AddChatGroupMembersRequestModel(userIds.ToList()));
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var result = await resp.Content.ReadFromJsonAsync<Result>();
        Assert.NotNull(result);
        return result;
    }

    [Fact]
    public async Task Owner_AddMember_ToPaidGroup_CreatesPendingInvite_AndNoMembership()
    {
        var (creatorToken, _) = await GetTokenAndUserIdForRoleAsync("DOMAIN_PROFESSIONAL", "inv_creator1", "inv_creator1@test.com", "Password@123");
        var (_, inviteeId) = await GetTokenAndUserIdForRoleAsync("MEMBER", "inv_user1", "inv_user1@test.com", "Password@123");

        var groupId = await CreateGroupAsync(creatorToken, "Invite Paid Group 1", "PAID", 75);

        var addResult = await AddMembersAsync(creatorToken, groupId, inviteeId);
        Assert.True(addResult.IsSuccess);
        Assert.Contains("must unlock it", addResult.Message);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // The whole point of the flow: no membership, so every membership-gated read stays closed.
        var membership = await db.TblChatGroupMembers
            .AnyAsync(m => m.ChatGroupId == groupId && m.UserId == inviteeId && !m.IsDeleted);
        Assert.False(membership);

        var invite = await db.TblChatGroupInvites
            .SingleAsync(i => i.ChatGroupId == groupId && i.UserId == inviteeId);
        Assert.Equal("PENDING", invite.Status);
        Assert.Equal(75, invite.FeeAtInviteLinkDrops);
        Assert.False(invite.IsDeleted);
    }

    [Fact]
    public async Task Owner_AddMember_ToFreeGroup_CreatesMembership_AndNoInvite()
    {
        var (creatorToken, _) = await GetTokenAndUserIdForRoleAsync("DOMAIN_PROFESSIONAL", "inv_creator2", "inv_creator2@test.com", "Password@123");
        var (_, memberId) = await GetTokenAndUserIdForRoleAsync("MEMBER", "inv_user2", "inv_user2@test.com", "Password@123");

        var groupId = await CreateGroupAsync(creatorToken, "Invite Free Group 2", "FREE", 0);

        var addResult = await AddMembersAsync(creatorToken, groupId, memberId);
        Assert.True(addResult.IsSuccess);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var membership = await db.TblChatGroupMembers
            .AnyAsync(m => m.ChatGroupId == groupId && m.UserId == memberId && !m.IsDeleted);
        Assert.True(membership);

        // A free group must not accumulate invitation rows.
        var inviteCount = await db.TblChatGroupInvites.CountAsync(i => i.ChatGroupId == groupId);
        Assert.Equal(0, inviteCount);
    }

    [Fact]
    public async Task Owner_AddMember_ToZeroFeePaidGroup_CreatesMembership_AndNoInvite()
    {
        // PAID with a 0 fee is a free group as far as the payment rules are concerned, so the
        // owner must still be able to add directly.
        var (creatorToken, _) = await GetTokenAndUserIdForRoleAsync("DOMAIN_PROFESSIONAL", "inv_creator3", "inv_creator3@test.com", "Password@123");
        var (_, memberId) = await GetTokenAndUserIdForRoleAsync("MEMBER", "inv_user3", "inv_user3@test.com", "Password@123");

        var groupId = await CreateGroupAsync(creatorToken, "Invite Zero Fee Group 3", "PAID", 0);

        var addResult = await AddMembersAsync(creatorToken, groupId, memberId);
        Assert.True(addResult.IsSuccess);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var membership = await db.TblChatGroupMembers
            .AnyAsync(m => m.ChatGroupId == groupId && m.UserId == memberId && !m.IsDeleted);
        Assert.True(membership);

        var inviteCount = await db.TblChatGroupInvites.CountAsync(i => i.ChatGroupId == groupId);
        Assert.Equal(0, inviteCount);
    }

    [Fact]
    public async Task Invitee_CannotRead_History_Or_Send_Until_InviteIsPaid()
    {
        var (creatorToken, _) = await GetTokenAndUserIdForRoleAsync("DOMAIN_PROFESSIONAL", "inv_creator4", "inv_creator4@test.com", "Password@123");
        var (inviteeToken, inviteeId) = await GetTokenAndUserIdForRoleAsync("MEMBER", "inv_user4", "inv_user4@test.com", "Password@123");
        await SetupWalletAsync(inviteeId, 500, 0);

        var groupId = await CreateGroupAsync(creatorToken, "Invite Gate Group 4", "PAID", 100);
        await AddMembersAsync(creatorToken, groupId, inviteeId);

        UseToken(inviteeToken);

        // The invite makes the group visible, but not the history.
        var messagesResp = await _client.GetAsync($"/api/chat-groups/{groupId}/messages");
        Assert.Equal(HttpStatusCode.Forbidden, messagesResp.StatusCode);

        var sendResp = await _client.PostAsJsonAsync(
            $"/api/chat-groups/{groupId}/messages",
            new SendChatGroupMessageRequestModel("Should not be delivered", null, "TEXT"));
        Assert.Equal(HttpStatusCode.Forbidden, sendResp.StatusCode);

        // Paying converts the invite into a membership and grants both.
        var payResp = await _client.PostAsJsonAsync($"/api/chat-groups/{groupId}/join-paid", "");
        Assert.Equal(HttpStatusCode.OK, payResp.StatusCode);

        var messagesAfter = await _client.GetAsync($"/api/chat-groups/{groupId}/messages");
        Assert.Equal(HttpStatusCode.OK, messagesAfter.StatusCode);

        var sendAfter = await _client.PostAsJsonAsync(
            $"/api/chat-groups/{groupId}/messages",
            new SendChatGroupMessageRequestModel("Now it goes through", null, "TEXT"));
        Assert.Equal(HttpStatusCode.OK, sendAfter.StatusCode);
    }

    [Fact]
    public async Task AcceptingInvite_AcceptsRow_CreatesMembership_AndChargesOnce()
    {
        await SetCommissionPercentageAsync(10.00m);

        var (creatorToken, creatorId) = await GetTokenAndUserIdForRoleAsync("DOMAIN_PROFESSIONAL", "inv_creator5", "inv_creator5@test.com", "Password@123");
        var (inviteeToken, inviteeId) = await GetTokenAndUserIdForRoleAsync("MEMBER", "inv_user5", "inv_user5@test.com", "Password@123");
        await SetupWalletAsync(inviteeId, 300, 0);

        var groupId = await CreateGroupAsync(creatorToken, "Invite Accept Group 5", "PAID", 120);
        await AddMembersAsync(creatorToken, groupId, inviteeId);

        UseToken(inviteeToken);
        var payResp = await _client.PostAsJsonAsync($"/api/chat-groups/{groupId}/join-paid", "");
        Assert.Equal(HttpStatusCode.OK, payResp.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var invite = await db.TblChatGroupInvites.SingleAsync(i => i.ChatGroupId == groupId && i.UserId == inviteeId);
        Assert.Equal("ACCEPTED", invite.Status);
        Assert.NotNull(invite.RespondedAt);
        // Retained, not soft-deleted: the owner relies on this row as the invite audit trail.
        Assert.False(invite.IsDeleted);

        var member = await db.TblChatGroupMembers
            .FirstAsync(m => m.ChatGroupId == groupId && m.UserId == inviteeId);
        Assert.Equal("MEMBER", member.Role);
        Assert.False(member.IsDeleted);

        // Billed the quoted fee, with commission split out of it.
        var inviteeWallet = await db.TblLinkDropWallets.FirstAsync(w => w.UserId == inviteeId);
        Assert.Equal(180, inviteeWallet.Balance);

        var creatorWallet = await db.TblLinkDropWallets.FirstAsync(w => w.UserId == creatorId);
        Assert.Equal(108, creatorWallet.EarnedBalance);

        var paymentTx = await db.TblChatGroupPaymentTransactions
            .FirstAsync(p => p.ChatGroupId == groupId && p.UserId == inviteeId);
        Assert.Equal(120, paymentTx.GrossAmount);
        Assert.Equal(12, paymentTx.CommissionAmount);
        Assert.Equal(108, paymentTx.NetAmount);
    }

    [Fact]
    public async Task AcceptingInvite_ChargesQuotedFee_NotFeeRaisedAfterInvite()
    {
        await SetCommissionPercentageAsync(0.00m);

        var (creatorToken, creatorId) = await GetTokenAndUserIdForRoleAsync("DOMAIN_PROFESSIONAL", "inv_creator6", "inv_creator6@test.com", "Password@123");
        var (inviteeToken, inviteeId) = await GetTokenAndUserIdForRoleAsync("MEMBER", "inv_user6", "inv_user6@test.com", "Password@123");
        await SetupWalletAsync(inviteeId, 500, 0);

        var groupId = await CreateGroupAsync(creatorToken, "Invite Reprice Group 6", "PAID", 40);
        await AddMembersAsync(creatorToken, groupId, inviteeId);

        // The owner raises the fee after the invitation was sent.
        UseToken(creatorToken);
        var feeResp = await _client.PutAsJsonAsync(
            $"/api/chat-groups/{groupId}/join-fee",
            new SetChatGroupJoinFeeRequestModel(400));
        Assert.Equal(HttpStatusCode.OK, feeResp.StatusCode);

        UseToken(inviteeToken);
        var payResp = await _client.PostAsJsonAsync($"/api/chat-groups/{groupId}/join-paid", "");
        Assert.Equal(HttpStatusCode.OK, payResp.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // 500 - 40 quoted, not 500 - 400. The invitee accepted the price they were shown.
        var inviteeWallet = await db.TblLinkDropWallets.FirstAsync(w => w.UserId == inviteeId);
        Assert.Equal(460, inviteeWallet.Balance);

        var creatorWallet = await db.TblLinkDropWallets.FirstAsync(w => w.UserId == creatorId);
        Assert.Equal(40, creatorWallet.EarnedBalance);

        var paymentTx = await db.TblChatGroupPaymentTransactions
            .FirstAsync(p => p.ChatGroupId == groupId && p.UserId == inviteeId);
        Assert.Equal(40, paymentTx.GrossAmount);
    }

    [Fact]
    public async Task InviteeWithInsufficientBalance_CannotAccept_AndInviteStaysPending()
    {
        var (creatorToken, _) = await GetTokenAndUserIdForRoleAsync("DOMAIN_PROFESSIONAL", "inv_creator7", "inv_creator7@test.com", "Password@123");
        var (inviteeToken, inviteeId) = await GetTokenAndUserIdForRoleAsync("MEMBER", "inv_user7", "inv_user7@test.com", "Password@123");
        await SetupWalletAsync(inviteeId, 10, 0);

        var groupId = await CreateGroupAsync(creatorToken, "Invite Broke Group 7", "PAID", 200);
        await AddMembersAsync(creatorToken, groupId, inviteeId);

        UseToken(inviteeToken);
        var payResp = await _client.PostAsJsonAsync($"/api/chat-groups/{groupId}/join-paid", "");
        Assert.Equal(HttpStatusCode.BadRequest, payResp.StatusCode);
        var payResult = await payResp.Content.ReadFromJsonAsync<Result>();
        Assert.Contains("Insufficient Link Drops balance", payResult?.Message);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // A failed payment must leave the invitation exactly as it was.
        var invite = await db.TblChatGroupInvites.SingleAsync(i => i.ChatGroupId == groupId && i.UserId == inviteeId);
        Assert.Equal("PENDING", invite.Status);
        Assert.Null(invite.RespondedAt);

        var membership = await db.TblChatGroupMembers
            .AnyAsync(m => m.ChatGroupId == groupId && m.UserId == inviteeId);
        Assert.False(membership);
    }

    [Fact]
    public async Task GetMyInvitations_ReturnsPendingInvite_WithQuotedFee_AndNoMembership()
    {
        var (creatorToken, _) = await GetTokenAndUserIdForRoleAsync("DOMAIN_PROFESSIONAL", "inv_creator8", "inv_creator8@test.com", "Password@123");
        var (inviteeToken, inviteeId) = await GetTokenAndUserIdForRoleAsync("MEMBER", "inv_user8", "inv_user8@test.com", "Password@123");

        var groupId = await CreateGroupAsync(creatorToken, "Invite Inbox Group 8", "PAID", 60);
        await AddMembersAsync(creatorToken, groupId, inviteeId);

        UseToken(inviteeToken);
        var resp = await _client.GetAsync("/api/chat-groups/my-invitations");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var result = await resp.Content.ReadFromJsonAsync<Result<IReadOnlyList<ChatGroupModel>>>();
        Assert.NotNull(result?.Data);

        var invitation = Assert.Single(result.Data, g => g.ChatGroupId == groupId);
        Assert.True(invitation.IsInvited);
        Assert.Equal(60, invitation.InviteFeeLinkDrops);
        Assert.Equal("inv_creator8", invitation.InvitedByName);
        // An invite is an offer, not access: the paywall has to stay up.
        Assert.False(invitation.IsJoined);
        Assert.Equal("NONE", invitation.UserRole);
    }

    [Fact]
    public async Task Invitee_CanDecline_AndGroupIsNoLongerListed()
    {
        var (creatorToken, _) = await GetTokenAndUserIdForRoleAsync("DOMAIN_PROFESSIONAL", "inv_creator9", "inv_creator9@test.com", "Password@123");
        var (inviteeToken, inviteeId) = await GetTokenAndUserIdForRoleAsync("MEMBER", "inv_user9", "inv_user9@test.com", "Password@123");

        var groupId = await CreateGroupAsync(creatorToken, "Invite Decline Group 9", "PAID", 60);
        await AddMembersAsync(creatorToken, groupId, inviteeId);

        UseToken(inviteeToken);
        var declineResp = await _client.PostAsJsonAsync($"/api/chat-groups/{groupId}/invitations/decline", "");
        Assert.Equal(HttpStatusCode.OK, declineResp.StatusCode);

        var listResp = await _client.GetAsync("/api/chat-groups/my-invitations");
        var listResult = await listResp.Content.ReadFromJsonAsync<Result<IReadOnlyList<ChatGroupModel>>>();
        Assert.NotNull(listResult?.Data);
        Assert.DoesNotContain(listResult.Data, g => g.ChatGroupId == groupId);

        // Declining does not revoke the ability to join later: the group is still self-service.
        var detailResp = await _client.GetAsync($"/api/chat-groups/{groupId}");
        var detail = await detailResp.Content.ReadFromJsonAsync<Result<ChatGroupModel>>();
        Assert.NotNull(detail?.Data);
        Assert.False(detail.Data.IsInvited);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var invite = await db.TblChatGroupInvites.SingleAsync(i => i.ChatGroupId == groupId && i.UserId == inviteeId);
        Assert.Equal("DECLINED", invite.Status);
        Assert.False(invite.IsDeleted);
    }

    [Fact]
    public async Task Owner_CanListAndRevoke_PendingInvitations_AndRevokedUserCanBeReinvited()
    {
        var (creatorToken, creatorId) = await GetTokenAndUserIdForRoleAsync("DOMAIN_PROFESSIONAL", "inv_creator10", "inv_creator10@test.com", "Password@123");
        var (_, inviteeId) = await GetTokenAndUserIdForRoleAsync("MEMBER", "inv_user10", "inv_user10@test.com", "Password@123");

        var groupId = await CreateGroupAsync(creatorToken, "Invite Revoke Group 10", "PAID", 60);
        await AddMembersAsync(creatorToken, groupId, inviteeId);

        UseToken(creatorToken);
        var listResp = await _client.GetAsync($"/api/chat-groups/{groupId}/invitations");
        Assert.Equal(HttpStatusCode.OK, listResp.StatusCode);
        var listResult = await listResp.Content.ReadFromJsonAsync<Result<IReadOnlyList<ChatGroupInviteModel>>>();
        Assert.NotNull(listResult?.Data);
        var pending = Assert.Single(listResult.Data);
        Assert.Equal(inviteeId, pending.InvitedUserId);
        Assert.Equal("inv_user10", pending.InvitedUserName);
        Assert.Equal(60, pending.FeeAtInviteLinkDrops);

        var revokeResp = await _client.DeleteAsync($"/api/chat-groups/{groupId}/invitations/{inviteeId}");
        Assert.Equal(HttpStatusCode.OK, revokeResp.StatusCode);

        var listAfter = await _client.GetAsync($"/api/chat-groups/{groupId}/invitations");
        var listAfterResult = await listAfter.Content.ReadFromJsonAsync<Result<IReadOnlyList<ChatGroupInviteModel>>>();
        Assert.NotNull(listAfterResult?.Data);
        Assert.Empty(listAfterResult.Data);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var revoked = await db.TblChatGroupInvites.SingleAsync(i => i.ChatGroupId == groupId && i.UserId == inviteeId);
        Assert.Equal("REVOKED", revoked.Status);
        Assert.Equal(creatorId, revoked.DeletedBy ?? revoked.UpdatedBy);

        // A resolved invite must not block a fresh one.
        var reInvite = await AddMembersAsync(creatorToken, groupId, inviteeId);
        Assert.True(reInvite.IsSuccess);
        Assert.Equal(2, await db.TblChatGroupInvites.CountAsync(i => i.ChatGroupId == groupId && i.UserId == inviteeId));
    }

    [Fact]
    public async Task AddMember_AsAdmin_IsRejected_AndAdminSeesNoAddControl()
    {
        var (creatorToken, _) = await GetTokenAndUserIdForRoleAsync("DOMAIN_PROFESSIONAL", "inv_creator11", "inv_creator11@test.com", "Password@123");
        var (adminToken, adminId) = await GetTokenAndUserIdForRoleAsync("MEMBER", "inv_admin11", "inv_admin11@test.com", "Password@123");
        var (_, targetId) = await GetTokenAndUserIdForRoleAsync("MEMBER", "inv_user11", "inv_user11@test.com", "Password@123");
        await SetupWalletAsync(adminId, 500, 0);

        var groupId = await CreateGroupAsync(creatorToken, "Invite Admin Group 11", "PAID", 60);

        // The future admin has to be a member before the owner can promote them, so it pays the
        // join fee like anyone else.
        UseToken(adminToken);
        var adminJoin = await _client.PostAsJsonAsync($"/api/chat-groups/{groupId}/join-paid", "");
        Assert.Equal(HttpStatusCode.OK, adminJoin.StatusCode);

        UseToken(creatorToken);
        var promoteResp = await _client.PostAsJsonAsync($"/api/chat-groups/{groupId}/members/{adminId}/promote", "");
        Assert.Equal(HttpStatusCode.OK, promoteResp.StatusCode);

        UseToken(adminToken);
        var addResp = await _client.PostAsJsonAsync(
            $"/api/chat-groups/{groupId}/members",
            new AddChatGroupMembersRequestModel(new List<int> { targetId }));
        Assert.Equal(HttpStatusCode.Forbidden, addResp.StatusCode);
        var addResult = await addResp.Content.ReadFromJsonAsync<Result>();
        Assert.NotNull(addResult);
        Assert.False(addResult.IsSuccess);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.False(await db.TblChatGroupInvites.AnyAsync(i => i.ChatGroupId == groupId && i.UserId == targetId));
        Assert.False(await db.TblChatGroupMembers.AnyAsync(m => m.ChatGroupId == groupId && m.UserId == targetId));

        // Admins may still review outstanding invitations, just not create them.
        var listResp = await _client.GetAsync($"/api/chat-groups/{groupId}/invitations");
        Assert.Equal(HttpStatusCode.OK, listResp.StatusCode);
    }

    [Fact]
    public async Task Invitee_CannotViewOrRevoke_GroupsInvitationList()
    {
        var (creatorToken, _) = await GetTokenAndUserIdForRoleAsync("DOMAIN_PROFESSIONAL", "inv_creator12", "inv_creator12@test.com", "Password@123");
        var (inviteeToken, inviteeId) = await GetTokenAndUserIdForRoleAsync("MEMBER", "inv_user12", "inv_user12@test.com", "Password@123");
        var (_, otherId) = await GetTokenAndUserIdForRoleAsync("MEMBER", "inv_user12b", "inv_user12b@test.com", "Password@123");

        var groupId = await CreateGroupAsync(creatorToken, "Invite Privacy Group 12", "PAID", 60);
        await AddMembersAsync(creatorToken, groupId, inviteeId);

        // An invitee is not a member, so the group's invitation list is not theirs to read.
        UseToken(inviteeToken);
        var listResp = await _client.GetAsync($"/api/chat-groups/{groupId}/invitations");
        Assert.Equal(HttpStatusCode.Forbidden, listResp.StatusCode);

        var revokeResp = await _client.DeleteAsync($"/api/chat-groups/{groupId}/invitations/{otherId}");
        Assert.Equal(HttpStatusCode.Forbidden, revokeResp.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var invite = await db.TblChatGroupInvites.SingleAsync(i => i.ChatGroupId == groupId && i.UserId == inviteeId);
        Assert.Equal("PENDING", invite.Status);
    }

    [Fact]
    public async Task DuplicateInvite_WhilePending_IsSkipped_NotDuplicated()
    {
        var (creatorToken, _) = await GetTokenAndUserIdForRoleAsync("DOMAIN_PROFESSIONAL", "inv_creator13", "inv_creator13@test.com", "Password@123");
        var (_, inviteeId) = await GetTokenAndUserIdForRoleAsync("MEMBER", "inv_user13", "inv_user13@test.com", "Password@123");

        var groupId = await CreateGroupAsync(creatorToken, "Invite Dup Group 13", "PAID", 60);

        var first = await AddMembersAsync(creatorToken, groupId, inviteeId);
        Assert.True(first.IsSuccess);
        Assert.Contains("Invited 1", first.Message);

        var second = await AddMembersAsync(creatorToken, groupId, inviteeId);
        Assert.True(second.IsSuccess);
        Assert.Contains("Invited 0", second.Message);
        Assert.Contains("Skipped 1", second.Message);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var inviteCount = await db.TblChatGroupInvites.CountAsync(i => i.ChatGroupId == groupId && i.UserId == inviteeId);
        Assert.Equal(1, inviteCount);
    }

    [Fact]
    public async Task InviteeOfPaidGroup_CannotJoinViaFreeJoinEndpoint()
    {
        // The invite must not be usable as a back door: the plain join route still refuses PAID
        // groups regardless of who invited the caller.
        var (creatorToken, _) = await GetTokenAndUserIdForRoleAsync("DOMAIN_PROFESSIONAL", "inv_creator14", "inv_creator14@test.com", "Password@123");
        var (inviteeToken, inviteeId) = await GetTokenAndUserIdForRoleAsync("MEMBER", "inv_user14", "inv_user14@test.com", "Password@123");
        await SetupWalletAsync(inviteeId, 500, 0);

        var groupId = await CreateGroupAsync(creatorToken, "Invite Backdoor Group 14", "PAID", 60);
        await AddMembersAsync(creatorToken, groupId, inviteeId);

        UseToken(inviteeToken);
        var joinResp = await _client.PostAsJsonAsync($"/api/chat-groups/{groupId}/join", "");
        Assert.Equal(HttpStatusCode.BadRequest, joinResp.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.False(await db.TblChatGroupMembers.AnyAsync(m => m.ChatGroupId == groupId && m.UserId == inviteeId && !m.IsDeleted));

        var wallet = await db.TblLinkDropWallets.FirstAsync(w => w.UserId == inviteeId);
        Assert.Equal(500, wallet.Balance);
    }
}
