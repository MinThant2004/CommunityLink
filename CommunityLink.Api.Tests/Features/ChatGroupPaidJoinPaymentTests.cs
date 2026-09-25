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

public class ChatGroupPaidJoinPaymentTests : IClassFixture<CommunityApiFactory>
{
    private readonly CommunityApiFactory _factory;
    private readonly HttpClient _client;

    public ChatGroupPaidJoinPaymentTests(CommunityApiFactory factory)
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
            wallet = new TblLinkDropWallet
            {
                UserId = userId,
                PurchasedBalance = purchasedBalance,
                EarnedBalance = earnedBalance,
                Balance = purchasedBalance + earnedBalance,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };
            db.TblLinkDropWallets.Add(wallet);
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
            setting = new TblPlatformSetting
            {
                SettingKey = "PlatformCommissionPercentage",
                SettingValue = percentage.ToString(System.Globalization.CultureInfo.InvariantCulture),
                DataType = "DECIMAL",
                Description = "Platform commission percentage",
                UpdatedAt = DateTime.UtcNow
            };
            db.TblPlatformSettings.Add(setting);
        }
        else
        {
            setting.SettingValue = percentage.ToString(System.Globalization.CultureInfo.InvariantCulture);
            setting.UpdatedAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task PaidJoin_Success_DeductsWallet_CreatesEarning_CreatesPaymentTransactionAndAuditLedgers()
    {
        // 1. Setup commission at 10%
        await SetCommissionPercentageAsync(10.00m);

        // 2. Creator creates PAID Chat Group with fee = 100
        var (creatorToken, creatorId) = await GetTokenAndUserIdForRoleAsync("DOMAIN_PROFESSIONAL", "paid_creator1", "paid_creator1@test.com", "Password@123");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", creatorToken);

        var createResp = await _client.PostAsJsonAsync("/api/chat-groups", new CreateChatGroupRequestModel("Premium C# Masters", "Exclusive paid group", null, "PAID", 100));
        var createResult = await createResp.Content.ReadFromJsonAsync<Result<ChatGroupModel>>();
        Assert.NotNull(createResult?.Data);
        var groupId = createResult.Data.ChatGroupId;

        // 3. User sets up wallet with 150 Purchased, 50 Earned (Total 200)
        var (userToken, userId) = await GetTokenAndUserIdForRoleAsync("MEMBER", "paid_member1", "paid_member1@test.com", "Password@123");
        await SetupWalletAsync(userId, 150, 50);

        // 4. User joins paid chat group
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);
        var joinResp = await _client.PostAsJsonAsync($"/api/chat-groups/{groupId}/join-paid", "");
        Assert.Equal(HttpStatusCode.OK, joinResp.StatusCode);
        var joinResult = await joinResp.Content.ReadFromJsonAsync<Result>();
        Assert.True(joinResult?.IsSuccess);

        // 5. Verify database state
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Verify User Wallet: 150 - 100 = 50 Purchased, 50 Earned, Total 100
        var userWallet = await db.TblLinkDropWallets.FirstOrDefaultAsync(w => w.UserId == userId);
        Assert.NotNull(userWallet);
        Assert.Equal(50, userWallet.PurchasedBalance);
        Assert.Equal(50, userWallet.EarnedBalance);
        Assert.Equal(100, userWallet.Balance);

        // Verify Creator Wallet: 90 Earned (100 fee - 10% commission = 90 net)
        var creatorWallet = await db.TblLinkDropWallets.FirstOrDefaultAsync(w => w.UserId == creatorId);
        Assert.NotNull(creatorWallet);
        Assert.Equal(90, creatorWallet.EarnedBalance);
        Assert.Equal(90, creatorWallet.Balance);

        // Verify TblChatGroupPaymentTransaction
        var paymentTx = await db.TblChatGroupPaymentTransactions
            .FirstOrDefaultAsync(p => p.ChatGroupId == groupId && p.UserId == userId);
        Assert.NotNull(paymentTx);
        Assert.Equal(100, paymentTx.GrossAmount);
        Assert.Equal(10.00m, paymentTx.CommissionPercentage);
        Assert.Equal(10, paymentTx.CommissionAmount);
        Assert.Equal(90, paymentTx.NetAmount);
        Assert.Equal(100, paymentTx.PurchasedAmountDeducted);
        Assert.Equal(0, paymentTx.EarnedAmountDeducted);
        Assert.Equal("COMPLETED", paymentTx.Status);

        // Verify TblChatGroupMember created
        var member = await db.TblChatGroupMembers
            .FirstOrDefaultAsync(m => m.ChatGroupId == groupId && m.UserId == userId);
        Assert.NotNull(member);
        Assert.False(member.IsDeleted);
        Assert.Equal("MEMBER", member.Role);

        // Verify TblLinkDropTransaction ledgers
        var userLedger = await db.TblLinkDropTransactions
            .FirstOrDefaultAsync(t => t.UserId == userId && t.TransactionType == "CHAT_GROUP_JOIN");
        Assert.NotNull(userLedger);
        Assert.Equal(100, userLedger.Amount);
        Assert.Equal(200, userLedger.BalanceBefore);
        Assert.Equal(100, userLedger.BalanceAfter);

        var creatorLedger = await db.TblLinkDropTransactions
            .FirstOrDefaultAsync(t => t.UserId == creatorId && t.TransactionType == "CHAT_GROUP_EARNING");
        Assert.NotNull(creatorLedger);
        Assert.Equal(90, creatorLedger.Amount);
        Assert.Equal(0, creatorLedger.BalanceBefore);
        Assert.Equal(90, creatorLedger.BalanceAfter);
    }

    [Fact]
    public async Task PaidJoin_InsufficientBalance_ReturnsValidationError()
    {
        // 1. Creator creates PAID Chat Group with fee = 500
        var (creatorToken, _) = await GetTokenAndUserIdForRoleAsync("DOMAIN_PROFESSIONAL", "paid_creator2", "paid_creator2@test.com", "Password@123");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", creatorToken);

        var createResp = await _client.PostAsJsonAsync("/api/chat-groups", new CreateChatGroupRequestModel("High Tier Group", "Expensive group", null, "PAID", 500));
        var createResult = await createResp.Content.ReadFromJsonAsync<Result<ChatGroupModel>>();
        Assert.NotNull(createResult?.Data);
        var groupId = createResult.Data.ChatGroupId;

        // 2. User has only 100 balance
        var (userToken, userId) = await GetTokenAndUserIdForRoleAsync("MEMBER", "poor_member1", "poor_member1@test.com", "Password@123");
        await SetupWalletAsync(userId, 100, 0);

        // 3. User attempts to join
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);
        var joinResp = await _client.PostAsJsonAsync($"/api/chat-groups/{groupId}/join-paid", "");
        Assert.Equal(HttpStatusCode.BadRequest, joinResp.StatusCode);
        var joinResult = await joinResp.Content.ReadFromJsonAsync<Result>();
        Assert.False(joinResult?.IsSuccess);
        Assert.Contains("Insufficient Link Drops balance", joinResult?.Message);

        // 4. Verify wallet unchanged
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userWallet = await db.TblLinkDropWallets.FirstOrDefaultAsync(w => w.UserId == userId);
        Assert.NotNull(userWallet);
        Assert.Equal(100, userWallet.Balance);

        // 5. Verify no member created
        var memberExists = await db.TblChatGroupMembers.AnyAsync(m => m.ChatGroupId == groupId && m.UserId == userId);
        Assert.False(memberExists);
    }

    [Fact]
    public async Task PaidJoin_CommissionCalculation_CalculatesCorrectSnapshots()
    {
        // 1. Set commission to 20%
        await SetCommissionPercentageAsync(20.00m);

        // 2. Creator creates PAID Chat Group with fee = 250
        var (creatorToken, creatorId) = await GetTokenAndUserIdForRoleAsync("DOMAIN_PROFESSIONAL", "paid_creator3", "paid_creator3@test.com", "Password@123");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", creatorToken);

        var createResp = await _client.PostAsJsonAsync("/api/chat-groups", new CreateChatGroupRequestModel("AI Deep Dive", "Paid AI Group", null, "PAID", 250));
        var createResult = await createResp.Content.ReadFromJsonAsync<Result<ChatGroupModel>>();
        Assert.NotNull(createResult?.Data);
        var groupId = createResult.Data.ChatGroupId;

        // 3. User with 500 balance joins
        var (userToken, userId) = await GetTokenAndUserIdForRoleAsync("MEMBER", "comm_user1", "comm_user1@test.com", "Password@123");
        await SetupWalletAsync(userId, 500, 0);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);
        var joinResp = await _client.PostAsJsonAsync($"/api/chat-groups/{groupId}/join-paid", "");
        Assert.Equal(HttpStatusCode.OK, joinResp.StatusCode);

        // 4. Verify Payment Transaction commission calculation
        // 20% of 250 = 50 commission, net = 200
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var paymentTx = await db.TblChatGroupPaymentTransactions
            .FirstOrDefaultAsync(p => p.ChatGroupId == groupId && p.UserId == userId);
        Assert.NotNull(paymentTx);
        Assert.Equal(250, paymentTx.GrossAmount);
        Assert.Equal(20.00m, paymentTx.CommissionPercentage);
        Assert.Equal(50, paymentTx.CommissionAmount);
        Assert.Equal(200, paymentTx.NetAmount);

        // Verify Creator received 200 net
        var creatorWallet = await db.TblLinkDropWallets.FirstOrDefaultAsync(w => w.UserId == creatorId);
        Assert.NotNull(creatorWallet);
        Assert.Equal(200, creatorWallet.EarnedBalance);
    }

    [Fact]
    public async Task PaidJoin_DuplicateJoin_PreventsDoublePayment()
    {
        // 1. Creator creates PAID Chat Group with fee = 80
        var (creatorToken, _) = await GetTokenAndUserIdForRoleAsync("DOMAIN_PROFESSIONAL", "paid_creator4", "paid_creator4@test.com", "Password@123");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", creatorToken);

        var createResp = await _client.PostAsJsonAsync("/api/chat-groups", new CreateChatGroupRequestModel("Dotnet Core Group", "Paid dotnet group", null, "PAID", 80));
        var createResult = await createResp.Content.ReadFromJsonAsync<Result<ChatGroupModel>>();
        Assert.NotNull(createResult?.Data);
        var groupId = createResult.Data.ChatGroupId;

        // 2. User joins first time
        var (userToken, userId) = await GetTokenAndUserIdForRoleAsync("MEMBER", "dup_user1", "dup_user1@test.com", "Password@123");
        await SetupWalletAsync(userId, 300, 0);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);
        var firstJoinResp = await _client.PostAsJsonAsync($"/api/chat-groups/{groupId}/join-paid", "");
        Assert.Equal(HttpStatusCode.OK, firstJoinResp.StatusCode);

        // 3. User attempts second join
        var secondJoinResp = await _client.PostAsJsonAsync($"/api/chat-groups/{groupId}/join-paid", "");
        Assert.Equal(HttpStatusCode.Conflict, secondJoinResp.StatusCode);
        var secondResult = await secondJoinResp.Content.ReadFromJsonAsync<Result>();
        Assert.False(secondResult?.IsSuccess);
        Assert.Contains("Already joined", secondResult?.Message);

        // 4. Verify user was only billed once (300 - 80 = 220)
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userWallet = await db.TblLinkDropWallets.FirstOrDefaultAsync(w => w.UserId == userId);
        Assert.NotNull(userWallet);
        Assert.Equal(220, userWallet.Balance);
    }

    [Fact]
    public async Task PaidJoin_WalletBalanceDeduction_DeductsPurchasedThenEarned()
    {
        // 1. Creator creates PAID Chat Group with fee = 50
        var (creatorToken, _) = await GetTokenAndUserIdForRoleAsync("DOMAIN_PROFESSIONAL", "paid_creator5", "paid_creator5@test.com", "Password@123");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", creatorToken);

        var createResp = await _client.PostAsJsonAsync("/api/chat-groups", new CreateChatGroupRequestModel("Split Balance Group", "Fee 50", null, "PAID", 50));
        var createResult = await createResp.Content.ReadFromJsonAsync<Result<ChatGroupModel>>();
        Assert.NotNull(createResult?.Data);
        var groupId = createResult.Data.ChatGroupId;

        // 2. User has 30 Purchased balance, 100 Earned balance (Total 130)
        var (userToken, userId) = await GetTokenAndUserIdForRoleAsync("MEMBER", "split_user1", "split_user1@test.com", "Password@123");
        await SetupWalletAsync(userId, 30, 100);

        // 3. User joins paid group requiring 50 drops
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);
        var joinResp = await _client.PostAsJsonAsync($"/api/chat-groups/{groupId}/join-paid", "");
        Assert.Equal(HttpStatusCode.OK, joinResp.StatusCode);

        // 4. Verify wallet balance breakdown:
        // All 30 Purchased drops used, remaining 20 deducted from Earned drops (100 - 20 = 80)
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userWallet = await db.TblLinkDropWallets.FirstOrDefaultAsync(w => w.UserId == userId);
        Assert.NotNull(userWallet);
        Assert.Equal(0, userWallet.PurchasedBalance);
        Assert.Equal(80, userWallet.EarnedBalance);
        Assert.Equal(80, userWallet.Balance);

        // Verify Payment Transaction record
        var paymentTx = await db.TblChatGroupPaymentTransactions.FirstOrDefaultAsync(p => p.ChatGroupId == groupId && p.UserId == userId);
        Assert.NotNull(paymentTx);
        Assert.Equal(30, paymentTx.PurchasedAmountDeducted);
        Assert.Equal(20, paymentTx.EarnedAmountDeducted);
    }

    [Fact]
    public async Task PaidJoin_TransactionAuditVerification_RecordsCorrectLedgerSnapshots()
    {
        // 1. Creator creates PAID Chat Group with fee = 60
        var (creatorToken, creatorId) = await GetTokenAndUserIdForRoleAsync("DOMAIN_PROFESSIONAL", "paid_creator6", "paid_creator6@test.com", "Password@123");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", creatorToken);

        var createResp = await _client.PostAsJsonAsync("/api/chat-groups", new CreateChatGroupRequestModel("Audit Test Group", "Fee 60", null, "PAID", 60));
        var createResult = await createResp.Content.ReadFromJsonAsync<Result<ChatGroupModel>>();
        Assert.NotNull(createResult?.Data);
        var groupId = createResult.Data.ChatGroupId;

        // 2. User joins with 200 Purchased balance
        var (userToken, userId) = await GetTokenAndUserIdForRoleAsync("MEMBER", "audit_user1", "audit_user1@test.com", "Password@123");
        await SetupWalletAsync(userId, 200, 0);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);
        var joinResp = await _client.PostAsJsonAsync($"/api/chat-groups/{groupId}/join-paid", "");
        Assert.Equal(HttpStatusCode.OK, joinResp.StatusCode);

        // 3. Verify Ledger Audit Snapshots
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var paymentTx = await db.TblChatGroupPaymentTransactions.FirstOrDefaultAsync(p => p.ChatGroupId == groupId && p.UserId == userId);
        Assert.NotNull(paymentTx);

        var userLedger = await db.TblLinkDropTransactions
            .FirstOrDefaultAsync(t => t.UserId == userId && t.ReferenceId == (int)paymentTx.PaymentTransactionId);
        Assert.NotNull(userLedger);
        Assert.Equal("TblChatGroupPaymentTransaction", userLedger.ReferenceType);
        Assert.Equal(60, userLedger.Amount);
        Assert.Equal(200, userLedger.BalanceBefore);
        Assert.Equal(140, userLedger.BalanceAfter);
        Assert.False(string.IsNullOrWhiteSpace(userLedger.Notes));

        var creatorLedger = await db.TblLinkDropTransactions
            .FirstOrDefaultAsync(t => t.UserId == creatorId && t.ReferenceId == (int)paymentTx.PaymentTransactionId);
        Assert.NotNull(creatorLedger);
        Assert.Equal("TblChatGroupPaymentTransaction", creatorLedger.ReferenceType);
        Assert.False(string.IsNullOrWhiteSpace(creatorLedger.Notes));
    }
}
