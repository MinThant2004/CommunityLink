using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CommunityLink.Api.Tests.Infrastructure;
using CommunityLink.Database.AppDbContextModels;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Authentication;
using CommunityLink.Shared.Features.ChatGroup;
using CommunityLink.Shared.Features.Creator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CommunityLink.Api.Tests.Features;

public class CreatorEarningsTests : IClassFixture<CommunityApiFactory>
{
    private readonly CommunityApiFactory _factory;
    private readonly HttpClient _client;

    public CreatorEarningsTests(CommunityApiFactory factory)
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

    private async Task TopUpUserWalletAsync(int userId, long purchasedAmount)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var wallet = await db.TblLinkDropWallets.FirstOrDefaultAsync(w => w.UserId == userId);
        if (wallet == null)
        {
            wallet = new TblLinkDropWallet
            {
                UserId = userId,
                Balance = purchasedAmount,
                PurchasedBalance = purchasedAmount,
                EarnedBalance = 0,
                CreatedAt = DateTime.UtcNow
            };
            db.TblLinkDropWallets.Add(wallet);
        }
        else
        {
            wallet.PurchasedBalance += purchasedAmount;
            wallet.Balance = wallet.PurchasedBalance + wallet.EarnedBalance;
        }

        await db.SaveChangesAsync();
    }

    private async Task<int> CreatePaidChatGroupAsync(string creatorToken, string name, long feeLinkDrops)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", creatorToken);

        var createResp = await client.PostAsJsonAsync("/api/chat-groups", new CreateChatGroupRequestModel(name, "Group Description", null, "PAID", feeLinkDrops));
        var createResult = await createResp.Content.ReadFromJsonAsync<Result<ChatGroupModel>>();
        Assert.NotNull(createResult?.Data);

        int groupId = createResult.Data.ChatGroupId;

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var group = await db.TblChatGroups.FirstAsync(g => g.ChatGroupId == groupId);
        group.IsActive = true;
        await db.SaveChangesAsync();

        return groupId;
    }

    [Fact]
    public async Task GetEarnings_EmptyCreator_ReturnsZeroEarningsAndEmptyState()
    {
        var (creatorToken, _) = await GetTokenAndUserIdForRoleAsync("DOMAIN_PROFESSIONAL", "earn_creator0", "earn_creator0@test.com", "Password@123");

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", creatorToken);

        var resp = await client.GetAsync("/api/creator/earnings");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

        var result = await resp.Content.ReadFromJsonAsync<Result<CreatorEarningsDashboardModel>>();
        Assert.True(result?.IsSuccess);
        Assert.NotNull(result?.Data);

        Assert.Equal(0, result.Data.Summary.AvailableEarnings);
        Assert.Equal(0, result.Data.Summary.TotalEarned);
        Assert.Equal(0, result.Data.Summary.TotalCommission);
        Assert.Equal(0, result.Data.Summary.TotalTransactions);
        Assert.Equal(0m, result.Data.Summary.EstimatedValueMmk);
        Assert.Empty(result.Data.Transactions);
    }

    [Fact]
    public async Task GetEarnings_PurchasedLinkDrops_AreNotCountedAsCreatorEarnings()
    {
        var (creatorToken, creatorId) = await GetTokenAndUserIdForRoleAsync("DOMAIN_PROFESSIONAL", "earn_creator1", "earn_creator1@test.com", "Password@123");

        // Top up 500 Purchased Link Drops for creator
        await TopUpUserWalletAsync(creatorId, 500);

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", creatorToken);

        var resp = await client.GetAsync("/api/creator/earnings");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

        var result = await resp.Content.ReadFromJsonAsync<Result<CreatorEarningsDashboardModel>>();
        Assert.True(result?.IsSuccess);
        Assert.NotNull(result?.Data);

        // MUST be 0 (Purchased Link Drops excluded)
        Assert.Equal(0, result.Data.Summary.AvailableEarnings);
        Assert.Equal(0, result.Data.Summary.NetEarnings);
    }

    [Fact]
    public async Task GetEarnings_PaidGroupJoin_CalculatesGrossCommissionAndNetCorrectly()
    {
        var (creatorToken, _) = await GetTokenAndUserIdForRoleAsync("DOMAIN_PROFESSIONAL", "earn_creator2", "earn_creator2@test.com", "Password@123");
        var (buyerToken, buyerId) = await GetTokenAndUserIdForRoleAsync("MEMBER", "earn_buyer2", "earn_buyer2@test.com", "Password@123");

        int groupId = await CreatePaidChatGroupAsync(creatorToken, "C# Mastery Group", 50);

        // Buyer tops up 100 Link Drops and joins paid group
        await TopUpUserWalletAsync(buyerId, 100);

        var buyerClient = _factory.CreateClient();
        buyerClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", buyerToken);
        var joinResp = await buyerClient.PostAsJsonAsync($"/api/chat-groups/{groupId}/join-paid", new { });
        Assert.Equal(HttpStatusCode.OK, joinResp.StatusCode);

        // Check Creator Earnings dashboard
        var creatorClient = _factory.CreateClient();
        creatorClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", creatorToken);

        var resp = await creatorClient.GetAsync("/api/creator/earnings");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

        var result = await resp.Content.ReadFromJsonAsync<Result<CreatorEarningsDashboardModel>>();
        Assert.True(result?.IsSuccess);
        Assert.NotNull(result?.Data);

        // 50 LD fee with 10% default platform commission = 5 LD commission, 45 LD net
        Assert.Equal(50, result.Data.Summary.GrossEarnings);
        Assert.Equal(5, result.Data.Summary.PlatformCommission);
        Assert.Equal(45, result.Data.Summary.NetEarnings);
        Assert.Equal(45, result.Data.Summary.AvailableEarnings);
        Assert.Equal(1, result.Data.Summary.TotalTransactions);
        Assert.Equal(4500m, result.Data.Summary.EstimatedValueMmk); // 45 * 100

        Assert.Single(result.Data.Transactions);
        var tx = result.Data.Transactions[0];
        Assert.Equal("C# Mastery Group", tx.ChatGroupName);
        Assert.Equal(50, tx.GrossAmount);
        Assert.Equal(5, tx.CommissionAmount);
        Assert.Equal(45, tx.NetAmount);
        Assert.Equal("COMPLETED", tx.Status);
    }

    [Fact]
    public async Task GetEarnings_Security_CreatorCannotViewAnotherCreatorsEarnings()
    {
        var (creatorAToken, _) = await GetTokenAndUserIdForRoleAsync("DOMAIN_PROFESSIONAL", "creator_sec_A", "creator_sec_A@test.com", "Password@123");
        var (creatorBToken, _) = await GetTokenAndUserIdForRoleAsync("DOMAIN_PROFESSIONAL", "creator_sec_B", "creator_sec_B@test.com", "Password@123");
        var (buyerToken, buyerId) = await GetTokenAndUserIdForRoleAsync("MEMBER", "buyer_sec", "buyer_sec@test.com", "Password@123");

        int groupA = await CreatePaidChatGroupAsync(creatorAToken, "Creator A Exclusive", 100);

        await TopUpUserWalletAsync(buyerId, 200);

        var buyerClient = _factory.CreateClient();
        buyerClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", buyerToken);
        await buyerClient.PostAsJsonAsync($"/api/chat-groups/{groupA}/join-paid", new { });

        // Creator B queries /api/creator/earnings
        var creatorBClient = _factory.CreateClient();
        creatorBClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", creatorBToken);

        var respB = await creatorBClient.GetAsync("/api/creator/earnings");
        var resultB = await respB.Content.ReadFromJsonAsync<Result<CreatorEarningsDashboardModel>>();

        // Creator B must see 0 earnings (cannot see Creator A's transactions)
        Assert.True(resultB?.IsSuccess);
        Assert.Equal(0, resultB?.Data?.Summary.AvailableEarnings);
        Assert.Empty(resultB?.Data?.Transactions ?? new List<CreatorEarningsTransactionModel>());
    }

    [Fact]
    public async Task GetEarnings_MultipleChatGroups_GroupedCorrectlyInBreakdown()
    {
        var (creatorToken, _) = await GetTokenAndUserIdForRoleAsync("DOMAIN_PROFESSIONAL", "multi_creator", "multi_creator@test.com", "Password@123");
        var (buyerToken, buyerId) = await GetTokenAndUserIdForRoleAsync("MEMBER", "multi_buyer", "multi_buyer@test.com", "Password@123");

        int group1 = await CreatePaidChatGroupAsync(creatorToken, "Mg Mg C# Group", 50);
        int group2 = await CreatePaidChatGroupAsync(creatorToken, "Mg Mg .NET Group", 100);

        await TopUpUserWalletAsync(buyerId, 300);

        var buyerClient = _factory.CreateClient();
        buyerClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", buyerToken);

        await buyerClient.PostAsJsonAsync($"/api/chat-groups/{group1}/join-paid", new { });
        await buyerClient.PostAsJsonAsync($"/api/chat-groups/{group2}/join-paid", new { });

        var creatorClient = _factory.CreateClient();
        creatorClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", creatorToken);

        var resp = await creatorClient.GetAsync("/api/creator/earnings");
        var result = await resp.Content.ReadFromJsonAsync<Result<CreatorEarningsDashboardModel>>();

        Assert.True(result?.IsSuccess);
        Assert.NotNull(result?.Data);

        // Group 1: 50 gross, 5 comm, 45 net
        // Group 2: 100 gross, 10 comm, 90 net
        // Total Net: 135 LD
        Assert.Equal(150, result.Data.Summary.GrossEarnings);
        Assert.Equal(15, result.Data.Summary.PlatformCommission);
        Assert.Equal(135, result.Data.Summary.NetEarnings);
        Assert.Equal(2, result.Data.Summary.TotalTransactions);

        Assert.Equal(2, result.Data.GroupBreakdown.Count);
        var g2Breakdown = result.Data.GroupBreakdown.First(g => g.ChatGroupId == group2);
        Assert.Equal("Mg Mg .NET Group", g2Breakdown.ChatGroupName);
        Assert.Equal(90, g2Breakdown.TotalEarned);

        var g1Breakdown = result.Data.GroupBreakdown.First(g => g.ChatGroupId == group1);
        Assert.Equal("Mg Mg C# Group", g1Breakdown.ChatGroupName);
        Assert.Equal(45, g1Breakdown.TotalEarned);
    }
}
