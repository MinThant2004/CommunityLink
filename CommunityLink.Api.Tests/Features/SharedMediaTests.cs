namespace CommunityLink.Api.Tests.Features;

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
using CommunityLink.Shared.Features.ChatGroup;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

public class SharedMediaTests : IClassFixture<CommunityApiFactory>
{
    private readonly CommunityApiFactory _factory;
    private readonly HttpClient _client;

    public SharedMediaTests(CommunityApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<(string Token, int UserId)> CreateTestUserAsync(string prefix)
    {
        var username = $"{prefix}_{Guid.NewGuid():N}";
        var email = $"{username}@test.com";

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var role = await db.TblRoles.FirstOrDefaultAsync(r => r.RoleCode == "MEMBER");
        if (role == null)
        {
            role = new TblRole
            {
                RoleCode = "MEMBER",
                RoleName = "Member",
                Description = "Member role",
                IsSystemRole = false,
                CreatedAt = DateTime.UtcNow
            };
            db.TblRoles.Add(role);
            await db.SaveChangesAsync();
        }

        var user = new TblUser
        {
            UserName = username,
            NormalizedUserName = username.ToUpperInvariant(),
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            DisplayName = username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
            IsActive = true,
            IsVerified = true,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };
        db.TblUsers.Add(user);
        await db.SaveChangesAsync();

        db.TblUserRoles.Add(new TblUserRole
        {
            UserId = user.UserId,
            RoleId = role.RoleId,
            CreatedAt = DateTime.UtcNow
        });

        var accessPerm = await db.TblPermissions.FirstOrDefaultAsync(p => p.PermissionCode == "CHAT_ACCESS");
        if (accessPerm != null)
        {
            db.TblRolePermissions.Add(new TblRolePermission { RoleId = role.RoleId, PermissionId = accessPerm.PermissionId });
        }
        var sendPerm = await db.TblPermissions.FirstOrDefaultAsync(p => p.PermissionCode == "CHAT_SEND");
        if (sendPerm != null)
        {
            db.TblRolePermissions.Add(new TblRolePermission { RoleId = role.RoleId, PermissionId = sendPerm.PermissionId });
        }
        await db.SaveChangesAsync();

        var loginRes = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequestModel(email, "Password123!"));
        var result = await loginRes.Content.ReadFromJsonAsync<Result<LoginResponseModel>>();
        return (result!.Data!.AccessToken, user.UserId);
    }

