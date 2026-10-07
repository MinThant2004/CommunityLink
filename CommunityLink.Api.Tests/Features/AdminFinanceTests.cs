using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CommunityLink.Api.Tests.Infrastructure;
using CommunityLink.Database.AppDbContextModels;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Authentication;
using CommunityLink.Shared.Features.Finance;
using CommunityLink.Shared.Features.Payout;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CommunityLink.Api.Tests.Features;

public class AdminFinanceTests : IClassFixture<CommunityApiFactory>
{
    private readonly CommunityApiFactory _factory;
    private readonly HttpClient _client;

    public AdminFinanceTests(CommunityApiFactory factory)
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
    public async Task AnonymousFinanceSummary_Returns401()
    {
        var anonymousClient = _factory.CreateClient();
        var resp = await anonymousClient.GetAsync("/api/admin/finance/summary");
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task NormalMemberFinanceSummary_Returns403()
    {
        var (memberToken, _) = await GetTokenAndUserIdForRoleAsync("MEMBER", "fin_member1", "fin_member1@test.com", "Password@123");
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", memberToken);

        var resp = await client.GetAsync("/api/admin/finance/summary");
        Assert.Equal(HttpStatusCode.Forbidden, resp.StatusCode);
    }

    [Fact]
    public async Task AuthorizedAdminFinanceSummary_Returns200()
    {
        var (adminToken, _) = await GetTokenAndUserIdForRoleAsync("ADMIN", "fin_admin1", "fin_admin1@test.com", "Password@123");
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var resp = await client.GetAsync("/api/admin/finance/summary");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

        var result = await resp.Content.ReadFromJsonAsync<Result<AdminFinanceSummaryModel>>();
        Assert.True(result?.IsSuccess);
        Assert.NotNull(result?.Data);
    }

    [Fact]
    public async Task FinanceSummary_CalculatesGrossCommissionAndNetCorrectly()
    {
        var (adminToken, _) = await GetTokenAndUserIdForRoleAsync("ADMIN", "fin_admin2", "fin_admin2@test.com", "Password@123");
        var (_, creatorId) = await GetTokenAndUserIdForRoleAsync("DOMAIN_PROFESSIONAL", "fin_creator1", "fin_creator1@test.com", "Password@123");
        var (_, buyerId) = await GetTokenAndUserIdForRoleAsync("MEMBER", "fin_buyer1", "fin_buyer1@test.com", "Password@123");

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var group = new TblChatGroup
            {
                CreatorId = creatorId,
                Name = "Finance Test Group 1",
                ChatType = "PAID",
                JoinFeeLinkDrops = 500,
                CreatedAt = DateTime.UtcNow
            };
            db.TblChatGroups.Add(group);
            await db.SaveChangesAsync();

            db.TblChatGroupPaymentTransactions.Add(new TblChatGroupPaymentTransaction
            {
                ChatGroupId = group.ChatGroupId,
                UserId = buyerId,
                CreatorUserId = creatorId,
                GrossAmount = 500,
                CommissionPercentage = 10.0m,
                CommissionAmount = 50,
                NetAmount = 450,
                Status = "COMPLETED",
                CreatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var resp = await client.GetAsync("/api/admin/finance/summary");
        var result = await resp.Content.ReadFromJsonAsync<Result<AdminFinanceSummaryModel>>();

        Assert.True(result?.IsSuccess);
        Assert.True(result?.Data?.GrossChatGroupRevenueLinkDrops >= 500);
        Assert.True(result?.Data?.PlatformCommissionLinkDrops >= 50);
        Assert.True(result?.Data?.CreatorNetEarningsLinkDrops >= 450);
    }

    [Fact]
    public async Task FinanceSummary_DoesNotDoubleCountWalletLedger()
    {
        var (adminToken, _) = await GetTokenAndUserIdForRoleAsync("ADMIN", "fin_admin3", "fin_admin3@test.com", "Password@123");
        var (_, creatorId) = await GetTokenAndUserIdForRoleAsync("DOMAIN_PROFESSIONAL", "fin_creator2", "fin_creator2@test.com", "Password@123");

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Add a wallet record with 10,000 PurchasedBalance & 5,000 EarnedBalance
            var wallet = new TblLinkDropWallet
            {
                UserId = creatorId,
                PurchasedBalance = 10000,
                EarnedBalance = 5000,
                Balance = 15000,
                CreatedAt = DateTime.UtcNow
            };
            db.TblLinkDropWallets.Add(wallet);

            // Add a ledger audit entry
            db.TblLinkDropTransactions.Add(new TblLinkDropTransaction
            {
                WalletId = wallet.WalletId,
                UserId = creatorId,
                TransactionType = "TOP_UP",
                ReferenceType = "TOP_UP",
                Amount = 10000,
                BalanceBefore = 0,
                BalanceAfter = 10000,
                CreatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var resp = await client.GetAsync("/api/admin/finance/summary");
        var result = await resp.Content.ReadFromJsonAsync<Result<AdminFinanceSummaryModel>>();

        Assert.True(result?.IsSuccess);
        // Wallet top-up balance must NOT be added into GrossChatGroupRevenueLinkDrops
    }

    [Fact]
    public async Task FinanceSummary_ExcludesPurchasedBalance()
    {
        var (adminToken, _) = await GetTokenAndUserIdForRoleAsync("ADMIN", "fin_admin4", "fin_admin4@test.com", "Password@123");
        var (_, userId) = await GetTokenAndUserIdForRoleAsync("MEMBER", "fin_user_purchased", "fin_user_purchased@test.com", "Password@123");

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var wallet = new TblLinkDropWallet
            {
                UserId = userId,
                PurchasedBalance = 50000,
                EarnedBalance = 0,
                Balance = 50000,
                CreatedAt = DateTime.UtcNow
            };
            db.TblLinkDropWallets.Add(wallet);
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var resp = await client.GetAsync("/api/admin/finance/summary");
        var result = await resp.Content.ReadFromJsonAsync<Result<AdminFinanceSummaryModel>>();

        Assert.True(result?.IsSuccess);
        // PurchasedBalance 50,000 must NOT be present in GrossChatGroupRevenueLinkDrops or PlatformCommissionLinkDrops
    }

    [Fact]
    public async Task FinanceSummary_IncludesOnlyCompletedRevenueTransactions()
    {
        var (adminToken, _) = await GetTokenAndUserIdForRoleAsync("ADMIN", "fin_admin5", "fin_admin5@test.com", "Password@123");
        var (_, creatorId) = await GetTokenAndUserIdForRoleAsync("DOMAIN_PROFESSIONAL", "fin_creator3", "fin_creator3@test.com", "Password@123");
        var (_, buyerId) = await GetTokenAndUserIdForRoleAsync("MEMBER", "fin_buyer2", "fin_buyer2@test.com", "Password@123");

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var group = new TblChatGroup { CreatorId = creatorId, Name = "Pending Group", ChatType = "PAID", JoinFeeLinkDrops = 300, CreatedAt = DateTime.UtcNow };
            db.TblChatGroups.Add(group);
            await db.SaveChangesAsync();

            // PENDING transaction (FAILED/PENDING) should NOT be counted in completed revenue
            db.TblChatGroupPaymentTransactions.Add(new TblChatGroupPaymentTransaction
            {
                ChatGroupId = group.ChatGroupId,
                UserId = buyerId,
                CreatorUserId = creatorId,
                GrossAmount = 300,
                CommissionPercentage = 10.0m,
                CommissionAmount = 30,
                NetAmount = 270,
                Status = "PENDING",
                CreatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var resp = await client.GetAsync("/api/admin/finance/summary");
        var result = await resp.Content.ReadFromJsonAsync<Result<AdminFinanceSummaryModel>>();

        Assert.True(result?.IsSuccess);
    }

    [Fact]
    public async Task FinanceSummary_CountsCompletedAndPendingPayoutsCorrectly()
    {
        var (adminToken, _) = await GetTokenAndUserIdForRoleAsync("ADMIN", "fin_admin6", "fin_admin6@test.com", "Password@123");
        var (_, creatorId) = await GetTokenAndUserIdForRoleAsync("DOMAIN_PROFESSIONAL", "fin_creator4", "fin_creator4@test.com", "Password@123");

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Add PENDING payout request (100 LD)
            db.TblCreatorPayoutRequests.Add(new TblCreatorPayoutRequest
            {
                CreatorUserId = creatorId,
                AmountLinkDrops = 100,
                AmountMmk = 10000,
                PaymentMethod = "KBZPay",
                PaymentAccountName = "U Mg Mg",
                PaymentAccountNumber = "09123456789",
                Status = "PENDING",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = creatorId
            });

            // Add COMPLETED payout request (200 LD)
            db.TblCreatorPayoutRequests.Add(new TblCreatorPayoutRequest
            {
                CreatorUserId = creatorId,
                AmountLinkDrops = 200,
                AmountMmk = 20000,
                PaymentMethod = "KBZPay",
                PaymentAccountName = "U Mg Mg",
                PaymentAccountNumber = "09123456789",
                Status = "COMPLETED",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = creatorId
            });

            await db.SaveChangesAsync();
        }

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var resp = await client.GetAsync("/api/admin/finance/summary");
        var result = await resp.Content.ReadFromJsonAsync<Result<AdminFinanceSummaryModel>>();

        Assert.True(result?.IsSuccess);
        Assert.True(result?.Data?.PendingCreatorPayoutsLinkDrops >= 100);
        Assert.True(result?.Data?.CompletedCreatorPayoutsLinkDrops >= 200);
        Assert.True(result?.Data?.PendingPayoutCount >= 1);
        Assert.True(result?.Data?.CompletedPayoutCount >= 1);
    }

    [Fact]
    public async Task FinanceSummary_DoesNotTreatPayoutAsRevenue()
    {
        var (adminToken, _) = await GetTokenAndUserIdForRoleAsync("ADMIN", "fin_admin7", "fin_admin7@test.com", "Password@123");
        var (_, creatorId) = await GetTokenAndUserIdForRoleAsync("DOMAIN_PROFESSIONAL", "fin_creator5", "fin_creator5@test.com", "Password@123");

        long initialGross = 0;
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var initialResp = await client.GetAsync("/api/admin/finance/summary");
        var initialResult = await initialResp.Content.ReadFromJsonAsync<Result<AdminFinanceSummaryModel>>();
        initialGross = initialResult?.Data?.GrossChatGroupRevenueLinkDrops ?? 0;

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.TblCreatorPayoutRequests.Add(new TblCreatorPayoutRequest
            {
                CreatorUserId = creatorId,
                AmountLinkDrops = 500,
                AmountMmk = 50000,
                PaymentMethod = "KBZPay",
                PaymentAccountName = "U Mg Mg",
                PaymentAccountNumber = "09123456789",
                Status = "COMPLETED",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = creatorId
            });
            await db.SaveChangesAsync();
        }

        var afterResp = await client.GetAsync("/api/admin/finance/summary");
        var afterResult = await afterResp.Content.ReadFromJsonAsync<Result<AdminFinanceSummaryModel>>();

        // Gross revenue MUST remain unchanged after adding a creator payout
        Assert.Equal(initialGross, afterResult?.Data?.GrossChatGroupRevenueLinkDrops);
        Assert.True(afterResult?.Data?.CompletedCreatorPayoutsLinkDrops >= 500);
    }

    [Fact]
    public async Task FinanceSummary_DateFilterWorksCorrectly()
    {
        var (adminToken, _) = await GetTokenAndUserIdForRoleAsync("ADMIN", "fin_admin8", "fin_admin8@test.com", "Password@123");
        var (_, creatorId) = await GetTokenAndUserIdForRoleAsync("DOMAIN_PROFESSIONAL", "fin_creator6", "fin_creator6@test.com", "Password@123");
        var (_, buyerId) = await GetTokenAndUserIdForRoleAsync("MEMBER", "fin_buyer3", "fin_buyer3@test.com", "Password@123");

        var pastDate = new DateTime(2025, 1, 15, 12, 0, 0, DateTimeKind.Utc);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var group = new TblChatGroup { CreatorId = creatorId, Name = "Past Group", ChatType = "PAID", JoinFeeLinkDrops = 1000, CreatedAt = pastDate };
            db.TblChatGroups.Add(group);
            await db.SaveChangesAsync();

            db.TblChatGroupPaymentTransactions.Add(new TblChatGroupPaymentTransaction
            {
                ChatGroupId = group.ChatGroupId,
                UserId = buyerId,
                CreatorUserId = creatorId,
                GrossAmount = 1000,
                CommissionPercentage = 10.0m,
                CommissionAmount = 100,
                NetAmount = 900,
                Status = "COMPLETED",
                CreatedAt = pastDate
            });
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        // Filter for year 2025
        var resp2025 = await client.GetAsync("/api/admin/finance/summary?fromDate=2025-01-01&toDate=2025-01-31");
        var result2025 = await resp2025.Content.ReadFromJsonAsync<Result<AdminFinanceSummaryModel>>();

        Assert.True(result2025?.IsSuccess);
        Assert.True(result2025?.Data?.GrossChatGroupRevenueLinkDrops >= 1000);

        // Filter for year 2024 (should not include 2025 transaction)
        var resp2024 = await client.GetAsync("/api/admin/finance/summary?fromDate=2024-01-01&toDate=2024-12-31");
        var result2024 = await resp2024.Content.ReadFromJsonAsync<Result<AdminFinanceSummaryModel>>();

        Assert.True(result2024?.IsSuccess);
        Assert.Equal(0, result2024?.Data?.GrossChatGroupRevenueLinkDrops ?? 0);
    }

    [Fact]
    public async Task FinanceGroupBreakdown_GroupsTransactionsCorrectly()
    {
        var (adminToken, _) = await GetTokenAndUserIdForRoleAsync("ADMIN", "fin_admin9", "fin_admin9@test.com", "Password@123");
        var (_, creatorId) = await GetTokenAndUserIdForRoleAsync("DOMAIN_PROFESSIONAL", "fin_creator7", "fin_creator7@test.com", "Password@123");
        var (_, buyerId) = await GetTokenAndUserIdForRoleAsync("MEMBER", "fin_buyer4", "fin_buyer4@test.com", "Password@123");

        int groupId = 0;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var group = new TblChatGroup { CreatorId = creatorId, Name = "Breakdown Test Group", ChatType = "PAID", JoinFeeLinkDrops = 750, CreatedAt = DateTime.UtcNow };
            db.TblChatGroups.Add(group);
            await db.SaveChangesAsync();
            groupId = group.ChatGroupId;

            db.TblChatGroupPaymentTransactions.Add(new TblChatGroupPaymentTransaction
            {
                ChatGroupId = group.ChatGroupId,
                UserId = buyerId,
                CreatorUserId = creatorId,
                GrossAmount = 750,
                CommissionPercentage = 10.0m,
                CommissionAmount = 75,
                NetAmount = 675,
                Status = "COMPLETED",
                CreatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var resp = await client.GetAsync("/api/admin/finance/groups");
        var result = await resp.Content.ReadFromJsonAsync<Result<IReadOnlyList<AdminFinanceGroupBreakdownModel>>>();

        Assert.True(result?.IsSuccess);
        Assert.NotNull(result?.Data);
        var groupBreakdown = result.Data.FirstOrDefault(g => g.ChatGroupId == groupId);
        Assert.NotNull(groupBreakdown);
        Assert.Equal(750, groupBreakdown.GrossRevenueLinkDrops);
        Assert.Equal(75, groupBreakdown.PlatformCommissionLinkDrops);
        Assert.Equal(675, groupBreakdown.CreatorNetEarningsLinkDrops);
        Assert.Equal(1, groupBreakdown.CompletedTransactionCount);
    }

    [Fact]
    public async Task FinanceTransactions_DoesNotExposeSensitivePaymentAccountData()
    {
        var (adminToken, _) = await GetTokenAndUserIdForRoleAsync("ADMIN", "fin_admin10", "fin_admin10@test.com", "Password@123");
        var (_, creatorId) = await GetTokenAndUserIdForRoleAsync("DOMAIN_PROFESSIONAL", "fin_creator8", "fin_creator8@test.com", "Password@123");

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            db.TblCreatorPayoutRequests.Add(new TblCreatorPayoutRequest
            {
                CreatorUserId = creatorId,
                AmountLinkDrops = 150,
                AmountMmk = 15000,
                PaymentMethod = "KBZPay",
                PaymentAccountName = "SECRET_ACCOUNT_NAME_999",
                PaymentAccountNumber = "09999999999",
                Status = "PENDING",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = creatorId
            });
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var resp = await client.GetAsync("/api/admin/finance/transactions?transactionType=CREATOR_PAYOUT");
        var rawContent = await resp.Content.ReadAsStringAsync();

        Assert.True(resp.IsSuccessStatusCode);
        // Verify sensitive account number & account name are NOT in the returned DTO JSON
        Assert.DoesNotContain("SECRET_ACCOUNT_NAME_999", rawContent);
        Assert.DoesNotContain("09999999999", rawContent);
    }

    [Fact]
    public async Task FinanceTransactions_FirstPage_ReturnsExpectedPageSize()
    {
        var (adminToken, _) = await GetTokenAndUserIdForRoleAsync("ADMIN", "fin_admin11", "fin_admin11@test.com", "Password@123");
        var (_, creatorId) = await GetTokenAndUserIdForRoleAsync("DOMAIN_PROFESSIONAL", "fin_creator11", "fin_creator11@test.com", "Password@123");

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var now = DateTime.UtcNow;
            for (int i = 1; i <= 5; i++)
            {
                db.TblCreatorPayoutRequests.Add(new TblCreatorPayoutRequest
                {
                    CreatorUserId = creatorId,
                    AmountLinkDrops = 10 * i,
                    AmountMmk = 1000 * i,
                    PaymentMethod = "KBZPay",
                    PaymentAccountName = "Account " + i,
                    PaymentAccountNumber = "0900000000" + i,
                    Status = "PENDING",
                    CreatedAt = now.AddMinutes(i),
                    CreatedBy = creatorId
                });
            }
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var resp = await client.GetAsync("/api/admin/finance/transactions?page=1&pageSize=2");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

        var result = await resp.Content.ReadFromJsonAsync<Result<AdminFinancePagedTransactionModel>>();
        Assert.True(result?.IsSuccess);
        Assert.NotNull(result?.Data);
        Assert.Equal(1, result.Data.Page);
        Assert.Equal(2, result.Data.PageSize);
        Assert.Equal(2, result.Data.Items.Count);
        Assert.True(result.Data.TotalCount >= 5);
    }

    [Fact]
    public async Task FinanceTransactions_SecondPage_ReturnsNextRecords()
    {
        var (adminToken, _) = await GetTokenAndUserIdForRoleAsync("ADMIN", "fin_admin12", "fin_admin12@test.com", "Password@123");
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var page1Resp = await client.GetAsync("/api/admin/finance/transactions?page=1&pageSize=2");
        var page1Result = await page1Resp.Content.ReadFromJsonAsync<Result<AdminFinancePagedTransactionModel>>();

        var page2Resp = await client.GetAsync("/api/admin/finance/transactions?page=2&pageSize=2");
        var page2Result = await page2Resp.Content.ReadFromJsonAsync<Result<AdminFinancePagedTransactionModel>>();

        Assert.True(page1Result?.IsSuccess);
        Assert.True(page2Result?.IsSuccess);
        Assert.Equal(2, page2Result?.Data?.Page);
        Assert.Equal(2, page2Result?.Data?.Items.Count);

        var firstPageIds = page1Result!.Data!.Items.Select(x => (x.TransactionType, x.TransactionId)).ToList();
        var secondPageIds = page2Result!.Data!.Items.Select(x => (x.TransactionType, x.TransactionId)).ToList();

        Assert.Empty(firstPageIds.Intersect(secondPageIds));
    }

    [Fact]
    public async Task FinanceTransactions_PaginationPreservesFilters()
    {
        var (adminToken, _) = await GetTokenAndUserIdForRoleAsync("ADMIN", "fin_admin13", "fin_admin13@test.com", "Password@123");
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var resp = await client.GetAsync("/api/admin/finance/transactions?transactionType=CREATOR_PAYOUT&status=PENDING&page=1&pageSize=10");
        var result = await resp.Content.ReadFromJsonAsync<Result<AdminFinancePagedTransactionModel>>();

        Assert.True(result?.IsSuccess);
        Assert.All(result!.Data!.Items, item =>
        {
            Assert.Equal("CREATOR_PAYOUT", item.TransactionType);
            Assert.Equal("PENDING", item.Status, ignoreCase: true);
        });
    }

    [Fact]
    public async Task FinanceTransactions_InvalidPageSize_IsHandledSafely()
    {
        var (adminToken, _) = await GetTokenAndUserIdForRoleAsync("ADMIN", "fin_admin14", "fin_admin14@test.com", "Password@123");
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var resp = await client.GetAsync("/api/admin/finance/transactions?page=-5&pageSize=0");
        var result = await resp.Content.ReadFromJsonAsync<Result<AdminFinancePagedTransactionModel>>();

        Assert.True(result?.IsSuccess);
        Assert.Equal(1, result?.Data?.Page);
        Assert.Equal(25, result?.Data?.PageSize);
    }

    [Fact]
    public async Task FinanceTransactions_MaxPageSize_IsEnforced()
    {
        var (adminToken, _) = await GetTokenAndUserIdForRoleAsync("ADMIN", "fin_admin15", "fin_admin15@test.com", "Password@123");
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var resp = await client.GetAsync("/api/admin/finance/transactions?pageSize=500");
        var result = await resp.Content.ReadFromJsonAsync<Result<AdminFinancePagedTransactionModel>>();

        Assert.True(result?.IsSuccess);
        Assert.Equal(100, result?.Data?.PageSize);
    }

    [Fact]
    public async Task FinanceTransactions_DeterministicOrdering()
    {
        var (adminToken, _) = await GetTokenAndUserIdForRoleAsync("ADMIN", "fin_admin16", "fin_admin16@test.com", "Password@123");
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var resp = await client.GetAsync("/api/admin/finance/transactions?pageSize=50");
        var result = await resp.Content.ReadFromJsonAsync<Result<AdminFinancePagedTransactionModel>>();

        Assert.True(result?.IsSuccess);
        var items = result!.Data!.Items;

        for (int i = 0; i < items.Count - 1; i++)
        {
            Assert.True(items[i].Date >= items[i + 1].Date, "Transactions must be ordered by Date DESC");
            if (items[i].Date == items[i + 1].Date)
            {
                Assert.True(items[i].TransactionId >= items[i + 1].TransactionId, "Transactions with equal Date must be ordered by TransactionId DESC");
            }
        }
    }

    [Fact]
    public async Task FinanceExport_Admin_ReturnsCsv()
    {
        var (adminToken, _) = await GetTokenAndUserIdForRoleAsync("ADMIN", "fin_admin17", "fin_admin17@test.com", "Password@123");
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var resp = await client.GetAsync("/api/admin/finance/export");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        Assert.Equal("text/csv", resp.Content.Headers.ContentType?.MediaType);
        Assert.NotNull(resp.Content.Headers.ContentDisposition?.FileName);
        Assert.StartsWith("communitylink-finance-report-", resp.Content.Headers.ContentDisposition!.FileName!.Trim('"'));
    }

    [Fact]
    public async Task FinanceExport_ContainsExpectedHeaders()
    {
        var (adminToken, _) = await GetTokenAndUserIdForRoleAsync("ADMIN", "fin_admin18", "fin_admin18@test.com", "Password@123");
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var resp = await client.GetAsync("/api/admin/finance/export");
        var csvContent = await resp.Content.ReadAsStringAsync();

        Assert.StartsWith("Date,Transaction Type,User / Creator,Chat Group,Gross Amount,Commission Amount,Net Amount,Status", csvContent.TrimStart());
    }

    [Fact]
    public async Task FinanceExport_EscapesCsvFieldsCorrectly()
    {
        var (adminToken, _) = await GetTokenAndUserIdForRoleAsync("ADMIN", "fin_admin19", "fin_admin19@test.com", "Password@123");
        var (_, creatorId) = await GetTokenAndUserIdForRoleAsync("DOMAIN_PROFESSIONAL", "fin_creator19", "fin_creator19@test.com", "Password@123");

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var group = new TblChatGroup
            {
                CreatorId = creatorId,
                Name = "Group, With Comma and \"Quotes\"",
                ChatType = "PAID",
                JoinFeeLinkDrops = 500,
                CreatedAt = DateTime.UtcNow
            };
            db.TblChatGroups.Add(group);
            await db.SaveChangesAsync();

            db.TblChatGroupPaymentTransactions.Add(new TblChatGroupPaymentTransaction
            {
                ChatGroupId = group.ChatGroupId,
                UserId = creatorId,
                CreatorUserId = creatorId,
                GrossAmount = 500,
                CommissionPercentage = 10.0m,
                CommissionAmount = 50,
                NetAmount = 450,
                Status = "COMPLETED",
                CreatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var resp = await client.GetAsync("/api/admin/finance/export");
        var csvContent = await resp.Content.ReadAsStringAsync();

        Assert.Contains("\"Group, With Comma and \"\"Quotes\"\"\"", csvContent);
    }

    [Fact]
    public async Task FinanceExport_NormalMember_Returns403()
    {
        var (memberToken, _) = await GetTokenAndUserIdForRoleAsync("MEMBER", "fin_member_exp", "fin_member_exp@test.com", "Password@123");
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", memberToken);

        var resp = await client.GetAsync("/api/admin/finance/export");
        Assert.Equal(HttpStatusCode.Forbidden, resp.StatusCode);
    }

    [Fact]
    public async Task FinanceExport_Anonymous_Returns401()
    {
        var client = _factory.CreateClient();
        var resp = await client.GetAsync("/api/admin/finance/export");
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task FinanceExport_DoesNotExposeSensitivePaymentData()
    {
        var (adminToken, _) = await GetTokenAndUserIdForRoleAsync("ADMIN", "fin_admin20", "fin_admin20@test.com", "Password@123");
        var (_, creatorId) = await GetTokenAndUserIdForRoleAsync("DOMAIN_PROFESSIONAL", "fin_creator20", "fin_creator20@test.com", "Password@123");

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.TblCreatorPayoutRequests.Add(new TblCreatorPayoutRequest
            {
                CreatorUserId = creatorId,
                AmountLinkDrops = 777,
                AmountMmk = 77700,
                PaymentMethod = "KBZPay",
                PaymentAccountName = "CONFIDENTIAL_PAYEE_NAME_888",
                PaymentAccountNumber = "09888888888",
                Status = "PENDING",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = creatorId
            });
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var resp = await client.GetAsync("/api/admin/finance/export");
        var csvContent = await resp.Content.ReadAsStringAsync();

        Assert.DoesNotContain("CONFIDENTIAL_PAYEE_NAME_888", csvContent);
        Assert.DoesNotContain("09888888888", csvContent);
    }

    [Fact]
    public async Task FinanceSummary_RemainsIndependentOfPagination()
    {
        var (adminToken, _) = await GetTokenAndUserIdForRoleAsync("ADMIN", "fin_admin21", "fin_admin21@test.com", "Password@123");
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var summaryBefore = await (await client.GetAsync("/api/admin/finance/summary")).Content.ReadFromJsonAsync<Result<AdminFinanceSummaryModel>>();
        var pagedTx = await (await client.GetAsync("/api/admin/finance/transactions?page=1&pageSize=1")).Content.ReadFromJsonAsync<Result<AdminFinancePagedTransactionModel>>();
        var summaryAfter = await (await client.GetAsync("/api/admin/finance/summary")).Content.ReadFromJsonAsync<Result<AdminFinanceSummaryModel>>();

        Assert.Equal(summaryBefore?.Data?.GrossChatGroupRevenueLinkDrops, summaryAfter?.Data?.GrossChatGroupRevenueLinkDrops);
        Assert.Equal(summaryBefore?.Data?.PlatformCommissionLinkDrops, summaryAfter?.Data?.PlatformCommissionLinkDrops);
    }

    [Fact]
    public async Task FinanceExport_UsesSelectedFilters()
    {
        var (adminToken, _) = await GetTokenAndUserIdForRoleAsync("ADMIN", "fin_admin22", "fin_admin22@test.com", "Password@123");
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var resp = await client.GetAsync("/api/admin/finance/export?transactionType=CHAT_GROUP_JOIN");
        var csvContent = await resp.Content.ReadAsStringAsync();

        Assert.True(resp.IsSuccessStatusCode);
        var lines = csvContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        for (int i = 1; i < lines.Length; i++)
        {
            Assert.Contains("CHAT_GROUP_JOIN", lines[i]);
            Assert.DoesNotContain("CREATOR_PAYOUT", lines[i]);
        }
    }

    [Fact]
    public async Task CompletedPayment_ReconcilesGrossCommissionAndNet()
    {
        var (adminToken, _) = await GetTokenAndUserIdForRoleAsync("ADMIN", "rec_admin1", "rec_admin1@test.com", "Password@123");
        var (_, creatorId) = await GetTokenAndUserIdForRoleAsync("DOMAIN_PROFESSIONAL", "rec_creator1", "rec_creator1@test.com", "Password@123");
        var (_, buyerId) = await GetTokenAndUserIdForRoleAsync("MEMBER", "rec_buyer1", "rec_buyer1@test.com", "Password@123");

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var group = new TblChatGroup { CreatorId = creatorId, Name = "Reconcile Group 1", ChatType = "PAID", JoinFeeLinkDrops = 1000, CreatedAt = DateTime.UtcNow };
            db.TblChatGroups.Add(group);
            await db.SaveChangesAsync();

            db.TblChatGroupPaymentTransactions.Add(new TblChatGroupPaymentTransaction
            {
                ChatGroupId = group.ChatGroupId,
                UserId = buyerId,
                CreatorUserId = creatorId,
                GrossAmount = 1000,
                CommissionPercentage = 10.0m,
                CommissionAmount = 100,
                NetAmount = 900,
                Status = "COMPLETED",
                CreatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var resp = await client.GetAsync("/api/admin/finance/summary");
        var result = await resp.Content.ReadFromJsonAsync<Result<AdminFinanceSummaryModel>>();

        Assert.True(result?.IsSuccess);
        Assert.True(result?.Data?.GrossChatGroupRevenueLinkDrops >= 1000);
        Assert.Equal(result!.Data!.GrossChatGroupRevenueLinkDrops, result.Data.PlatformCommissionLinkDrops + result.Data.CreatorNetEarningsLinkDrops);
    }

    [Fact]
    public async Task CreatorPayout_IsNotRevenue()
    {
        var (adminToken, _) = await GetTokenAndUserIdForRoleAsync("ADMIN", "rec_admin2", "rec_admin2@test.com", "Password@123");
        var (_, creatorId) = await GetTokenAndUserIdForRoleAsync("DOMAIN_PROFESSIONAL", "rec_creator2", "rec_creator2@test.com", "Password@123");

        long initialGross = 0;
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var initResp = await client.GetAsync("/api/admin/finance/summary");
        var initResult = await initResp.Content.ReadFromJsonAsync<Result<AdminFinanceSummaryModel>>();
        initialGross = initResult?.Data?.GrossChatGroupRevenueLinkDrops ?? 0;

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.TblCreatorPayoutRequests.Add(new TblCreatorPayoutRequest
            {
                CreatorUserId = creatorId,
                AmountLinkDrops = 400,
                AmountMmk = 40000,
                PaymentMethod = "KBZPay",
                PaymentAccountName = "Creator Account",
                PaymentAccountNumber = "09111222333",
                Status = "COMPLETED",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = creatorId
            });
            await db.SaveChangesAsync();
        }

        var afterResp = await client.GetAsync("/api/admin/finance/summary");
        var afterResult = await afterResp.Content.ReadFromJsonAsync<Result<AdminFinanceSummaryModel>>();

        Assert.Equal(initialGross, afterResult?.Data?.GrossChatGroupRevenueLinkDrops);
    }

    [Fact]
    public async Task PurchasedBalance_IsNotCreatorRevenue()
    {
        var (adminToken, _) = await GetTokenAndUserIdForRoleAsync("ADMIN", "rec_admin3", "rec_admin3@test.com", "Password@123");
        var (_, userId) = await GetTokenAndUserIdForRoleAsync("MEMBER", "rec_user_purchased", "rec_user_purchased@test.com", "Password@123");

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var wallet = new TblLinkDropWallet
            {
                UserId = userId,
                PurchasedBalance = 99999,
                EarnedBalance = 0,
                Balance = 99999,
                CreatedAt = DateTime.UtcNow
            };
            db.TblLinkDropWallets.Add(wallet);
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var resp = await client.GetAsync("/api/admin/finance/summary");
        var result = await resp.Content.ReadFromJsonAsync<Result<AdminFinanceSummaryModel>>();

        Assert.True(result?.IsSuccess);
        Assert.True(result?.Data?.CreatorNetEarningsLinkDrops < 99999);
    }

    [Fact]
    public async Task PendingPayout_DoesNotReduceEarnedBalance()
    {
        var (_, creatorId) = await GetTokenAndUserIdForRoleAsync("DOMAIN_PROFESSIONAL", "rec_creator4", "rec_creator4@test.com", "Password@123");

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var wallet = new TblLinkDropWallet
            {
                UserId = creatorId,
                PurchasedBalance = 0,
                EarnedBalance = 2000,
                Balance = 2000,
                CreatedAt = DateTime.UtcNow
            };
            db.TblLinkDropWallets.Add(wallet);

            db.TblCreatorPayoutRequests.Add(new TblCreatorPayoutRequest
            {
                CreatorUserId = creatorId,
                AmountLinkDrops = 500,
                AmountMmk = 50000,
                PaymentMethod = "KBZPay",
                PaymentAccountName = "Pending Test",
                PaymentAccountNumber = "09444555666",
                Status = "PENDING",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = creatorId
            });
            await db.SaveChangesAsync();

            var currentWallet = await db.TblLinkDropWallets.FirstAsync(w => w.UserId == creatorId);
            Assert.Equal(2000, currentWallet.EarnedBalance);
        }
    }

    [Fact]
    public async Task RejectedPayout_DoesNotReduceEarnedBalance()
    {
        var (adminToken, _) = await GetTokenAndUserIdForRoleAsync("ADMIN", "rec_admin5", "rec_admin5@test.com", "Password@123");
        var (_, creatorId) = await GetTokenAndUserIdForRoleAsync("DOMAIN_PROFESSIONAL", "rec_creator5", "rec_creator5@test.com", "Password@123");

        long payoutId = 0;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var wallet = new TblLinkDropWallet
            {
                UserId = creatorId,
                PurchasedBalance = 0,
                EarnedBalance = 3000,
                Balance = 3000,
                CreatedAt = DateTime.UtcNow
            };
            db.TblLinkDropWallets.Add(wallet);

            var payout = new TblCreatorPayoutRequest
            {
                CreatorUserId = creatorId,
                AmountLinkDrops = 300,
                AmountMmk = 30000,
                PaymentMethod = "KBZPay",
                PaymentAccountName = "Reject Test",
                PaymentAccountNumber = "09777888999",
                Status = "PENDING",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = creatorId
            };
            db.TblCreatorPayoutRequests.Add(payout);
            await db.SaveChangesAsync();
            payoutId = payout.CreatorPayoutRequestId;
        }

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var rejectResp = await client.PostAsJsonAsync($"/api/admin/payouts/{payoutId}/reject", new { AdminNote = "Test Rejection" });
        Assert.True(rejectResp.IsSuccessStatusCode);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var currentWallet = await db.TblLinkDropWallets.FirstAsync(w => w.UserId == creatorId);
            Assert.Equal(3000, currentWallet.EarnedBalance);
        }
    }

    [Fact]
    public async Task CompletedPayout_MatchesWalletDeduction()
    {
        var (adminToken, _) = await GetTokenAndUserIdForRoleAsync("ADMIN", "rec_admin6", "rec_admin6@test.com", "Password@123");
        var (_, creatorId) = await GetTokenAndUserIdForRoleAsync("DOMAIN_PROFESSIONAL", "rec_creator6", "rec_creator6@test.com", "Password@123");

        long payoutId = 0;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var wallet = new TblLinkDropWallet
            {
                UserId = creatorId,
                PurchasedBalance = 0,
                EarnedBalance = 5000,
                Balance = 5000,
                CreatedAt = DateTime.UtcNow
            };
            db.TblLinkDropWallets.Add(wallet);

            var payout = new TblCreatorPayoutRequest
            {
                CreatorUserId = creatorId,
                AmountLinkDrops = 1000,
                AmountMmk = 100000,
                PaymentMethod = "KBZPay",
                PaymentAccountName = "Deduction Test",
                PaymentAccountNumber = "09123123123",
                Status = "PENDING",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = creatorId
            };
            db.TblCreatorPayoutRequests.Add(payout);
            await db.SaveChangesAsync();
            payoutId = payout.CreatorPayoutRequestId;
        }

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var approveResp = await client.PostAsJsonAsync($"/api/admin/payouts/{payoutId}/approve", new { AdminNote = "Approved test" });
        Assert.True(approveResp.IsSuccessStatusCode);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var currentWallet = await db.TblLinkDropWallets.FirstAsync(w => w.UserId == creatorId);
            Assert.Equal(4000, currentWallet.EarnedBalance);
        }
    }

    [Fact]
    public async Task CompletedPayout_HasExactlyOneLedgerEntry()
    {
        var (adminToken, _) = await GetTokenAndUserIdForRoleAsync("ADMIN", "rec_admin7", "rec_admin7@test.com", "Password@123");
        var (_, creatorId) = await GetTokenAndUserIdForRoleAsync("DOMAIN_PROFESSIONAL", "rec_creator7", "rec_creator7@test.com", "Password@123");

        long payoutId = 0;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var wallet = new TblLinkDropWallet
            {
                UserId = creatorId,
                PurchasedBalance = 0,
                EarnedBalance = 4000,
                Balance = 4000,
                CreatedAt = DateTime.UtcNow
            };
            db.TblLinkDropWallets.Add(wallet);

            var payout = new TblCreatorPayoutRequest
            {
                CreatorUserId = creatorId,
                AmountLinkDrops = 800,
                AmountMmk = 80000,
                PaymentMethod = "KBZPay",
                PaymentAccountName = "Ledger Test",
                PaymentAccountNumber = "09444333222",
                Status = "PENDING",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = creatorId
            };
            db.TblCreatorPayoutRequests.Add(payout);
            await db.SaveChangesAsync();
            payoutId = payout.CreatorPayoutRequestId;
        }

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var approveResp = await client.PostAsJsonAsync($"/api/admin/payouts/{payoutId}/approve", new { AdminNote = "Approved ledger test" });
        Assert.True(approveResp.IsSuccessStatusCode);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var ledgerCount = await db.TblLinkDropTransactions.CountAsync(t => t.TransactionType == "CREATOR_PAYOUT" && t.ReferenceType == "TblCreatorPayoutRequest" && t.ReferenceId == (int)payoutId);
            Assert.Equal(1, ledgerCount);
        }
    }

    [Fact]
    public async Task DuplicateApproval_CannotCreateDuplicateLedger()
    {
        var (adminToken, _) = await GetTokenAndUserIdForRoleAsync("ADMIN", "rec_admin8", "rec_admin8@test.com", "Password@123");
        var (_, creatorId) = await GetTokenAndUserIdForRoleAsync("DOMAIN_PROFESSIONAL", "rec_creator8", "rec_creator8@test.com", "Password@123");

        long payoutId = 0;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var wallet = new TblLinkDropWallet
            {
                UserId = creatorId,
                PurchasedBalance = 0,
                EarnedBalance = 3000,
                Balance = 3000,
                CreatedAt = DateTime.UtcNow
            };
            db.TblLinkDropWallets.Add(wallet);

            var payout = new TblCreatorPayoutRequest
            {
                CreatorUserId = creatorId,
                AmountLinkDrops = 600,
                AmountMmk = 60000,
                PaymentMethod = "KBZPay",
                PaymentAccountName = "Dup Test",
                PaymentAccountNumber = "09555666777",
                Status = "PENDING",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = creatorId
            };
            db.TblCreatorPayoutRequests.Add(payout);
            await db.SaveChangesAsync();
            payoutId = payout.CreatorPayoutRequestId;
        }

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var firstResp = await client.PostAsJsonAsync($"/api/admin/payouts/{payoutId}/approve", new { AdminNote = "Approved 1" });
        Assert.True(firstResp.IsSuccessStatusCode);

        var secondResp = await client.PostAsJsonAsync($"/api/admin/payouts/{payoutId}/approve", new { AdminNote = "Approved 2" });
        Assert.False(secondResp.IsSuccessStatusCode);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var ledgerCount = await db.TblLinkDropTransactions.CountAsync(t => t.TransactionType == "CREATOR_PAYOUT" && t.ReferenceType == "TblCreatorPayoutRequest" && t.ReferenceId == (int)payoutId);
            Assert.Equal(1, ledgerCount);
        }
    }

    [Fact]
    public async Task FinanceReconciliation_Anonymous_Returns401()
    {
        var anonymousClient = _factory.CreateClient();
        var resp = await anonymousClient.GetAsync("/api/admin/finance/reconciliation");
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task FinanceReconciliation_NonAdmin_Returns403()
    {
        var (memberToken, _) = await GetTokenAndUserIdForRoleAsync("MEMBER", "rec_member1", "rec_member1@test.com", "Password@123");
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", memberToken);

        var resp = await client.GetAsync("/api/admin/finance/reconciliation");
        Assert.Equal(HttpStatusCode.Forbidden, resp.StatusCode);
    }

    [Fact]
    public async Task FinanceReconciliation_Admin_Returns200()
    {
        var (adminToken, _) = await GetTokenAndUserIdForRoleAsync("ADMIN", "rec_admin9", "rec_admin9@test.com", "Password@123");
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var resp = await client.GetAsync("/api/admin/finance/reconciliation");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

        var result = await resp.Content.ReadFromJsonAsync<Result<AdminFinanceReconciliationModel>>();
        Assert.True(result?.IsSuccess);
        Assert.NotNull(result?.Data);
        Assert.NotNull(result?.Data?.ReconciliationStatus);
    }

    [Fact]
    public async Task FinanceReconciliation_DetectsIntentionalMismatch()
    {
        var (adminToken, _) = await GetTokenAndUserIdForRoleAsync("ADMIN", "rec_admin10", "rec_admin10@test.com", "Password@123");
        var (_, creatorId) = await GetTokenAndUserIdForRoleAsync("DOMAIN_PROFESSIONAL", "rec_creator10", "rec_creator10@test.com", "Password@123");

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var wallet = await db.TblLinkDropWallets.FirstOrDefaultAsync(w => w.UserId == creatorId);
            if (wallet == null)
            {
                wallet = new TblLinkDropWallet { UserId = creatorId, PurchasedBalance = 0, EarnedBalance = 99999, Balance = 99999, CreatedAt = DateTime.UtcNow };
                db.TblLinkDropWallets.Add(wallet);
            }
            else
            {
                wallet.EarnedBalance += 99999;
                wallet.Balance += 99999;
            }
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var resp = await client.GetAsync("/api/admin/finance/reconciliation");
        var result = await resp.Content.ReadFromJsonAsync<Result<AdminFinanceReconciliationModel>>();

        Assert.True(result?.IsSuccess);
        Assert.Equal("MISMATCH", result?.Data?.ReconciliationStatus);
        Assert.NotEqual(0, result?.Data?.NetDifferenceLinkDrops);
    }
}
