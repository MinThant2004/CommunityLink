using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CommunityLink.Api.Tests.Infrastructure;
using CommunityLink.Database.AppDbContextModels;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Authentication;
using CommunityLink.Shared.Features.Payout;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CommunityLink.Api.Tests.Features;

public class CreatorPayoutTests : IClassFixture<CommunityApiFactory>
{
    private readonly CommunityApiFactory _factory;
    private readonly HttpClient _client;

    public CreatorPayoutTests(CommunityApiFactory factory)
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

    private async Task SetupWalletBalancesAsync(int userId, long purchasedBalance, long earnedBalance)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var wallet = await db.TblLinkDropWallets.FirstOrDefaultAsync(w => w.UserId == userId);
        if (wallet == null)
        {
            wallet = new TblLinkDropWallet
            {
                UserId = userId,
                PurchasedBalance = purchasedBalance,
                EarnedBalance = earnedBalance,
                Balance = purchasedBalance + earnedBalance,
                CreatedAt = DateTime.UtcNow
            };
            db.TblLinkDropWallets.Add(wallet);
        }
        else
        {
            wallet.PurchasedBalance = purchasedBalance;
            wallet.EarnedBalance = earnedBalance;
            wallet.Balance = purchasedBalance + earnedBalance;
        }

        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task Creator_WithEnoughBalance_CanRequestPayout()
    {
        var (creatorToken, creatorId) = await GetTokenAndUserIdForRoleAsync("DOMAIN_PROFESSIONAL", "pay_creator1", "pay_creator1@test.com", "Password@123");
        await SetupWalletBalancesAsync(creatorId, purchasedBalance: 0, earnedBalance: 100);

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", creatorToken);

        var req = new CreatePayoutRequestModel(30, "KBZPay", "U Mg Mg", "09123456789");
        var resp = await client.PostAsJsonAsync("/api/creator/payouts", req);
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

        var result = await resp.Content.ReadFromJsonAsync<Result<CreatorPayoutModel>>();
        Assert.True(result?.IsSuccess);
        Assert.NotNull(result?.Data);
        Assert.Equal(30, result.Data.AmountLinkDrops);
        Assert.Equal(3000m, result.Data.AmountMMK);
        Assert.Equal("PENDING", result.Data.Status);
    }

    [Fact]
    public async Task Creator_WithInsufficientBalance_CannotRequest()
    {
        var (creatorToken, creatorId) = await GetTokenAndUserIdForRoleAsync("DOMAIN_PROFESSIONAL", "pay_creator2", "pay_creator2@test.com", "Password@123");
        await SetupWalletBalancesAsync(creatorId, purchasedBalance: 0, earnedBalance: 20);

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", creatorToken);

        var req = new CreatePayoutRequestModel(50, "KBZPay", "U Mg Mg", "09123456789");
        var resp = await client.PostAsJsonAsync("/api/creator/payouts", req);

        var result = await resp.Content.ReadFromJsonAsync<Result<CreatorPayoutModel>>();
        Assert.False(result?.IsSuccess);
    }

    [Fact]
    public async Task CreateRequest_DoesNotReduceBalance()
    {
        var (creatorToken, creatorId) = await GetTokenAndUserIdForRoleAsync("DOMAIN_PROFESSIONAL", "pay_creator3", "pay_creator3@test.com", "Password@123");
        await SetupWalletBalancesAsync(creatorId, purchasedBalance: 0, earnedBalance: 200);

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", creatorToken);

        var req = new CreatePayoutRequestModel(50, "KBZPay", "U Mg Mg", "09123456789");
        var resp = await client.PostAsJsonAsync("/api/creator/payouts", req);
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

        // Verify wallet balance is NOT deducted on request submission
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var wallet = await db.TblLinkDropWallets.AsNoTracking().FirstAsync(w => w.UserId == creatorId);

        Assert.Equal(200, wallet.EarnedBalance);
        Assert.Equal(200, wallet.Balance);
    }

