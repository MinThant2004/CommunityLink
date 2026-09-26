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
}
