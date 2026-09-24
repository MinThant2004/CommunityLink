namespace CommunityLink.Shared.Features.GroupChat;

public sealed record GroupChatRoomModel(
    int GroupChatRoomId,
    int GroupId,
    string GroupName,
    int CreatorId,
    string CreatorName,
    string ChatType,
    int JoinFeeLinkDrops,
    decimal CommissionPercentageSnapshot,
    bool IsActive,
    DateTime CreatedAt
);

public sealed record CreateGroupChatRoomRequestModel(
    int GroupId,
    string? ChatType = "FREE",
    int? JoinFeeLinkDrops = 0
);

public sealed record GroupChatMessageModel(
    int GroupChatMessageId,
    int GroupChatRoomId,
    int SenderId,
    string SenderName,
    string? SenderAvatar,
    string Content,
    DateTime CreatedAt
);

public sealed record SendGroupChatMessageRequestModel(
    string Content
);