    [Fact]
    public async Task AdminApprove_ReducesEarnedBalance_And_CreatesTransaction()
    {
        var (creatorToken, creatorId) = await GetTokenAndUserIdForRoleAsync("DOMAIN_PROFESSIONAL", "pay_creator4", "pay_creator4@test.com", "Password@123");
        var (adminToken, _) = await GetTokenAndUserIdForRoleAsync("ADMIN", "admin_pay4", "admin_pay4@test.com", "Password@123");
        await SetupWalletBalancesAsync(creatorId, purchasedBalance: 0, earnedBalance: 300);

        // Creator requests payout
        var creatorClient = _factory.CreateClient();
        creatorClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", creatorToken);

        var req = new CreatePayoutRequestModel(100, "KBZPay", "U Mg Mg", "09123456789");
        var createResp = await creatorClient.PostAsJsonAsync("/api/creator/payouts", req);
        var createResult = await createResp.Content.ReadFromJsonAsync<Result<CreatorPayoutModel>>();
        long payoutId = createResult!.Data!.CreatorPayoutRequestId;

        // Admin approves payout
        var adminClient = _factory.CreateClient();
        adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var approveResp = await adminClient.PostAsJsonAsync($"/api/admin/payouts/{payoutId}/approve", new ReviewPayoutRequestModel("Approved and sent via KBZPay"));
        Assert.Equal(HttpStatusCode.OK, approveResp.StatusCode);

        var approveResult = await approveResp.Content.ReadFromJsonAsync<Result<AdminPayoutModel>>();
        Assert.True(approveResult?.IsSuccess);
        Assert.Equal("COMPLETED", approveResult?.Data?.Status);

        // Verify creator wallet is NOW deducted
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var wallet = await db.TblLinkDropWallets.AsNoTracking().FirstAsync(w => w.UserId == creatorId);

        Assert.Equal(200, wallet.EarnedBalance);
        Assert.Equal(200, wallet.Balance);

        // Verify audit ledger record in TblLinkDropTransaction
        var auditTx = await db.TblLinkDropTransactions.FirstOrDefaultAsync(t => t.UserId == creatorId && t.TransactionType == "CREATOR_PAYOUT");
        Assert.NotNull(auditTx);
        Assert.Equal(100, auditTx.Amount);
        Assert.Equal(300, auditTx.BalanceBefore);
        Assert.Equal(200, auditTx.BalanceAfter);
    }

    [Fact]
    public async Task AdminReject_DoesNotReduceBalance()
    {
        var (creatorToken, creatorId) = await GetTokenAndUserIdForRoleAsync("DOMAIN_PROFESSIONAL", "pay_creator5", "pay_creator5@test.com", "Password@123");
        var (adminToken, _) = await GetTokenAndUserIdForRoleAsync("ADMIN", "admin_pay5", "admin_pay5@test.com", "Password@123");
        await SetupWalletBalancesAsync(creatorId, purchasedBalance: 0, earnedBalance: 150);

        // Creator requests payout
        var creatorClient = _factory.CreateClient();
        creatorClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", creatorToken);

        var req = new CreatePayoutRequestModel(50, "KBZPay", "U Mg Mg", "09123456789");
        var createResp = await creatorClient.PostAsJsonAsync("/api/creator/payouts", req);
        var createResult = await createResp.Content.ReadFromJsonAsync<Result<CreatorPayoutModel>>();
        long payoutId = createResult!.Data!.CreatorPayoutRequestId;

        // Admin rejects payout
        var adminClient = _factory.CreateClient();
        adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var rejectResp = await adminClient.PostAsJsonAsync($"/api/admin/payouts/{payoutId}/reject", new ReviewPayoutRequestModel("Invalid account number"));
        Assert.Equal(HttpStatusCode.OK, rejectResp.StatusCode);

        var rejectResult = await rejectResp.Content.ReadFromJsonAsync<Result<AdminPayoutModel>>();
        Assert.True(rejectResult?.IsSuccess);
        Assert.Equal("REJECTED", rejectResult?.Data?.Status);

        // Verify wallet balance is UNCHANGED
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var wallet = await db.TblLinkDropWallets.AsNoTracking().FirstAsync(w => w.UserId == creatorId);

        Assert.Equal(150, wallet.EarnedBalance);
        Assert.Equal(150, wallet.Balance);
    }

