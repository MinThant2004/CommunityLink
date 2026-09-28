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
    DateTime SentAt,
    // Quoted message. ReplyToIsDeleted is true when the original was deleted for everyone,
    // so the client can render the tombstone strip instead of vanished text.
    int? ReplyToMessageId = null,
    string? ReplyToSenderName = null,
    string? ReplyToPreview = null,
    bool ReplyToIsDeleted = false,
    IReadOnlyList<MessageReactionModel>? Reactions = null,
    // Server-computed so the client never offers an action the service will refuse.
    bool CanDeleteForEveryone = false);

public sealed record SendMessageRequestModel(
    int? ConversationId,
    int? TargetUserId,
    string MessageText,
    int? ReplyToMessageId = null);

/// <summary>Body of a reaction toggle. The same emoji twice removes the reaction.</summary>
public sealed record SetMessageReactionRequestModel(string? Emoji);

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