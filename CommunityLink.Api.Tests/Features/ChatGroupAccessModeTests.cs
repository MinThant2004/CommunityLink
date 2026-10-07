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

public class ChatGroupAccessModeTests : IClassFixture<CommunityApiFactory>
{
    private readonly CommunityApiFactory _factory;
    private readonly HttpClient _client;

    public ChatGroupAccessModeTests(CommunityApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<(string Token, int UserId)> GetTokenAndUserIdAsync(string roleCode, string username, string email, string password)
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

            db.TblUserRoles.Add(new TblUserRole
            {
                UserId = user.UserId,
                RoleId = role.RoleId,
                CreatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }

        var loginRes = await _client.PostAsJsonAsync("api/auth/login", new LoginRequestModel(email, password));
        loginRes.EnsureSuccessStatusCode();
        var content = await loginRes.Content.ReadFromJsonAsync<Result<LoginResponseModel>>();
        return (content!.Data!.AccessToken, user.UserId);
    }

    private async Task EnsureWalletBalanceAsync(int userId, long amount)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var wallet = await db.TblLinkDropWallets.FirstOrDefaultAsync(w => w.UserId == userId);
        if (wallet == null)
        {
            wallet = new TblLinkDropWallet
            {
                UserId = userId,
                PurchasedBalance = amount,
                EarnedBalance = 0,
                Balance = amount,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };
            db.TblLinkDropWallets.Add(wallet);
        }
        else
        {
            wallet.PurchasedBalance = Math.Max(wallet.PurchasedBalance, amount);
            wallet.Balance = wallet.PurchasedBalance + wallet.EarnedBalance;
            wallet.UpdatedAt = DateTime.UtcNow;
        }
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task PublicFreeGroup_JoinImmediately()
    {
        var (ownerToken, _) = await GetTokenAndUserIdAsync("DOMAIN_PROFESSIONAL", "pubfree_owner", "pubfree_owner@test.com", "Pass123!");
        var (userToken, userId) = await GetTokenAndUserIdAsync("MEMBER", "pubfree_user", "pubfree_user@test.com", "Pass123!");

        // 1. Create PUBLIC + FREE group
        var createReq = new HttpRequestMessage(HttpMethod.Post, "api/chat-groups");
        createReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ownerToken);
        createReq.Content = JsonContent.Create(new CreateChatGroupRequestModel("Public Free Test", "Desc", null, "FREE", 0, "PUBLIC"));
        var createRes = await _client.SendAsync(createReq);
        createRes.EnsureSuccessStatusCode();
        var group = (await createRes.Content.ReadFromJsonAsync<Result<ChatGroupModel>>())!.Data!;

        // 2. User joins immediately
        var joinReq = new HttpRequestMessage(HttpMethod.Post, $"api/chat-groups/{group.ChatGroupId}/join");
        joinReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", userToken);
        var joinRes = await _client.SendAsync(joinReq);
        Assert.Equal(HttpStatusCode.OK, joinRes.StatusCode);

        // 3. Verify membership in DB
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var isMember = await db.TblChatGroupMembers.AnyAsync(m => m.ChatGroupId == group.ChatGroupId && m.UserId == userId && !m.IsDeleted);
        Assert.True(isMember);
    }

    [Fact]
    public async Task PublicPaidGroup_PaymentThenJoin()
    {
        var (ownerToken, _) = await GetTokenAndUserIdAsync("DOMAIN_PROFESSIONAL", "pubpaid_owner", "pubpaid_owner@test.com", "Pass123!");
        var (userToken, userId) = await GetTokenAndUserIdAsync("MEMBER", "pubpaid_user", "pubpaid_user@test.com", "Pass123!");
        await EnsureWalletBalanceAsync(userId, 500);

        // 1. Create PUBLIC + PAID group
        var createReq = new HttpRequestMessage(HttpMethod.Post, "api/chat-groups");
        createReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ownerToken);
        createReq.Content = JsonContent.Create(new CreateChatGroupRequestModel("Public Paid Test", "Desc", null, "PAID", 100, "PUBLIC"));
        var createRes = await _client.SendAsync(createReq);
        createRes.EnsureSuccessStatusCode();
        var group = (await createRes.Content.ReadFromJsonAsync<Result<ChatGroupModel>>())!.Data!;

        // 2. Pay and join
        var payReq = new HttpRequestMessage(HttpMethod.Post, $"api/chat-groups/{group.ChatGroupId}/join-paid");
        payReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", userToken);
        var payRes = await _client.SendAsync(payReq);
        Assert.Equal(HttpStatusCode.OK, payRes.StatusCode);

