using System.Net.Http.Headers;
using System.Net.Http.Json;
using CommunityLink.Api.Tests.Infrastructure;
using CommunityLink.Database.AppDbContextModels;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Authentication;
using CommunityLink.Shared.Features.ChatGroup;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CommunityLink.Api.Tests.Features;

public class ChatGroupSignalRTests : IClassFixture<CommunityApiFactory>
{
    private readonly CommunityApiFactory _factory;
    private readonly HttpClient _client;

    public ChatGroupSignalRTests(CommunityApiFactory factory)
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

    private async Task<int> CreateChatGroupAsync(string creatorToken, string name)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", creatorToken);

        var createResp = await client.PostAsJsonAsync("/api/chat-groups", new CreateChatGroupRequestModel(name, "Description", null, "FREE", 0));
        var createResult = await createResp.Content.ReadFromJsonAsync<Result<ChatGroupModel>>();
        Assert.NotNull(createResult?.Data);
        return createResult.Data.ChatGroupId;
    }

    private HubConnection CreateHubConnection(string token)
    {
        var hubUrl = new Uri(_factory.Server.BaseAddress, "hubs/chat-groups?access_token=" + token);
        return new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
            })
            .Build();
    }

    [Fact]
    public async Task SignalR_ActiveMember_ReceivesRealtimeBroadcastWhenMessageIsSent()
    {
        var (creatorToken, _) = await GetTokenAndUserIdForRoleAsync("DOMAIN_PROFESSIONAL", "creator_sr1", "creator_sr1@test.com", "Password@123");
        var (memberToken, _) = await GetTokenAndUserIdForRoleAsync("MEMBER", "user_sr1", "user_sr1@test.com", "Password@123");

        int chatGroupId = await CreateChatGroupAsync(creatorToken, "SignalR Live Group 1");

        // Member joins group via REST API
        var memberClient = _factory.CreateClient();
        memberClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", memberToken);
        var joinRes = await memberClient.PostAsJsonAsync($"/api/chat-groups/{chatGroupId}/join", "");
        joinRes.EnsureSuccessStatusCode();

        // Connect member to SignalR hub
        var hubConnection = CreateHubConnection(memberToken);
        var tcs = new TaskCompletionSource<ChatGroupMessageModel>();

        hubConnection.On<ChatGroupMessageModel>("ReceiveChatGroupMessage", msg =>
        {
            tcs.TrySetResult(msg);
        });

        await hubConnection.StartAsync();
        await hubConnection.InvokeAsync("JoinChatGroup", chatGroupId);

        // Creator posts message via REST API
        var creatorClient = _factory.CreateClient();
        creatorClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", creatorToken);
        var sendRes = await creatorClient.PostAsJsonAsync($"/api/chat-groups/{chatGroupId}/messages", new SendChatGroupMessageRequestModel("Hello via SignalR real-time broadcast!"));
        sendRes.EnsureSuccessStatusCode();

        // Wait for real-time broadcast signal with timeout
        var completedTask = await Task.WhenAny(tcs.Task, Task.Delay(5000));
        Assert.Same(tcs.Task, completedTask);

        var receivedMessage = await tcs.Task;
        Assert.NotNull(receivedMessage);
        Assert.Equal("Hello via SignalR real-time broadcast!", receivedMessage.Content);
        Assert.Equal(chatGroupId, receivedMessage.ChatGroupId);

        await hubConnection.StopAsync();
        await hubConnection.DisposeAsync();
    }

    [Fact]
    public async Task SignalR_NonMember_CannotJoinHubGroup()
    {
        var (creatorToken, _) = await GetTokenAndUserIdForRoleAsync("DOMAIN_PROFESSIONAL", "creator_sr2", "creator_sr2@test.com", "Password@123");
        var (nonMemberToken, _) = await GetTokenAndUserIdForRoleAsync("MEMBER", "user_sr2", "user_sr2@test.com", "Password@123");

        int chatGroupId = await CreateChatGroupAsync(creatorToken, "SignalR Isolated Group 2");

        var hubConnection = CreateHubConnection(nonMemberToken);
        await hubConnection.StartAsync();

        // Non-member trying to join the SignalR room should fail with exception
        await Assert.ThrowsAsync<HubException>(async () =>
        {
            await hubConnection.InvokeAsync("JoinChatGroup", chatGroupId);
        });

        await hubConnection.StopAsync();
        await hubConnection.DisposeAsync();
    }
}