    private void AuthenticateClient(string token)
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    private async Task<(int ConversationId, string User1Token, int User1Id, string User2Token, int User2Id)> CreateConversationWithMediaAsync(string prefix)
    {
        var (token1, user1Id) = await CreateTestUserAsync($"{prefix}_u1");
        var (token2, user2Id) = await CreateTestUserAsync($"{prefix}_u2");

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var conv = new TblConversation
        {
            UserOneId = user1Id,
            UserTwoId = user2Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.TblConversations.Add(conv);
        await db.SaveChangesAsync();

        var photoMsg = new TblChatMessage
        {
            ConversationId = conv.ConversationId,
            SenderId = user1Id,
            MessageType = "IMAGE",
            MessageText = "Photo 1",
            AttachmentUrl = "/uploads/chat/photo1.jpg",
            FileName = "photo1.jpg",
            FileSizeByte = 1024,
            CreatedAt = DateTime.UtcNow.AddMinutes(-10),
            IsDeleted = false
        };

        var videoMsg = new TblChatMessage
        {
            ConversationId = conv.ConversationId,
            SenderId = user2Id,
            MessageType = "VIDEO",
            MessageText = "Video 1",
            AttachmentUrl = "/uploads/chat/video1.mp4",
            FileName = "video1.mp4",
            FileSizeByte = 1048576,
            CreatedAt = DateTime.UtcNow.AddMinutes(-5),
            IsDeleted = false
        };

        var fileMsg = new TblChatMessage
        {
            ConversationId = conv.ConversationId,
            SenderId = user1Id,
            MessageType = "FILE",
            MessageText = "File 1",
            AttachmentUrl = "/uploads/chat/file1.pdf",
            FileName = "file1.pdf",
            FileSizeByte = 2048,
            CreatedAt = DateTime.UtcNow.AddMinutes(-1),
            IsDeleted = false
        };

        db.TblChatMessages.AddRange(photoMsg, videoMsg, fileMsg);
        await db.SaveChangesAsync();

        return (conv.ConversationId, token1, user1Id, token2, user2Id);
    }

    private async Task<(int ChatGroupId, string OwnerToken, int OwnerId, string MemberToken, int MemberId)> CreateGroupWithMediaAsync(string prefix)
    {
        var (token1, ownerId) = await CreateTestUserAsync($"{prefix}_owner");
        var (token2, memberId) = await CreateTestUserAsync($"{prefix}_member");

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var group = new TblChatGroup
        {
            Name = $"{prefix}_Group_{Guid.NewGuid():N}",
            Description = "Test group",
            ChatType = "FREE",
            AccessMode = "PRIVATE",
            CreatorId = ownerId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };
        db.TblChatGroups.Add(group);
        await db.SaveChangesAsync();

        var ownerMember = new TblChatGroupMember
        {
            ChatGroupId = group.ChatGroupId,
            UserId = ownerId,
            Role = "OWNER",
            JoinedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        var activeMember = new TblChatGroupMember
        {
            ChatGroupId = group.ChatGroupId,
            UserId = memberId,
            Role = "MEMBER",
            JoinedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        db.TblChatGroupMembers.AddRange(ownerMember, activeMember);
        await db.SaveChangesAsync();

        var photoMsg = new TblChatGroupMessage
        {
            ChatGroupId = group.ChatGroupId,
            SenderId = ownerId,
            MessageType = "IMAGE",
            Content = "Group Photo 1",
            AttachmentUrl = "/uploads/chat-groups/gphoto1.jpg",
            FileName = "gphoto1.jpg",
            FileSizeByte = 1024,
            CreatedAt = DateTime.UtcNow.AddMinutes(-10),
            IsDeleted = false
        };

        var videoMsg = new TblChatGroupMessage
        {
            ChatGroupId = group.ChatGroupId,
            SenderId = memberId,
            MessageType = "VIDEO",
            Content = "Group Video 1",
            AttachmentUrl = "/uploads/chat-groups/gvideo1.mp4",
            FileName = "gvideo1.mp4",
            FileSizeByte = 1048576,
            CreatedAt = DateTime.UtcNow.AddMinutes(-5),
            IsDeleted = false
        };

        var fileMsg = new TblChatGroupMessage
        {
            ChatGroupId = group.ChatGroupId,
            SenderId = ownerId,
            MessageType = "FILE",
            Content = "Group File 1",
            AttachmentUrl = "/uploads/chat-groups/gfile1.pdf",
            FileName = "gfile1.pdf",
            FileSizeByte = 2048,
            CreatedAt = DateTime.UtcNow.AddMinutes(-1),
            IsDeleted = false
        };

        db.TblChatGroupMessages.AddRange(photoMsg, videoMsg, fileMsg);
        await db.SaveChangesAsync();

        return (group.ChatGroupId, token1, ownerId, token2, memberId);
    }

    [Fact]
    public async Task PrivateChatPhotos_ReturnsOnlyConversationPhotos()
    {
        var (convId, token1, _, _, _) = await CreateConversationWithMediaAsync("pc_photos");
        AuthenticateClient(token1);

        var res = await _client.GetAsync($"/api/chat/conversations/{convId}/media/photos");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        var result = await res.Content.ReadFromJsonAsync<Result<SharedMediaPagedResultModel>>();
        Assert.True(result!.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data.Items);
        Assert.Equal("IMAGE", result.Data.Items[0].MessageType);
    }

    [Fact]
    public async Task PrivateChatVideos_ReturnsOnlyConversationVideos()
    {
        var (convId, token1, _, _, _) = await CreateConversationWithMediaAsync("pc_videos");
        AuthenticateClient(token1);

        var res = await _client.GetAsync($"/api/chat/conversations/{convId}/media/videos");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        var result = await res.Content.ReadFromJsonAsync<Result<SharedMediaPagedResultModel>>();
        Assert.True(result!.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data.Items);
        Assert.Equal("VIDEO", result.Data.Items[0].MessageType);
    }

    [Fact]
    public async Task PrivateChatFiles_ReturnsOnlyConversationFiles()
    {
        var (convId, token1, _, _, _) = await CreateConversationWithMediaAsync("pc_files");
        AuthenticateClient(token1);

        var res = await _client.GetAsync($"/api/chat/conversations/{convId}/media/files");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        var result = await res.Content.ReadFromJsonAsync<Result<SharedMediaPagedResultModel>>();
        Assert.True(result!.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data.Items);
        Assert.Equal("FILE", result.Data.Items[0].MessageType);
    }

    [Fact]
    public async Task ChatGroupPhotos_ReturnsOnlyGroupPhotos()
    {
        var (groupId, ownerToken, _, _, _) = await CreateGroupWithMediaAsync("cg_photos");
        AuthenticateClient(ownerToken);

        var res = await _client.GetAsync($"/api/chat-groups/{groupId}/media/photos");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        var result = await res.Content.ReadFromJsonAsync<Result<SharedMediaPagedResultModel>>();
        Assert.True(result!.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data.Items);
        Assert.Equal("IMAGE", result.Data.Items[0].MessageType);
    }

    [Fact]
    public async Task ChatGroupVideos_ReturnsOnlyGroupVideos()
    {
        var (groupId, ownerToken, _, _, _) = await CreateGroupWithMediaAsync("cg_videos");
        AuthenticateClient(ownerToken);

        var res = await _client.GetAsync($"/api/chat-groups/{groupId}/media/videos");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        var result = await res.Content.ReadFromJsonAsync<Result<SharedMediaPagedResultModel>>();
        Assert.True(result!.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data.Items);
        Assert.Equal("VIDEO", result.Data.Items[0].MessageType);
    }

    [Fact]
    public async Task ChatGroupFiles_ReturnsOnlyGroupFiles()
    {
        var (groupId, ownerToken, _, _, _) = await CreateGroupWithMediaAsync("cg_files");
        AuthenticateClient(ownerToken);

        var res = await _client.GetAsync($"/api/chat-groups/{groupId}/media/files");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        var result = await res.Content.ReadFromJsonAsync<Result<SharedMediaPagedResultModel>>();
        Assert.True(result!.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data.Items);
        Assert.Equal("FILE", result.Data.Items[0].MessageType);
    }

    [Fact]
    public async Task UnauthorizedUser_CannotAccessPrivateChatMedia()
    {
        var (convId, _, _, _, _) = await CreateConversationWithMediaAsync("unauth_pc");
        var (thirdToken, _) = await CreateTestUserAsync("third_user");
        AuthenticateClient(thirdToken);

        var res = await _client.GetAsync($"/api/chat/conversations/{convId}/media/photos");
        Assert.Equal(HttpStatusCode.Forbidden, res.StatusCode);
    }

    [Fact]
    public async Task NonMember_CannotAccessPrivateGroupMedia()
    {
        var (groupId, _, _, _, _) = await CreateGroupWithMediaAsync("unauth_cg");
        var (thirdToken, _) = await CreateTestUserAsync("third_user_group");
        AuthenticateClient(thirdToken);

        var res = await _client.GetAsync($"/api/chat-groups/{groupId}/media/photos");
        Assert.Equal(HttpStatusCode.Forbidden, res.StatusCode);
    }

    [Fact]
    public async Task DeletedMessageAttachment_IsExcluded()
    {
        var (convId, token1, user1Id, _, user2Id) = await CreateConversationWithMediaAsync("del_media");
        AuthenticateClient(token1);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var photos = await db.TblChatMessages
                .Where(m => m.ConversationId == convId && m.MessageType == "IMAGE")
                .ToListAsync();
            foreach (var p in photos)
            {
                p.IsDeleted = true;
            }
            await db.SaveChangesAsync();
        }

        var res = await _client.GetAsync($"/api/chat/conversations/{convId}/media/photos");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        var result = await res.Content.ReadFromJsonAsync<Result<SharedMediaPagedResultModel>>();
        Assert.True(result!.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data.Items);
    }

    [Fact]
    public async Task PaginationWorksForSharedMedia()
    {
        var (token1, user1Id) = await CreateTestUserAsync("page_u1");
        var (token2, user2Id) = await CreateTestUserAsync("page_u2");

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var conv = new TblConversation
        {
            UserOneId = user1Id,
            UserTwoId = user2Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.TblConversations.Add(conv);
        await db.SaveChangesAsync();

        for (int i = 1; i <= 35; i++)
        {
            db.TblChatMessages.Add(new TblChatMessage
            {
                ConversationId = conv.ConversationId,
                SenderId = user1Id,
                MessageType = "IMAGE",
                MessageText = $"Photo {i}",
                AttachmentUrl = $"/uploads/chat/photo_{i}.jpg",
                FileName = $"photo_{i}.jpg",
                FileSizeByte = 1024,
                CreatedAt = DateTime.UtcNow.AddMinutes(-i),
                IsDeleted = false
            });
        }
        await db.SaveChangesAsync();

        AuthenticateClient(token1);

        var resPage1 = await _client.GetAsync($"/api/chat/conversations/{conv.ConversationId}/media/photos?page=1&pageSize=20");
        Assert.Equal(HttpStatusCode.OK, resPage1.StatusCode);
        var resultPage1 = await resPage1.Content.ReadFromJsonAsync<Result<SharedMediaPagedResultModel>>();
        Assert.True(resultPage1!.IsSuccess);
        Assert.Equal(35, resultPage1.Data!.TotalCount);
        Assert.Equal(20, resultPage1.Data.Items.Count);
        Assert.True(resultPage1.Data.HasNextPage);

        var resPage2 = await _client.GetAsync($"/api/chat/conversations/{conv.ConversationId}/media/photos?page=2&pageSize=20");
        Assert.Equal(HttpStatusCode.OK, resPage2.StatusCode);
        var resultPage2 = await resPage2.Content.ReadFromJsonAsync<Result<SharedMediaPagedResultModel>>();
        Assert.True(resultPage2!.IsSuccess);
        Assert.Equal(35, resultPage2.Data!.TotalCount);
        Assert.Equal(15, resultPage2.Data.Items.Count);
        Assert.False(resultPage2.Data.HasNextPage);
    }

    [Fact]
    public async Task ViewInChat_ReturnsOriginalMessageReference()
    {
        var (convId, token1, _, _, _) = await CreateConversationWithMediaAsync("view_chat_ref");
        AuthenticateClient(token1);

        var res = await _client.GetAsync($"/api/chat/conversations/{convId}/media/photos");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        var result = await res.Content.ReadFromJsonAsync<Result<SharedMediaPagedResultModel>>();
        Assert.True(result!.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data.Items);

        var item = result.Data.Items[0];
        Assert.True(item.MessageId > 0);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var dbMsg = await db.TblChatMessages.FirstOrDefaultAsync(m => m.ChatMessageId == item.MessageId);
        Assert.NotNull(dbMsg);
        Assert.Equal(convId, dbMsg.ConversationId);
        Assert.Equal(item.AttachmentUrl, dbMsg.AttachmentUrl);
    }
}
