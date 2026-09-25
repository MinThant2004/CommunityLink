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

public class ChatGroupFoundationTests : IClassFixture<CommunityApiFactory>
{
    private readonly CommunityApiFactory _factory;
    private readonly HttpClient _client;

    public ChatGroupFoundationTests(CommunityApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<string> GetTokenForRoleAsync(string roleCode, string username, string email, string password)
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
        return loginResult.Data.AccessToken;
    }

    [Fact]
    public async Task CreateChatGroup_AsPremiumCreator_Succeeds()
    {
        var token = await GetTokenForRoleAsync("DOMAIN_PROFESSIONAL", "dp_creator1", "dp_creator1@test.com", "Password@123");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new CreateChatGroupRequestModel("C# Expert Group", "Learn C# best practices", null, "FREE", 0);
        var response = await _client.PostAsJsonAsync("/api/chat-groups", request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<Result<ChatGroupModel>>();
        Assert.NotNull(result?.Data);
        Assert.True(result.IsSuccess);
        Assert.True(result.Data.ChatGroupId > 0);
        Assert.Equal("C# Expert Group", result.Data.Name);
        Assert.Equal("OWNER", result.Data.UserRole);
        Assert.True(result.Data.IsJoined);
    }

    [Fact]
    public async Task CreateChatGroup_AsNormalUser_FailsWithForbiddenMessage()
    {
        var token = await GetTokenForRoleAsync("MEMBER", "normal_user1", "member_user1@test.com", "Password@123");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new CreateChatGroupRequestModel("Member Room", "Should fail", null, "FREE", 0);
        var response = await _client.PostAsJsonAsync("/api/chat-groups", request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<Result<ChatGroupModel>>();
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.Contains("Only Premium Creators can create Chat Groups.", result.Message);
    }

    [Fact]
    public async Task CreateChatGroup_CreatorCanCreateMultipleGroups_AllSucceed()
    {
        var token = await GetTokenForRoleAsync("PUBLIC_FIGURE", "pf_creator1", "pf_creator1@test.com", "Password@123");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // 1st Chat Group: C# Group
        var req1 = new CreateChatGroupRequestModel("Mg Mg C# Group", "C# Discussions", null, "FREE", 0);
        var resp1 = await _client.PostAsJsonAsync("/api/chat-groups", req1);
        Assert.Equal(HttpStatusCode.OK, resp1.StatusCode);
        var res1 = await resp1.Content.ReadFromJsonAsync<Result<ChatGroupModel>>();
        Assert.True(res1?.IsSuccess);

        // 2nd Chat Group: .NET Group
        var req2 = new CreateChatGroupRequestModel("Mg Mg .NET Group", ".NET Architecture", null, "PAID", 50);
        var resp2 = await _client.PostAsJsonAsync("/api/chat-groups", req2);
        Assert.Equal(HttpStatusCode.OK, resp2.StatusCode);
        var res2 = await resp2.Content.ReadFromJsonAsync<Result<ChatGroupModel>>();
        Assert.True(res2?.IsSuccess);

        // 3rd Chat Group: AI Group
        var req3 = new CreateChatGroupRequestModel("Mg Mg AI Group", "AI & ML", null, "FREE", 0);
        var resp3 = await _client.PostAsJsonAsync("/api/chat-groups", req3);
        Assert.Equal(HttpStatusCode.OK, resp3.StatusCode);
        var res3 = await resp3.Content.ReadFromJsonAsync<Result<ChatGroupModel>>();
        Assert.True(res3?.IsSuccess);

        // Fetch My Groups
        var myResp = await _client.GetAsync("/api/chat-groups/my");
        Assert.Equal(HttpStatusCode.OK, myResp.StatusCode);
        var myResult = await myResp.Content.ReadFromJsonAsync<Result<IReadOnlyList<ChatGroupModel>>>();
        Assert.NotNull(myResult?.Data);
        Assert.True(myResult.IsSuccess);
        Assert.True(myResult.Data.Count >= 3);
        Assert.Contains(myResult.Data, g => g.Name == "Mg Mg C# Group");
        Assert.Contains(myResult.Data, g => g.Name == "Mg Mg .NET Group");
        Assert.Contains(myResult.Data, g => g.Name == "Mg Mg AI Group");
    }

    [Fact]
    public void ChatGroup_IsIndependentFromTblGroup()
    {
        var chatGroupProperties = typeof(TblChatGroup).GetProperties().Select(p => p.Name).ToList();
        
        // Chat Group must NOT have GroupId or FK to TblGroup
        Assert.DoesNotContain("GroupId", chatGroupProperties);
        Assert.DoesNotContain("Group", chatGroupProperties);
        Assert.DoesNotContain("SubCommunityId", chatGroupProperties);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Verify DbSets are separate
        Assert.NotNull(db.TblChatGroups);
        Assert.NotNull(db.TblGroups);
        Assert.NotSame(db.TblChatGroups, db.TblGroups);
    }

    [Fact]
    public void ChatGroupMember_DuplicateMembershipConstraint_EnforcesUniqueness()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var entityType = db.Model.FindEntityType(typeof(TblChatGroupMember));
        Assert.NotNull(entityType);

        var index = entityType.GetIndexes()
            .FirstOrDefault(i => i.Properties.Select(p => p.Name).SequenceEqual(new[] { "ChatGroupId", "UserId" }));

        Assert.NotNull(index);
        Assert.True(index.IsUnique, "Unique constraint on (ChatGroupId, UserId) must be defined.");
    }
}
