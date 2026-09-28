using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CommunityLink.Api.Tests.Infrastructure;
using CommunityLink.Database.AppDbContextModels;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Authentication;
using CommunityLink.Shared.Features.Chat;
using CommunityLink.Shared.Features.ChatGroup;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CommunityLink.Api.Tests.Features;

/// <summary>
/// Covers the 1:1 and group-preview read paths that the unified /chat surface depends on:
/// conversation membership checks, read receipts and the group preview projection.
/// </summary>
public class UnifiedChatReadTests : IClassFixture<CommunityApiFactory>
{
    private readonly CommunityApiFactory _factory;
    private readonly HttpClient _client;

    public UnifiedChatReadTests(CommunityApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<(string Token, int UserId)> GetTokenAndUserIdForRoleAsync(
        string roleCode, string username, string email, string password)
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

        var login = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequestModel(email, password));
        var loginResult = await login.Content.ReadFromJsonAsync<Result<LoginResponseModel>>();
        Assert.NotNull(loginResult?.Data?.AccessToken);
        return (loginResult.Data.AccessToken, user.UserId);
    }

    private void UseToken(string token) =>
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

    private async Task<int> CreateConversationAsync(string senderToken, int targetUserId, string text)
    {
        UseToken(senderToken);
        var resp = await _client.PostAsJsonAsync(
            "/api/chat/messages",
            new SendMessageRequestModel(null, targetUserId, text));
        var result = await resp.Content.ReadFromJsonAsync<Result<ChatMessageModel>>();
        Assert.NotNull(result?.Data);
        return result.Data.ConversationId;
    }

    [Fact]
    public async Task GetMessages_NonParticipant_IsRejected()
    {
        var (aliceToken, aliceId) = await GetTokenAndUserIdForRoleAsync(
            "MEMBER", "unified_alice", "unified_alice@test.com", "Password@123");
        var (bobToken, bobId) = await GetTokenAndUserIdForRoleAsync(
            "MEMBER", "unified_bob", "unified_bob@test.com", "Password@123");
        var (malloryToken, _) = await GetTokenAndUserIdForRoleAsync(
            "MEMBER", "unified_mallory", "unified_mallory@test.com", "Password@123");

        var conversationId = await CreateConversationAsync(aliceToken, bobId, "Hello Bob");

        // A participant can read the history.
        UseToken(bobToken);
        var allowed = await _client.GetAsync($"/api/chat/conversations/{conversationId}/messages");
        Assert.Equal(HttpStatusCode.OK, allowed.StatusCode);

        // A third party must not be able to read it, even with a valid token.
        UseToken(malloryToken);
        var denied = await _client.GetAsync($"/api/chat/conversations/{conversationId}/messages");
        Assert.True(
            denied.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.NotFound,
            $"Expected the non-participant to be rejected but got {(int)denied.StatusCode}.");

        Assert.NotEqual(aliceId, bobId);
    }

    [Fact]
    public async Task MarkConversationRead_NonParticipant_IsRejected()
    {
        var (aliceToken, _) = await GetTokenAndUserIdForRoleAsync(
            "MEMBER", "read_alice", "read_alice@test.com", "Password@123");
        var (bobToken, bobId) = await GetTokenAndUserIdForRoleAsync(
            "MEMBER", "read_bob", "read_bob@test.com", "Password@123");
        var (malloryToken, _) = await GetTokenAndUserIdForRoleAsync(
            "MEMBER", "read_mallory", "read_mallory@test.com", "Password@123");

        var conversationId = await CreateConversationAsync(aliceToken, bobId, "Read receipt probe");

        UseToken(malloryToken);
        var denied = await _client.PostAsync($"/api/chat/conversations/{conversationId}/read", null);
        Assert.True(
            denied.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.NotFound,
            $"Expected the non-participant to be rejected but got {(int)denied.StatusCode}.");
    }

    [Fact]
    public async Task MarkConversationRead_Recipient_MarksInboundMessageAsRead()
    {
        var (aliceToken, _) = await GetTokenAndUserIdForRoleAsync(
            "MEMBER", "receipt_alice", "receipt_alice@test.com", "Password@123");
        var (bobToken, bobId) = await GetTokenAndUserIdForRoleAsync(
            "MEMBER", "receipt_bob", "receipt_bob@test.com", "Password@123");

        var conversationId = await CreateConversationAsync(aliceToken, bobId, "Did you see this?");

        // Before the recipient marks it read, the message is unread for Bob.
        UseToken(bobToken);
        var before = await _client.GetAsync($"/api/chat/conversations/{conversationId}/messages");
        var beforeResult = await before.Content.ReadFromJsonAsync<Result<IReadOnlyList<ChatMessageModel>>>();
        Assert.NotNull(beforeResult?.Data);
        Assert.All(beforeResult.Data, m => Assert.False(m.IsRead));

        var markResp = await _client.PostAsync($"/api/chat/conversations/{conversationId}/read", null);
        Assert.Equal(HttpStatusCode.OK, markResp.StatusCode);

        var after = await _client.GetAsync($"/api/chat/conversations/{conversationId}/messages");
        var afterResult = await after.Content.ReadFromJsonAsync<Result<IReadOnlyList<ChatMessageModel>>>();
        Assert.NotNull(afterResult?.Data);
        Assert.All(afterResult.Data, m => Assert.True(m.IsRead));
    }

    [Fact]
    public async Task MarkConversationRead_AlreadyRead_ReturnsZeroUnreadCount()
    {
        var (aliceToken, _) = await GetTokenAndUserIdForRoleAsync(
            "MEMBER", "twice_alice", "twice_alice@test.com", "Password@123");
        var (bobToken, bobId) = await GetTokenAndUserIdForRoleAsync(
            "MEMBER", "twice_bob", "twice_bob@test.com", "Password@123");

        var conversationId = await CreateConversationAsync(aliceToken, bobId, "Only once please");

        UseToken(bobToken);
        await _client.PostAsync($"/api/chat/conversations/{conversationId}/read", null);

        // The repeat call must not report new state, so the peer is not notified twice.
        var second = await _client.PostAsync($"/api/chat/conversations/{conversationId}/read", null);
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);
        var secondResult = await second.Content.ReadFromJsonAsync<Result<int>>();
        Assert.NotNull(secondResult);
        Assert.True(secondResult.IsSuccess);
        Assert.Equal(0, secondResult.Data);
    }

    [Fact]
    public async Task MarkConversationRead_ConcurrentRequests_AllSucceed()
    {
        var (aliceToken, _) = await GetTokenAndUserIdForRoleAsync(
            "MEMBER", "race_alice", "race_alice@test.com", "Password@123");
        var (bobToken, bobId) = await GetTokenAndUserIdForRoleAsync(
            "MEMBER", "race_bob", "race_bob@test.com", "Password@123");

        var conversationId = await CreateConversationAsync(aliceToken, bobId, "Race me");

        // The REST endpoint and the hub both mark the thread read, and a user can have the
        // thread open in two tabs. TblChatMessage carries a rowversion concurrency token, so
        // racing writers used to surface DbUpdateConcurrencyException as a 500. Every request
        // must now answer 200 regardless of who wins.
        //
        // Note: the in-memory provider used by this suite does not enforce rowversion, so it
        // cannot reproduce the original conflict and cannot assert that exactly one caller
        // reports new state. That recovery path needs a relational provider to verify.
        UseToken(bobToken);
        var path = $"/api/chat/conversations/{conversationId}/read";

        var responses = await Task.WhenAll(
            Enumerable.Range(0, 4).Select(_ => _client.PostAsync(path, null)));

        Assert.All(responses, r => Assert.Equal(HttpStatusCode.OK, r.StatusCode));

        foreach (var response in responses)
        {
            var result = await response.Content.ReadFromJsonAsync<Result<int>>();
            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.True(result.Data >= 0);
        }

        // The message ends up read regardless of who won the race.
        var messages = await _client.GetAsync($"/api/chat/conversations/{conversationId}/messages");
        var messagesResult = await messages.Content.ReadFromJsonAsync<Result<IReadOnlyList<ChatMessageModel>>>();
        Assert.NotNull(messagesResult?.Data);
        Assert.All(messagesResult.Data, m => Assert.True(m.IsRead));

        // Once the receipt is settled, later calls report no new state, so the peer is not
        // notified again.
        var settled = await _client.PostAsync(path, null);
        var settledResult = await settled.Content.ReadFromJsonAsync<Result<int>>();
        Assert.NotNull(settledResult);
        Assert.Equal(0, settledResult.Data);
    }

    [Fact]
    public async Task GetPreviews_ReturnsOnlyJoinedGroups_WithLastMessage()
    {
        var (creatorToken, _) = await GetTokenAndUserIdForRoleAsync(
            "DOMAIN_PROFESSIONAL", "preview_creator", "preview_creator@test.com", "Password@123");
        var (memberToken, _) = await GetTokenAndUserIdForRoleAsync(
            "MEMBER", "preview_member", "preview_member@test.com", "Password@123");
        var (outsiderToken, _) = await GetTokenAndUserIdForRoleAsync(
            "MEMBER", "preview_outsider", "preview_outsider@test.com", "Password@123");

        UseToken(creatorToken);
        var createJoined = await _client.PostAsJsonAsync(
            "/api/chat-groups",
            new CreateChatGroupRequestModel("Preview Joined", "visible in previews", null, "FREE", 0));
        var joinedResult = await createJoined.Content.ReadFromJsonAsync<Result<ChatGroupModel>>();
        var joinedGroupId = joinedResult!.Data!.ChatGroupId;

        var createHidden = await _client.PostAsJsonAsync(
            "/api/chat-groups",
            new CreateChatGroupRequestModel("Preview Hidden", "must not leak", null, "FREE", 0));
        var hiddenResult = await createHidden.Content.ReadFromJsonAsync<Result<ChatGroupModel>>();
        var hiddenGroupId = hiddenResult!.Data!.ChatGroupId;

        // Only the first group is joined by the member; the second stays out of scope.
        UseToken(memberToken);
        await _client.PostAsJsonAsync($"/api/chat-groups/{joinedGroupId}/join", "");

        var sendResp = await _client.PostAsJsonAsync(
            $"/api/chat-groups/{joinedGroupId}/messages",
            new SendChatGroupMessageRequestModel("Latest group activity"));
        Assert.Equal(HttpStatusCode.OK, sendResp.StatusCode);

        UseToken(memberToken);
        var previewResp = await _client.GetAsync("/api/chat-groups/preview");
        Assert.Equal(HttpStatusCode.OK, previewResp.StatusCode);
        var previewResult = await previewResp.Content.ReadFromJsonAsync<Result<IReadOnlyList<ChatGroupPreviewModel>>>();
        Assert.NotNull(previewResult?.Data);

        var joinedPreview = Assert.Single(previewResult.Data, p => p.ChatGroupId == joinedGroupId);
        Assert.Equal("Latest group activity", joinedPreview.LastMessagePreview);
        Assert.Equal("preview_member", joinedPreview.LastSenderName);
        Assert.NotNull(joinedPreview.LastMessageAt);

        Assert.DoesNotContain(previewResult.Data, p => p.ChatGroupId == hiddenGroupId);

        // A non-member must not receive either group in the preview feed.
        UseToken(outsiderToken);
        var outsiderResp = await _client.GetAsync("/api/chat-groups/preview");
        var outsiderResult = await outsiderResp.Content.ReadFromJsonAsync<Result<IReadOnlyList<ChatGroupPreviewModel>>>();
        Assert.NotNull(outsiderResult?.Data);
        Assert.Empty(outsiderResult.Data);
    }

    [Fact]
    public async Task GetPreviews_TruncatesLongPreviewText()
    {
        var (creatorToken, _) = await GetTokenAndUserIdForRoleAsync(
            "DOMAIN_PROFESSIONAL", "trunc_creator", "trunc_creator@test.com", "Password@123");
        var (memberToken, _) = await GetTokenAndUserIdForRoleAsync(
            "MEMBER", "trunc_member", "trunc_member@test.com", "Password@123");

        UseToken(creatorToken);
        var create = await _client.PostAsJsonAsync(
            "/api/chat-groups",
            new CreateChatGroupRequestModel("Truncation Group", "long preview", null, "FREE", 0));
        var createResult = await create.Content.ReadFromJsonAsync<Result<ChatGroupModel>>();
        var groupId = createResult!.Data!.ChatGroupId;

        UseToken(memberToken);
        await _client.PostAsJsonAsync($"/api/chat-groups/{groupId}/join", "");

        var longText = new string('x', 400);
        var send = await _client.PostAsJsonAsync(
            $"/api/chat-groups/{groupId}/messages",
            new SendChatGroupMessageRequestModel(longText));
        Assert.Equal(HttpStatusCode.OK, send.StatusCode);

        var previewResp = await _client.GetAsync("/api/chat-groups/preview");
        var previewResult = await previewResp.Content.ReadFromJsonAsync<Result<IReadOnlyList<ChatGroupPreviewModel>>>();
        var preview = Assert.Single(previewResult!.Data!, p => p.ChatGroupId == groupId);

        Assert.NotNull(preview.LastMessagePreview);
        Assert.True(
            preview.LastMessagePreview.Length < longText.Length,
            "The preview must be truncated rather than returning the full message body.");
    }

    [Fact]
    public async Task GetPrivateChatStatus_NonCreator_ReportsFreeChat()
    {
        var (aliceToken, _) = await GetTokenAndUserIdForRoleAsync(
            "MEMBER", "status_alice", "status_alice@test.com", "Password@123");
        var (_, bobId) = await GetTokenAndUserIdForRoleAsync(
            "MEMBER", "status_bob", "status_bob@test.com", "Password@123");

        UseToken(aliceToken);
        var resp = await _client.GetAsync($"/api/chat/status/{bobId}");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

        var result = await resp.Content.ReadFromJsonAsync<Result<PrivateChatStatusModel>>();
        Assert.NotNull(result?.Data);
        Assert.Equal(bobId, result.Data.CreatorUserId);
        Assert.False(result.Data.IsPaidChat);
        Assert.True(result.Data.IsUnlocked);
        Assert.Equal(0, result.Data.FeeLinkDrops);
    }
}
