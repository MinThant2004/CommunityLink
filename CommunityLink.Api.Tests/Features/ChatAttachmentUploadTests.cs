using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CommunityLink.Api.Tests.Infrastructure;
using CommunityLink.Database.AppDbContextModels;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Authentication;
using CommunityLink.Shared.Features.Chat;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CommunityLink.Api.Tests.Features;

/// <summary>
/// Covers the multipart attachment endpoint: the allow-list, the size cap, and — most
/// importantly — that a message row ends up holding a short URL rather than an inline
/// base64 blob, which is what previously made video uploads fail.
/// </summary>
public class ChatAttachmentUploadTests : IClassFixture<CommunityApiFactory>
{
    private readonly CommunityApiFactory _factory;
    private readonly HttpClient _client;

    public ChatAttachmentUploadTests(CommunityApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<(string Token, int UserId)> GetMemberTokenAsync(string prefix)
    {
        var unique = Guid.NewGuid().ToString("N")[..8];
        var userName = $"{prefix}_{unique}";
        var email = $"{userName}@test.local";
        const string password = "Password@123";

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var role = db.TblRoles.FirstOrDefault(r => r.RoleCode == "MEMBER");
            if (role == null)
            {
                role = new TblRole
                {
                    RoleCode = "MEMBER",
                    RoleName = "MEMBER",
                    Description = "Member",
                    IsSystemRole = false,
                    CreatedAt = DateTime.UtcNow
                };
                db.TblRoles.Add(role);
                await db.SaveChangesAsync();
            }

            var user = new TblUser
            {
                UserName = userName,
                NormalizedUserName = userName.ToUpperInvariant(),
                DisplayName = userName,
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

            var login = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequestModel(email, password));
            var loginResult = await login.Content.ReadFromJsonAsync<Result<LoginResponseModel>>();
            Assert.NotNull(loginResult?.Data?.AccessToken);

            return (loginResult.Data.AccessToken, user.UserId);
        }
    }

    private void UseToken(string token) =>
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

    private static MultipartFormDataContent BuildUpload(string fileName, string contentType, int byteCount)
    {
        // Filler that is not all zero bytes, so the payload is not mistaken for empty.
        var bytes = new byte[byteCount];
        for (var i = 0; i < bytes.Length; i++) bytes[i] = (byte)(i % 251 + 1);

        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(bytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        content.Add(fileContent, "file", fileName);
        return content;
    }

    [Fact]
    public async Task UploadAttachment_WithoutToken_ReturnsUnauthorized()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        using var content = BuildUpload("clip.mp4", "video/mp4", 2048);

        var response = await _client.PostAsync("/api/chat/attachments", content);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("clip.mp4", "video/mp4", 512 * 1024, "VIDEO")]
    [InlineData("clip.webm", "video/webm", 512 * 1024, "VIDEO")]
    [InlineData("clip.mov", "video/quicktime", 512 * 1024, "VIDEO")]
    [InlineData("photo.png", "image/png", 512 * 1024, "IMAGE")]
    [InlineData("photo.jpeg", "image/jpeg", 512 * 1024, "IMAGE")]
    [InlineData("notes.txt", "text/plain", 4 * 1024, "FILE")]
    [InlineData("doc.pdf", "application/pdf", 4 * 1024, "FILE")]
    public async Task UploadAttachment_AllowedTypes_ReturnsAbsoluteUrlAndClassifiedType(
        string fileName, string contentType, int size, string expectedMessageType)
    {
        var (token, _) = await GetMemberTokenAsync("attach");
        UseToken(token);

        using var content = BuildUpload(fileName, contentType, size);
        var response = await _client.PostAsync("/api/chat/attachments", content);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<Result<ChatAttachmentUploadResponse>>();
        Assert.NotNull(result);
        Assert.True(result.IsSuccess, result.Message);
        Assert.NotNull(result.Data);

        // A relative "/uploads/..." would be resolved by the browser against the Blazor App
        // origin, which serves a different wwwroot and 404s.
        Assert.StartsWith("https://", result.Data.Url);
        Assert.Contains("/uploads/chat/", result.Data.Url);
        Assert.DoesNotContain("data:", result.Data.Url);
        Assert.Equal(fileName, result.Data.FileName);
        Assert.Equal(size, result.Data.FileSizeByte);
        Assert.Equal(expectedMessageType, result.Data.MessageType);
    }