        // 3. Verify membership
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var isMember = await db.TblChatGroupMembers.AnyAsync(m => m.ChatGroupId == group.ChatGroupId && m.UserId == userId && !m.IsDeleted);
        Assert.True(isMember);
    }

    [Fact]
    public async Task PrivateFreeGroup_RequestThenApproveThenJoin()
    {
        var (ownerToken, _) = await GetTokenAndUserIdAsync("DOMAIN_PROFESSIONAL", "privfree_owner", "privfree_owner@test.com", "Pass123!");
        var (userToken, userId) = await GetTokenAndUserIdAsync("MEMBER", "privfree_user", "privfree_user@test.com", "Pass123!");

        // 1. Create PRIVATE + FREE group
        var createReq = new HttpRequestMessage(HttpMethod.Post, "api/chat-groups");
        createReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ownerToken);
        createReq.Content = JsonContent.Create(new CreateChatGroupRequestModel("Private Free Test", "Desc", null, "FREE", 0, "PRIVATE"));
        var createRes = await _client.SendAsync(createReq);
        createRes.EnsureSuccessStatusCode();
        var group = (await createRes.Content.ReadFromJsonAsync<Result<ChatGroupModel>>())!.Data!;

        // 2. Direct join is blocked for PRIVATE group
        var directReq = new HttpRequestMessage(HttpMethod.Post, $"api/chat-groups/{group.ChatGroupId}/join");
        directReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", userToken);
        var directRes = await _client.SendAsync(directReq);
        Assert.Equal(HttpStatusCode.BadRequest, directRes.StatusCode);

        // 3. Submit join request
        var subReq = new HttpRequestMessage(HttpMethod.Post, $"api/chat-groups/{group.ChatGroupId}/join-requests");
        subReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", userToken);
        subReq.Content = JsonContent.Create(new SubmitJoinRequestModel("Please let me join"));
        var subRes = await _client.SendAsync(subReq);
        Assert.Equal(HttpStatusCode.OK, subRes.StatusCode);

        // 4. Owner fetches join requests
        var getReqs = new HttpRequestMessage(HttpMethod.Get, $"api/chat-groups/{group.ChatGroupId}/join-requests");
        getReqs.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ownerToken);
        var getRes = await _client.SendAsync(getReqs);
        var reqs = (await getRes.Content.ReadFromJsonAsync<Result<IReadOnlyList<ChatGroupJoinRequestModel>>>())!.Data!;
        Assert.Single(reqs);
        Assert.Equal("PENDING_APPROVAL", reqs[0].Status);

        // 5. Owner approves
        var appReq = new HttpRequestMessage(HttpMethod.Post, $"api/chat-groups/{group.ChatGroupId}/join-requests/{reqs[0].ChatGroupJoinRequestId}/approve");
        appReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ownerToken);
        var appRes = await _client.SendAsync(appReq);
        Assert.Equal(HttpStatusCode.OK, appRes.StatusCode);

        // 6. Verify membership created
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var isMember = await db.TblChatGroupMembers.AnyAsync(m => m.ChatGroupId == group.ChatGroupId && m.UserId == userId && !m.IsDeleted);
        Assert.True(isMember);
    }

    [Fact]
    public async Task PrivateFreeGroup_RejectDoesNotCreateMembership()
    {
        var (ownerToken, _) = await GetTokenAndUserIdAsync("DOMAIN_PROFESSIONAL", "privrej_owner", "privrej_owner@test.com", "Pass123!");
        var (userToken, userId) = await GetTokenAndUserIdAsync("MEMBER", "privrej_user", "privrej_user@test.com", "Pass123!");

        // 1. Create PRIVATE + FREE group
        var createReq = new HttpRequestMessage(HttpMethod.Post, "api/chat-groups");
        createReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ownerToken);
        createReq.Content = JsonContent.Create(new CreateChatGroupRequestModel("Private Rej Test", "Desc", null, "FREE", 0, "PRIVATE"));
        var createRes = await _client.SendAsync(createReq);
        var group = (await createRes.Content.ReadFromJsonAsync<Result<ChatGroupModel>>())!.Data!;

        // 2. Submit request
        var subReq = new HttpRequestMessage(HttpMethod.Post, $"api/chat-groups/{group.ChatGroupId}/join-requests");
        subReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", userToken);
        await _client.SendAsync(subReq);

        // 3. Owner rejects
        var getReqs = new HttpRequestMessage(HttpMethod.Get, $"api/chat-groups/{group.ChatGroupId}/join-requests");
        getReqs.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ownerToken);
        var getRes = await _client.SendAsync(getReqs);
        var reqs = (await getRes.Content.ReadFromJsonAsync<Result<IReadOnlyList<ChatGroupJoinRequestModel>>>())!.Data!;

        var rejReq = new HttpRequestMessage(HttpMethod.Post, $"api/chat-groups/{group.ChatGroupId}/join-requests/{reqs[0].ChatGroupJoinRequestId}/reject");
        rejReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ownerToken);
        var rejRes = await _client.SendAsync(rejReq);
        Assert.Equal(HttpStatusCode.OK, rejRes.StatusCode);

        // 4. Verify status is REJECTED and NO membership
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var isMember = await db.TblChatGroupMembers.AnyAsync(m => m.ChatGroupId == group.ChatGroupId && m.UserId == userId && !m.IsDeleted);
        Assert.False(isMember);

        var reqDb = await db.TblChatGroupJoinRequests.FirstOrDefaultAsync(r => r.ChatGroupJoinRequestId == reqs[0].ChatGroupJoinRequestId);
        Assert.Equal("REJECTED", reqDb!.Status);
    }

    [Fact]
    public async Task PrivatePaidGroup_FullApprovalAndPaymentFlow()
    {
        var (ownerToken, _) = await GetTokenAndUserIdAsync("DOMAIN_PROFESSIONAL", "privpaid_owner", "privpaid_owner@test.com", "Pass123!");
        var (userToken, userId) = await GetTokenAndUserIdAsync("MEMBER", "privpaid_user", "privpaid_user@test.com", "Pass123!");
        await EnsureWalletBalanceAsync(userId, 500);

        // 1. Create PRIVATE + PAID group (100 fee)
        var createReq = new HttpRequestMessage(HttpMethod.Post, "api/chat-groups");
        createReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ownerToken);
        createReq.Content = JsonContent.Create(new CreateChatGroupRequestModel("Private Paid Flow", "Desc", null, "PAID", 100, "PRIVATE"));
        var createRes = await _client.SendAsync(createReq);
        var group = (await createRes.Content.ReadFromJsonAsync<Result<ChatGroupModel>>())!.Data!;

        // 2. Submit request -> balance NOT deducted
        var subReq = new HttpRequestMessage(HttpMethod.Post, $"api/chat-groups/{group.ChatGroupId}/join-requests");
        subReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", userToken);
        var subRes = await _client.SendAsync(subReq);
        Assert.Equal(HttpStatusCode.OK, subRes.StatusCode);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var wallet = await db.TblLinkDropWallets.FirstOrDefaultAsync(w => w.UserId == userId);
            Assert.Equal(500, wallet!.Balance); // No deduction
        }

        // 3. Owner approves -> status becomes APPROVED_WAITING_PAYMENT, balance NOT deducted
        var getReqs = new HttpRequestMessage(HttpMethod.Get, $"api/chat-groups/{group.ChatGroupId}/join-requests");
        getReqs.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ownerToken);
        var getRes = await _client.SendAsync(getReqs);
        var reqs = (await getRes.Content.ReadFromJsonAsync<Result<IReadOnlyList<ChatGroupJoinRequestModel>>>())!.Data!;

        var appReq = new HttpRequestMessage(HttpMethod.Post, $"api/chat-groups/{group.ChatGroupId}/join-requests/{reqs[0].ChatGroupJoinRequestId}/approve");
        appReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ownerToken);
        var appRes = await _client.SendAsync(appReq);
        Assert.Equal(HttpStatusCode.OK, appRes.StatusCode);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var wallet = await db.TblLinkDropWallets.FirstOrDefaultAsync(w => w.UserId == userId);
            Assert.Equal(500, wallet!.Balance); // Still no deduction

            var reqDb = await db.TblChatGroupJoinRequests.FirstOrDefaultAsync(r => r.ChatGroupJoinRequestId == reqs[0].ChatGroupJoinRequestId);
            Assert.Equal("APPROVED_WAITING_PAYMENT", reqDb!.Status);
        }

        // 4. User calls PayAndJoin -> balance deducted (400 remaining), membership created
        var payReq = new HttpRequestMessage(HttpMethod.Post, $"api/chat-groups/{group.ChatGroupId}/pay-and-join");
        payReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", userToken);
        var payRes = await _client.SendAsync(payReq);
        Assert.Equal(HttpStatusCode.OK, payRes.StatusCode);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var wallet = await db.TblLinkDropWallets.FirstOrDefaultAsync(w => w.UserId == userId);
            Assert.Equal(400, wallet!.Balance); // 100 deducted

            var isMember = await db.TblChatGroupMembers.AnyAsync(m => m.ChatGroupId == group.ChatGroupId && m.UserId == userId && !m.IsDeleted);
            Assert.True(isMember);

            var reqDb = await db.TblChatGroupJoinRequests.FirstOrDefaultAsync(r => r.ChatGroupJoinRequestId == reqs[0].ChatGroupJoinRequestId);
            Assert.Equal("JOINED", reqDb!.Status);
        }
    }

    [Fact]
    public async Task PrivatePaidGroup_DirectPaymentWithoutApproval_IsBlocked()
    {
        var (ownerToken, _) = await GetTokenAndUserIdAsync("DOMAIN_PROFESSIONAL", "privdirect_owner", "privdirect_owner@test.com", "Pass123!");
        var (userToken, userId) = await GetTokenAndUserIdAsync("MEMBER", "privdirect_user", "privdirect_user@test.com", "Pass123!");
        await EnsureWalletBalanceAsync(userId, 500);

        // 1. Create PRIVATE + PAID group
        var createReq = new HttpRequestMessage(HttpMethod.Post, "api/chat-groups");
        createReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ownerToken);
        createReq.Content = JsonContent.Create(new CreateChatGroupRequestModel("Private Paid Direct Block", "Desc", null, "PAID", 100, "PRIVATE"));
        var createRes = await _client.SendAsync(createReq);
        var group = (await createRes.Content.ReadFromJsonAsync<Result<ChatGroupModel>>())!.Data!;

        // 2. Direct payment attempt WITHOUT approval -> BadRequest
        var payReq = new HttpRequestMessage(HttpMethod.Post, $"api/chat-groups/{group.ChatGroupId}/join-paid");
        payReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", userToken);
        var payRes = await _client.SendAsync(payReq);
        Assert.Equal(HttpStatusCode.BadRequest, payRes.StatusCode);

        // 3. Verify balance unchanged and not a member
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var wallet = await db.TblLinkDropWallets.FirstOrDefaultAsync(w => w.UserId == userId);
        Assert.Equal(500, wallet!.Balance);

        var isMember = await db.TblChatGroupMembers.AnyAsync(m => m.ChatGroupId == group.ChatGroupId && m.UserId == userId && !m.IsDeleted);
        Assert.False(isMember);
    }

    [Fact]
    public async Task NonAdminCannotApproveOrRejectJoinRequest()
    {
        var (ownerToken, _) = await GetTokenAndUserIdAsync("DOMAIN_PROFESSIONAL", "na_owner", "na_owner@test.com", "Pass123!");
        var (userToken, _) = await GetTokenAndUserIdAsync("MEMBER", "na_user", "na_user@test.com", "Pass123!");
        var (otherToken, _) = await GetTokenAndUserIdAsync("MEMBER", "na_other", "na_other@test.com", "Pass123!");

        // 1. Create PRIVATE group
        var createReq = new HttpRequestMessage(HttpMethod.Post, "api/chat-groups");
        createReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ownerToken);
        createReq.Content = JsonContent.Create(new CreateChatGroupRequestModel("NonAdmin Test", "Desc", null, "FREE", 0, "PRIVATE"));
        var createRes = await _client.SendAsync(createReq);
        var group = (await createRes.Content.ReadFromJsonAsync<Result<ChatGroupModel>>())!.Data!;

        // 2. User submits request
        var subReq = new HttpRequestMessage(HttpMethod.Post, $"api/chat-groups/{group.ChatGroupId}/join-requests");
        subReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", userToken);
        await _client.SendAsync(subReq);

        // 3. Other user tries to get join requests -> Forbidden
        var getReqs = new HttpRequestMessage(HttpMethod.Get, $"api/chat-groups/{group.ChatGroupId}/join-requests");
        getReqs.Headers.Authorization = new AuthenticationHeaderValue("Bearer", otherToken);
        var getRes = await _client.SendAsync(getReqs);
        Assert.Equal(HttpStatusCode.Forbidden, getRes.StatusCode);
    }

    [Fact]
    public async Task AccessMode_CanBeUpdatedByOwner()
    {
        var (ownerToken, _) = await GetTokenAndUserIdAsync("DOMAIN_PROFESSIONAL", "am_owner", "am_owner@test.com", "Pass123!");

        // 1. Create PUBLIC group
        var createReq = new HttpRequestMessage(HttpMethod.Post, "api/chat-groups");
        createReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ownerToken);
        createReq.Content = JsonContent.Create(new CreateChatGroupRequestModel("AM Group", "Desc", null, "FREE", 0, "PUBLIC"));
        var createRes = await _client.SendAsync(createReq);
        var group = (await createRes.Content.ReadFromJsonAsync<Result<ChatGroupModel>>())!.Data!;

        // 2. Owner updates to PRIVATE
        var updateReq = new HttpRequestMessage(HttpMethod.Put, $"api/chat-groups/{group.ChatGroupId}");
        updateReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ownerToken);
        updateReq.Content = JsonContent.Create(new UpdateChatGroupRequestModel("AM Group", "Desc", "FREE", 0, "PRIVATE"));
        var updateRes = await _client.SendAsync(updateReq);
        Assert.Equal(HttpStatusCode.OK, updateRes.StatusCode);
        var updated = (await updateRes.Content.ReadFromJsonAsync<Result<ChatGroupModel>>())!.Data!;
        Assert.Equal("PRIVATE", updated.AccessMode);
    }
}