    [Fact]
    public async Task CreatorCannotAccessOtherCreatorPayouts()
    {
        var (creatorAToken, creatorAId) = await GetTokenAndUserIdForRoleAsync("DOMAIN_PROFESSIONAL", "pay_creatorA", "pay_creatorA@test.com", "Password@123");
        var (creatorBToken, _) = await GetTokenAndUserIdForRoleAsync("DOMAIN_PROFESSIONAL", "pay_creatorB", "pay_creatorB@test.com", "Password@123");

        await SetupWalletBalancesAsync(creatorAId, purchasedBalance: 0, earnedBalance: 100);

        // Creator A creates payout
        var clientA = _factory.CreateClient();
        clientA.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", creatorAToken);
        await clientA.PostAsJsonAsync("/api/creator/payouts", new CreatePayoutRequestModel(40, "KBZPay", "U Mg Mg", "09123456789"));

        // Creator B queries /api/creator/payouts
        var clientB = _factory.CreateClient();
        clientB.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", creatorBToken);

        var getRespB = await clientB.GetAsync("/api/creator/payouts");
        var resultB = await getRespB.Content.ReadFromJsonAsync<Result<IReadOnlyList<CreatorPayoutModel>>>();

        Assert.True(resultB?.IsSuccess);
        Assert.Empty(resultB?.Data ?? new List<CreatorPayoutModel>());
    }

    [Fact]
    public async Task PurchasedBalanceNeverUsedForPayout()
    {
        var (creatorToken, creatorId) = await GetTokenAndUserIdForRoleAsync("DOMAIN_PROFESSIONAL", "pay_creator6", "pay_creator6@test.com", "Password@123");

        // Creator has 500 Purchased Link Drops, 0 Earned Balance
        await SetupWalletBalancesAsync(creatorId, purchasedBalance: 500, earnedBalance: 0);

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", creatorToken);

        var req = new CreatePayoutRequestModel(50, "KBZPay", "U Mg Mg", "09123456789");
        var resp = await client.PostAsJsonAsync("/api/creator/payouts", req);

        var result = await resp.Content.ReadFromJsonAsync<Result<CreatorPayoutModel>>();
        Assert.False(result?.IsSuccess);
    }

    [Fact]
    public async Task CannotApprove_AlreadyCompletedOrRejectedPayout()
    {
        var (creatorToken, creatorId) = await GetTokenAndUserIdForRoleAsync("DOMAIN_PROFESSIONAL", "pay_creator7", "pay_creator7@test.com", "Password@123");
        var (adminToken, _) = await GetTokenAndUserIdForRoleAsync("ADMIN", "admin_pay7", "admin_pay7@test.com", "Password@123");
        await SetupWalletBalancesAsync(creatorId, purchasedBalance: 0, earnedBalance: 200);

        var creatorClient = _factory.CreateClient();
        creatorClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", creatorToken);

        var createResp = await creatorClient.PostAsJsonAsync("/api/creator/payouts", new CreatePayoutRequestModel(50, "KBZPay", "U Mg Mg", "09123456789"));
        var createResult = await createResp.Content.ReadFromJsonAsync<Result<CreatorPayoutModel>>();
        long payoutId = createResult!.Data!.CreatorPayoutRequestId;

        var adminClient = _factory.CreateClient();
        adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        // First approval succeeds
        var approveResp1 = await adminClient.PostAsJsonAsync($"/api/admin/payouts/{payoutId}/approve", new ReviewPayoutRequestModel("First approval"));
        Assert.Equal(HttpStatusCode.OK, approveResp1.StatusCode);
        var approveResult1 = await approveResp1.Content.ReadFromJsonAsync<Result<AdminPayoutModel>>();
        Assert.True(approveResult1?.IsSuccess);

        // Second approval attempt MUST fail and NOT duplicate ledger entry
        var approveResp2 = await adminClient.PostAsJsonAsync($"/api/admin/payouts/{payoutId}/approve", new ReviewPayoutRequestModel("Duplicate approval"));
        var approveResult2 = await approveResp2.Content.ReadFromJsonAsync<Result<AdminPayoutModel>>();
        Assert.False(approveResult2?.IsSuccess);

        // Verify only 1 audit transaction was created in TblLinkDropTransaction
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        int txCount = await db.TblLinkDropTransactions.CountAsync(t => t.UserId == creatorId && t.TransactionType == "CREATOR_PAYOUT");
        Assert.Equal(1, txCount);
    }

