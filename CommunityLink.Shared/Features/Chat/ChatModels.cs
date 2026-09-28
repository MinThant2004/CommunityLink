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

public sealed record CreatorChatSettingModel(
    int CreatorUserId,
    bool IsPrivateChatEnabled,
    long PrivateChatFeeLinkDrops,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public sealed record SaveCreatorChatSettingRequestModel(
    bool IsPrivateChatEnabled,
    long PrivateChatFeeLinkDrops);

/// <summary>
/// Lock state for a 1:1 thread, so the client can show the unlock banner before the
/// first send is rejected with PRIVATE_CHAT_PAYMENT_REQUIRED.
/// </summary>
public sealed record PrivateChatStatusModel(
    int CreatorUserId,
    bool IsPaidChat,
    bool IsUnlocked,
    long FeeLinkDrops);