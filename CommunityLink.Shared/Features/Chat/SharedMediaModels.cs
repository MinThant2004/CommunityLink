namespace CommunityLink.Shared.Features.Chat;

using System;
using System.Collections.Generic;

public sealed record SharedMediaItemModel(
    int MessageId,
    string MessageType, // IMAGE | VIDEO | FILE
    string AttachmentUrl,
    string? FileName,
    long? FileSizeByte,
    DateTime CreatedAt,
    int SenderId,
    string SenderName,
    string? SenderAvatarUrl
)
{
    public string SenderDisplayName => SenderName;
}

public sealed record SharedMediaCountsModel(
    int PhotosCount,
    int VideosCount,
    int FilesCount
)
{
    public int PhotoCount => PhotosCount;
    public int VideoCount => VideosCount;
    public int FileCount => FilesCount;
}

public sealed record SharedMediaPagedResultModel(
    IReadOnlyList<SharedMediaItemModel> Items,
    int TotalCount,
    int Page,
    int PageSize
)
{
    public bool HasNextPage => Page * PageSize < TotalCount;
}
