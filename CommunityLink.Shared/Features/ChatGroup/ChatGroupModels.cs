namespace CommunityLink.Shared.Features.ChatGroup;

using CommunityLink.Shared.Features.Chat;

public sealed record CreateChatGroupRequestModel(
    string Name,
    string? Description,
    string? AvatarUrl,
    string ChatType = "FREE", // FREE | PAID
    long JoinFeeLinkDrops = 0
);

public sealed record ChatGroupModel(
    int ChatGroupId,
    string Name,
    string? Description,
    string? AvatarUrl,
    int CreatorId,
    string CreatorName,
    string ChatType,
    long JoinFeeLinkDrops,
    decimal CommissionPercentageSnapshot,
    int MemberCount,
    DateTime CreatedAt,
    bool IsJoined = false,
    string UserRole = "NONE" // OWNER | ADMIN | MEMBER | NONE
);

public sealed record ChatGroupMemberModel(
    int ChatGroupMemberId,
    int ChatGroupId,
    int UserId,
    string UserName,
    string DisplayName,
    string? UserAvatar,
    string Role,
    DateTime JoinedAt
);

public sealed record SendChatGroupMessageRequestModel(
    string Content,
    int? ReplyToChatGroupMessageId = null
);

public sealed record ChatGroupPreviewModel(
    int ChatGroupId,
    string? LastMessagePreview,
    DateTime? LastMessageAt,
    string? LastSenderName
);

public sealed record ChatGroupMessageModel(
    int ChatGroupMessageId,
    int ChatGroupId,
    int SenderId,
    string SenderName,
    string SenderDisplayName,
    string? SenderAvatar,
    string Content,
    DateTime CreatedAt,
    bool IsMine = false,
    // Quoted message; see ChatMessageModel for the meaning of ReplyToIsDeleted.
    int? ReplyToChatGroupMessageId = null,
    string? ReplyToSenderName = null,
    string? ReplyToPreview = null,
    bool ReplyToIsDeleted = false,
    IReadOnlyList<MessageReactionModel>? Reactions = null,
    // Sender, or a group OWNER/ADMIN acting as a moderator.
    bool CanDeleteForEveryone = false
);