    [Fact]
    public async Task MultiplePendingRequests_CumulativeValidation()
    {
        var (creatorToken, creatorId) = await GetTokenAndUserIdForRoleAsync("DOMAIN_PROFESSIONAL", "pay_creator8", "pay_creator8@test.com", "Password@123");
        await SetupWalletBalancesAsync(creatorId, purchasedBalance: 0, earnedBalance: 100);

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", creatorToken);

        // Request 1: 60 LD -> net available = 40 LD
        var resp1 = await client.PostAsJsonAsync("/api/creator/payouts", new CreatePayoutRequestModel(60, "KBZPay", "U Mg Mg", "09123456789"));
        var res1 = await resp1.Content.ReadFromJsonAsync<Result<CreatorPayoutModel>>();
        Assert.True(res1?.IsSuccess);

        // Request 2: 50 LD -> requires 50 LD, but net available is 40 LD -> MUST FAIL
        var resp2 = await client.PostAsJsonAsync("/api/creator/payouts", new CreatePayoutRequestModel(50, "KBZPay", "U Mg Mg", "09123456789"));
        var res2 = await resp2.Content.ReadFromJsonAsync<Result<CreatorPayoutModel>>();
        Assert.False(res2?.IsSuccess);

        // Request 3: 40 LD -> net available is 40 LD -> MUST SUCCEED
        var resp3 = await client.PostAsJsonAsync("/api/creator/payouts", new CreatePayoutRequestModel(40, "KBZPay", "U Mg Mg", "09123456789"));
        var res3 = await resp3.Content.ReadFromJsonAsync<Result<CreatorPayoutModel>>();
        Assert.True(res3?.IsSuccess);
    }

    [Fact]
    public async Task AdminPayoutEndpoints_AnonymousUser_Returns401()
    {
        var anonymousClient = _factory.CreateClient();

        var getResp = await anonymousClient.GetAsync("/api/admin/payouts");
        Assert.Equal(HttpStatusCode.Unauthorized, getResp.StatusCode);

        var approveResp = await anonymousClient.PostAsJsonAsync("/api/admin/payouts/1/approve", new ReviewPayoutRequestModel("Unauthorized test"));
        Assert.Equal(HttpStatusCode.Unauthorized, approveResp.StatusCode);

        var rejectResp = await anonymousClient.PostAsJsonAsync("/api/admin/payouts/1/reject", new ReviewPayoutRequestModel("Unauthorized test"));
        Assert.Equal(HttpStatusCode.Unauthorized, rejectResp.StatusCode);
    }

    [Fact]
    public async Task AdminPayoutEndpoints_NormalMember_Returns403()
    {
        var (memberToken, _) = await GetTokenAndUserIdForRoleAsync("MEMBER", "normal_user1", "normal_user1@test.com", "Password@123");

        var memberClient = _factory.CreateClient();
        memberClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", memberToken);

        var getResp = await memberClient.GetAsync("/api/admin/payouts");
        Assert.Equal(HttpStatusCode.Forbidden, getResp.StatusCode);
    }

