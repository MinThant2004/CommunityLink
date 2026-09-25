namespace CommunityLink.Shared.Features.ChatGroup;

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
    string Content
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
    bool IsMine = false
);
