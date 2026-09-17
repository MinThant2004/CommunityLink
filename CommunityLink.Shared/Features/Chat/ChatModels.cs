namespace CommunityLink.Shared.Features.Chat;

public sealed record ConversationModel(
    int ConversationId,
    int OtherUserId,
    string OtherUserName,
    string OtherDisplayName,
    string? OtherAvatarUrl,
    string? LastMessagePreview,
    DateTime? LastMessageAt);

public sealed record ChatMessageModel(
    int MessageId,
    int ConversationId,
    int SenderId,
    string SenderName,
    string? SenderAvatar,
    string MessageText,
    bool IsRead,
    DateTime SentAt);

public sealed record SendMessageRequestModel(int? ConversationId, int? TargetUserId, string MessageText);