    [Fact]
    public async Task AdminPayoutEndpoints_AuthorizedAdmin_ReturnsSuccess()
    {
        var (adminToken, _) = await GetTokenAndUserIdForRoleAsync("ADMIN", "admin_user_auth", "admin_user_auth@test.com", "Password@123");

        var adminClient = _factory.CreateClient();
        adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var getResp = await adminClient.GetAsync("/api/admin/payouts");
        Assert.Equal(HttpStatusCode.OK, getResp.StatusCode);

        var result = await getResp.Content.ReadFromJsonAsync<Result<IReadOnlyList<AdminPayoutModel>>>();
        Assert.True(result?.IsSuccess);
    }

    [Fact]
    public async Task NormalUser_CannotApprovePayout()
    {
        var (creatorToken, creatorId) = await GetTokenAndUserIdForRoleAsync("DOMAIN_PROFESSIONAL", "pay_creator9", "pay_creator9@test.com", "Password@123");
        var (memberToken, _) = await GetTokenAndUserIdForRoleAsync("MEMBER", "normal_user2", "normal_user2@test.com", "Password@123");

        await SetupWalletBalancesAsync(creatorId, purchasedBalance: 0, earnedBalance: 200);

        var creatorClient = _factory.CreateClient();
        creatorClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", creatorToken);
        var createResp = await creatorClient.PostAsJsonAsync("/api/creator/payouts", new CreatePayoutRequestModel(50, "KBZPay", "U Mg Mg", "09123456789"));
        var createResult = await createResp.Content.ReadFromJsonAsync<Result<CreatorPayoutModel>>();
        long payoutId = createResult!.Data!.CreatorPayoutRequestId;

        // Normal member attempts approval
        var memberClient = _factory.CreateClient();
        memberClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", memberToken);
        var approveResp = await memberClient.PostAsJsonAsync($"/api/admin/payouts/{payoutId}/approve", new ReviewPayoutRequestModel("Illegal approval attempt"));

        Assert.Equal(HttpStatusCode.Forbidden, approveResp.StatusCode);

        // Verify status remains PENDING
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var payout = await db.TblCreatorPayoutRequests.FirstAsync(p => p.CreatorPayoutRequestId == payoutId);
        Assert.Equal("PENDING", payout.Status);
    }

    [Fact]
    public async Task NormalUser_CannotRejectPayout()
    {
        var (creatorToken, creatorId) = await GetTokenAndUserIdForRoleAsync("DOMAIN_PROFESSIONAL", "pay_creator10", "pay_creator10@test.com", "Password@123");
        var (memberToken, _) = await GetTokenAndUserIdForRoleAsync("MEMBER", "normal_user3", "normal_user3@test.com", "Password@123");

        await SetupWalletBalancesAsync(creatorId, purchasedBalance: 0, earnedBalance: 200);

        var creatorClient = _factory.CreateClient();
        creatorClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", creatorToken);
        var createResp = await creatorClient.PostAsJsonAsync("/api/creator/payouts", new CreatePayoutRequestModel(50, "KBZPay", "U Mg Mg", "09123456789"));
        var createResult = await createResp.Content.ReadFromJsonAsync<Result<CreatorPayoutModel>>();
        long payoutId = createResult!.Data!.CreatorPayoutRequestId;

        // Normal member attempts rejection
        var memberClient = _factory.CreateClient();
        memberClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", memberToken);
        var rejectResp = await memberClient.PostAsJsonAsync($"/api/admin/payouts/{payoutId}/reject", new ReviewPayoutRequestModel("Illegal rejection attempt"));

        Assert.Equal(HttpStatusCode.Forbidden, rejectResp.StatusCode);

        // Verify status remains PENDING
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var payout = await db.TblCreatorPayoutRequests.FirstAsync(p => p.CreatorPayoutRequestId == payoutId);
        Assert.Equal("PENDING", payout.Status);
    }
}