    [Fact]
    public async Task UploadAttachment_DisallowedExtension_ReturnsBadRequestWithMessage()
    {
        var (token, _) = await GetMemberTokenAsync("attach");
        UseToken(token);

        using var content = BuildUpload("payload.exe", "application/octet-stream", 1024);
        var response = await _client.PostAsync("/api/chat/attachments", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<Result<ChatAttachmentUploadResponse>>();
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.Contains("Unsupported file type", result.Message);
    }

    [Fact]
    public async Task UploadAttachment_OverSizeLimit_ReturnsBadRequest()
    {
        var (token, _) = await GetMemberTokenAsync("attach");
        UseToken(token);

        // Exactly one byte over the cap.
        using var content = BuildUpload("big.mp4", "video/mp4", (int)ChatAttachmentPolicy.MaxBytes + 1);
        var response = await _client.PostAsync("/api/chat/attachments", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<Result<ChatAttachmentUploadResponse>>();
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.Contains("too large", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UploadAttachment_MissingFile_ReturnsBadRequest()
    {
        var (token, _) = await GetMemberTokenAsync("attach");
        UseToken(token);

        using var content = new MultipartFormDataContent();
        var response = await _client.PostAsync("/api/chat/attachments", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UploadedAttachment_ThenPostedAsMessage_IsStoredByShortUrlNotInlineData()
    {
        var (token, _) = await GetMemberTokenAsync("attachsend");
        var (_, targetUserId) = await GetMemberTokenAsync("attachtarget");
        UseToken(token);

        using var uploadContent = BuildUpload("clip.mp4", "video/mp4", 256 * 1024);
        var uploadResponse = await _client.PostAsync("/api/chat/attachments", uploadContent);
        Assert.Equal(HttpStatusCode.OK, uploadResponse.StatusCode);

        var upload = await uploadResponse.Content.ReadFromJsonAsync<Result<ChatAttachmentUploadResponse>>();
        Assert.NotNull(upload?.Data);

        var sendRequest = new SendMessageRequestModel(
            ConversationId: null,
            TargetUserId: targetUserId,
            MessageText: "here is the clip",
            MessageType: upload.Data.MessageType,
            AttachmentUrl: upload.Data.Url,
            FileName: upload.Data.FileName,
            FileSizeByte: upload.Data.FileSizeByte);

        var sendResponse = await _client.PostAsJsonAsync("/api/chat/messages", sendRequest);
        Assert.Equal(HttpStatusCode.OK, sendResponse.StatusCode);

        var sent = await sendResponse.Content.ReadFromJsonAsync<Result<ChatMessageModel>>();
        Assert.NotNull(sent?.Data);
        Assert.Equal("VIDEO", sent.Data.MessageType);
        Assert.Equal("clip.mp4", sent.Data.FileName);
        Assert.Equal(256 * 1024, sent.Data.FileSizeByte);

        // The whole point: the message row carries a short URL, not a base64 blob.
        Assert.Equal(upload.Data.Url, sent.Data.AttachmentUrl);
        Assert.True(
            sent.Data.AttachmentUrl!.Length < 200,
            $"AttachmentUrl should be a short path but was {sent.Data.AttachmentUrl!.Length} characters.");
    }

    [Fact]
    public async Task SendMessage_WithNoAttachment_StaysTextType()
    {
        var (token, _) = await GetMemberTokenAsync("plaintext");
        var (_, targetUserId) = await GetMemberTokenAsync("texttarget");
        UseToken(token);

        var request = new SendMessageRequestModel(null, targetUserId, "just text", "TEXT");
        var response = await _client.PostAsJsonAsync("/api/chat/messages", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<Result<ChatMessageModel>>();
        Assert.NotNull(result?.Data);
        Assert.Equal("TEXT", result.Data.MessageType);
        Assert.Null(result.Data.AttachmentUrl);
    }
}
