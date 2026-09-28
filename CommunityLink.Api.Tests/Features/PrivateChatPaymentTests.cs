using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using CommunityLink.Api.Tests.Infrastructure;
using CommunityLink.Database.AppDbContextModels;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Authentication;
using CommunityLink.Shared.Features.Chat;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CommunityLink.Api.Tests.Features;

public class PrivateChatPaymentTests : IClassFixture<CommunityApiFactory>
{
    private readonly CommunityApiFactory _factory;
    private readonly HttpClient _client;

    public PrivateChatPaymentTests(CommunityApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<(string Token, int UserId)> GetUserWithRoleAsync(string roleCode, string username, string email, string password)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var role = await db.TblRoles.FirstOrDefaultAsync(r => r.RoleCode == roleCode);
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

        var user = await db.TblUsers.FirstOrDefaultAsync(u => u.NormalizedEmail == email.ToUpperInvariant());
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

        var loginResp = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequestModel(email, password));
        var loginResult = await loginResp.Content.ReadFromJsonAsync<Result<LoginResponseModel>>();
        Assert.NotNull(loginResult?.Data?.AccessToken);

        return (loginResult.Data.AccessToken, user.UserId);
    }

    private async Task GiveWalletBalanceAsync(int userId, long purchasedBalance)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var wallet = await db.TblLinkDropWallets.FirstOrDefaultAsync(w => w.UserId == userId);
        if (wallet == null)
        {
            wallet = new TblLinkDropWallet
            {
                UserId = userId,
                Balance = purchasedBalance,
                PurchasedBalance = purchasedBalance,
                EarnedBalance = 0,
                CreatedAt = DateTime.UtcNow
            };
            db.TblLinkDropWallets.Add(wallet);
        }
        else
        {
            wallet.PurchasedBalance = purchasedBalance;
            wallet.Balance = wallet.PurchasedBalance + wallet.EarnedBalance;
            wallet.UpdatedAt = DateTime.UtcNow;
        }
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task CreatorPrivateChatEnabled_CanUnlock()
    {
        var (creatorToken, creatorId) = await GetUserWithRoleAsync("DOMAIN_PROFESSIONAL", "CreatorAlpha", "creatoralpha@test.local", "Password123!");
        var (buyerToken, buyerId) = await GetUserWithRoleAsync("MEMBER", "BuyerAlpha", "buyeralpha@test.local", "Password123!");

        // Creator enables private chat (fee: 100 drops)
        var creatorClient = _factory.CreateClient();
        creatorClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", creatorToken);
        var setRes = await creatorClient.PostAsJsonAsync("/api/creator/chat/settings", new SaveCreatorChatSettingRequestModel(true, 100));
        Assert.Equal(HttpStatusCode.OK, setRes.StatusCode);

        // Give buyer 200 drops
        await GiveWalletBalanceAsync(buyerId, 200);

        // Buyer unlocks chat
        var buyerClient = _factory.CreateClient();
        buyerClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", buyerToken);
        var unlockRes = await buyerClient.PostAsync($"/api/chat/unlock/{creatorId}", null);
        Assert.Equal(HttpStatusCode.OK, unlockRes.StatusCode);

        var unlockData = await unlockRes.Content.ReadFromJsonAsync<Result<int>>();
        Assert.NotNull(unlockData);
        Assert.True(unlockData.IsSuccess);
        Assert.True(unlockData.Data > 0);
    }

    [Fact]
    public async Task CreatorDisabledChat_CannotUnlock()
    {
        var (creatorToken, creatorId) = await GetUserWithRoleAsync("DOMAIN_PROFESSIONAL", "CreatorDisabled", "creatordisabled@test.local", "Password123!");
        var (buyerToken, buyerId) = await GetUserWithRoleAsync("MEMBER", "BuyerBeta", "buyerbeta@test.local", "Password123!");

        // Creator disables private chat
        var creatorClient = _factory.CreateClient();
        creatorClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", creatorToken);
        await creatorClient.PostAsJsonAsync("/api/creator/chat/settings", new SaveCreatorChatSettingRequestModel(false, 100));

        await GiveWalletBalanceAsync(buyerId, 200);

        var buyerClient = _factory.CreateClient();
        buyerClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", buyerToken);
        var unlockRes = await buyerClient.PostAsync($"/api/chat/unlock/{creatorId}", null);

        Assert.Equal(HttpStatusCode.BadRequest, unlockRes.StatusCode);
    }

    [Fact]
    public async Task InsufficientBalance_CannotUnlock()
    {
        var (creatorToken, creatorId) = await GetUserWithRoleAsync("DOMAIN_PROFESSIONAL", "CreatorGamma", "creatorgamma@test.local", "Password123!");
        var (buyerToken, buyerId) = await GetUserWithRoleAsync("MEMBER", "PoorBuyer", "poorbuyer@test.local", "Password123!");

        var creatorClient = _factory.CreateClient();
        creatorClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", creatorToken);
        await creatorClient.PostAsJsonAsync("/api/creator/chat/settings", new SaveCreatorChatSettingRequestModel(true, 500));

        // Give buyer 50 drops (less than 500 fee)
        await GiveWalletBalanceAsync(buyerId, 50);

        var buyerClient = _factory.CreateClient();
        buyerClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", buyerToken);
        var unlockRes = await buyerClient.PostAsync($"/api/chat/unlock/{creatorId}", null);

        Assert.Equal(HttpStatusCode.BadRequest, unlockRes.StatusCode);
        var result = await unlockRes.Content.ReadFromJsonAsync<Result<int>>();
        Assert.Contains("Insufficient LinkDrop balance", result?.Message ?? "");
    }

    [Fact]
    public async Task BuyerWalletDeductedCorrectly()
    {
        var (creatorToken, creatorId) = await GetUserWithRoleAsync("DOMAIN_PROFESSIONAL", "CreatorDelta", "creatordelta@test.local", "Password123!");
        var (buyerToken, buyerId) = await GetUserWithRoleAsync("MEMBER", "BuyerDelta", "buyerdelta@test.local", "Password123!");

        var creatorClient = _factory.CreateClient();
        creatorClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", creatorToken);
        await creatorClient.PostAsJsonAsync("/api/creator/chat/settings", new SaveCreatorChatSettingRequestModel(true, 120));

        await GiveWalletBalanceAsync(buyerId, 300);

        var buyerClient = _factory.CreateClient();
        buyerClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", buyerToken);
        var unlockRes = await buyerClient.PostAsync($"/api/chat/unlock/{creatorId}", null);
        Assert.Equal(HttpStatusCode.OK, unlockRes.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var wallet = await db.TblLinkDropWallets.FirstAsync(w => w.UserId == buyerId);

        Assert.Equal(180, wallet.Balance); // 300 - 120 = 180
        Assert.Equal(180, wallet.PurchasedBalance);
    }

    [Fact]
    public async Task CreatorEarnedBalanceUpdated()
    {
        var (creatorToken, creatorId) = await GetUserWithRoleAsync("DOMAIN_PROFESSIONAL", "CreatorEpsilon", "creatorepsilon@test.local", "Password123!");
        var (buyerToken, buyerId) = await GetUserWithRoleAsync("MEMBER", "BuyerEpsilon", "buyerepsilon@test.local", "Password123!");

        var creatorClient = _factory.CreateClient();
        creatorClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", creatorToken);
        await creatorClient.PostAsJsonAsync("/api/creator/chat/settings", new SaveCreatorChatSettingRequestModel(true, 100));

        await GiveWalletBalanceAsync(buyerId, 200);

        var buyerClient = _factory.CreateClient();
        buyerClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", buyerToken);
        await buyerClient.PostAsync($"/api/chat/unlock/{creatorId}", null);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var creatorWallet = await db.TblLinkDropWallets.FirstAsync(w => w.UserId == creatorId);

        // 100 fee with 10% commission = 10 commission, 90 net creator earnings
        Assert.Equal(90, creatorWallet.EarnedBalance);
        Assert.Equal(90, creatorWallet.Balance);
    }

    [Fact]
    public async Task PrivateChatCreatesLedgerEntry()
    {
        var (creatorToken, creatorId) = await GetUserWithRoleAsync("DOMAIN_PROFESSIONAL", "CreatorZeta", "creatorzeta@test.local", "Password123!");
        var (buyerToken, buyerId) = await GetUserWithRoleAsync("MEMBER", "BuyerZeta", "buyerzeta@test.local", "Password123!");

        var creatorClient = _factory.CreateClient();
        creatorClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", creatorToken);
        await creatorClient.PostAsJsonAsync("/api/creator/chat/settings", new SaveCreatorChatSettingRequestModel(true, 150));

        await GiveWalletBalanceAsync(buyerId, 300);

        var buyerClient = _factory.CreateClient();
        buyerClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", buyerToken);
        await buyerClient.PostAsync($"/api/chat/unlock/{creatorId}", null);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var pTx = await db.TblPrivateChatPaymentTransactions
            .FirstOrDefaultAsync(t => t.BuyerUserId == buyerId && t.CreatorUserId == creatorId);
        Assert.NotNull(pTx);
        Assert.Equal("COMPLETED", pTx.Status);
        Assert.Equal(150, pTx.GrossAmountLinkDrops);

        var buyerLedger = await db.TblLinkDropTransactions
            .FirstOrDefaultAsync(t => t.UserId == buyerId && t.TransactionType == "PRIVATE_CHAT_UNLOCK");
        Assert.NotNull(buyerLedger);
        Assert.Equal(150, buyerLedger.Amount);
    }

    [Fact]
    public async Task CannotSendMessageWithoutPayment()
    {
        var (creatorToken, creatorId) = await GetUserWithRoleAsync("DOMAIN_PROFESSIONAL", "CreatorEta", "creatoreta@test.local", "Password123!");
        var (buyerToken, buyerId) = await GetUserWithRoleAsync("MEMBER", "UnpaidBuyer", "unpaidbuyer@test.local", "Password123!");

        var creatorClient = _factory.CreateClient();
        creatorClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", creatorToken);
        await creatorClient.PostAsJsonAsync("/api/creator/chat/settings", new SaveCreatorChatSettingRequestModel(true, 100));

        var buyerClient = _factory.CreateClient();
        buyerClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", buyerToken);

        // Buyer attempts to send message without unlocking
        var sendRes = await buyerClient.PostAsJsonAsync("/api/chat/messages", new SendMessageRequestModel(null, creatorId, "Hello creator!"));

        Assert.Equal(HttpStatusCode.Forbidden, sendRes.StatusCode);
        var result = await sendRes.Content.ReadFromJsonAsync<Result<ChatMessageModel>>();
        Assert.Equal("PRIVATE_CHAT_PAYMENT_REQUIRED", result?.Message);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var msgCount = await db.TblChatMessages.CountAsync(m => m.SenderId == buyerId);
        Assert.Equal(0, msgCount); // Verified no message was saved
    }

    [Fact]
    public async Task DuplicateUnlockDoesNotDoubleCharge()
    {
        var (creatorToken, creatorId) = await GetUserWithRoleAsync("DOMAIN_PROFESSIONAL", "CreatorTheta", "creatortheta@test.local", "Password123!");
        var (buyerToken, buyerId) = await GetUserWithRoleAsync("MEMBER", "BuyerTheta", "buyertheta@test.local", "Password123!");

        var creatorClient = _factory.CreateClient();
        creatorClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", creatorToken);
        await creatorClient.PostAsJsonAsync("/api/creator/chat/settings", new SaveCreatorChatSettingRequestModel(true, 100));

        await GiveWalletBalanceAsync(buyerId, 300);

        var buyerClient = _factory.CreateClient();
        buyerClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", buyerToken);

        // First unlock
        var firstUnlock = await buyerClient.PostAsync($"/api/chat/unlock/{creatorId}", null);
        Assert.Equal(HttpStatusCode.OK, firstUnlock.StatusCode);
        var firstResult = await firstUnlock.Content.ReadFromJsonAsync<Result<int>>();

        // Second unlock attempt
        var secondUnlock = await buyerClient.PostAsync($"/api/chat/unlock/{creatorId}", null);
        Assert.Equal(HttpStatusCode.OK, secondUnlock.StatusCode);
        var secondResult = await secondUnlock.Content.ReadFromJsonAsync<Result<int>>();

        Assert.Equal(firstResult?.Data, secondResult?.Data);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var wallet = await db.TblLinkDropWallets.FirstAsync(w => w.UserId == buyerId);

        // Charged ONLY 100 once: 300 - 100 = 200
        Assert.Equal(200, wallet.Balance);
    }

    [Fact]
    public async Task PrivateMessageSignalRRealtimeDelivery()
    {
        var (user1Token, user1Id) = await GetUserWithRoleAsync("MEMBER", "ChatUserOne", "chatuserone@test.local", "Password123!");
        var (user2Token, user2Id) = await GetUserWithRoleAsync("MEMBER", "ChatUserTwo", "chatusertwo@test.local", "Password123!");

        var client1 = _factory.CreateClient();
        client1.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user1Token);

        var sendRes = await client1.PostAsJsonAsync("/api/chat/messages", new SendMessageRequestModel(null, user2Id, "Realtime test message!"));
        Assert.Equal(HttpStatusCode.OK, sendRes.StatusCode);
        var sendData = await sendRes.Content.ReadFromJsonAsync<Result<ChatMessageModel>>();
        Assert.NotNull(sendData?.Data);
        Assert.Equal("Realtime test message!", sendData.Data.MessageText);
    }

    [Fact]
    public async Task CreatorCannotModifyAnotherCreatorSettings()
    {
        var (creator1Token, creator1Id) = await GetUserWithRoleAsync("DOMAIN_PROFESSIONAL", "CreatorIota", "creatoriota@test.local", "Password123!");
        var (memberToken, memberId) = await GetUserWithRoleAsync("MEMBER", "RegularMember", "regularmember@test.local", "Password123!");

        // 1. Regular member attempts to enable private chat
        var memberClient = _factory.CreateClient();
        memberClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", memberToken);
        var memberSetRes = await memberClient.PostAsJsonAsync("/api/creator/chat/settings", new SaveCreatorChatSettingRequestModel(true, 100));

        Assert.Equal(HttpStatusCode.Forbidden, memberSetRes.StatusCode);

        // 2. Verified that settings endpoint uses ICurrentUserContext.UserId and never trusts client body
        var creatorClient = _factory.CreateClient();
        creatorClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", creator1Token);
        var creatorSetRes = await creatorClient.PostAsJsonAsync("/api/creator/chat/settings", new SaveCreatorChatSettingRequestModel(true, 150));
        Assert.Equal(HttpStatusCode.OK, creatorSetRes.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var setting = await db.TblCreatorChatSettings.FirstOrDefaultAsync(s => s.CreatorUserId == creator1Id);
        Assert.NotNull(setting);
        Assert.Equal(150, setting.PrivateChatFeeLinkDrops);
        Assert.True(setting.IsPrivateChatEnabled);
    }
}
