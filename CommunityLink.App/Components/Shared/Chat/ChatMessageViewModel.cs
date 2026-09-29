namespace CommunityLink.App.Components.Shared.Chat;

using CommunityLink.Shared.Features.Chat;

public sealed class ChatMessageViewModel
{
    public int Id { get; init; }

    public ChatThreadType ThreadType { get; init; }

    public int ThreadId { get; init; }

    public int SenderId { get; init; }

    public string SenderName { get; init; } = string.Empty;

    public string? SenderAvatar { get; init; }

    public string Content { get; init; } = string.Empty;

    public string MessageType { get; init; } = "TEXT";

    public string? AttachmentUrl { get; init; }

    public string? FileName { get; init; }

    public long? FileSizeByte { get; init; }

    public string? FormattedFileSize { get; init; }

    public DateTime SentAtLocal { get; init; }

    public bool IsMine { get; init; }

    public bool IsRead { get; init; }

    public bool CanDelete { get; init; }

    public bool CanDeleteForEveryone { get; init; }

    public bool CanDeleteForSelf { get; init; } = true;

    public int? ReplyToMessageId { get; init; }

    public string? ReplyToSenderName { get; init; }

    public string? ReplyToPreview { get; init; }

    public bool ReplyToIsDeleted { get; init; }

    public IReadOnlyList<MessageReactionModel>? Reactions { get; init; }

    /// <summary>Stable key used to de-duplicate REST responses against SignalR pushes.</summary>
    public string DedupeKey => $"{(int)ThreadType}:{ThreadId}:{Id}";

    public string SentTime => SentAtLocal.ToString("h:mm tt");

    public string SentDateKey => SentAtLocal.ToString("yyyy-MM-dd");

    public ChatMessageViewModel WithRead() => new()
    {
        Id = Id,
        ThreadType = ThreadType,
        ThreadId = ThreadId,
        SenderId = SenderId,
        SenderName = SenderName,
        SenderAvatar = SenderAvatar,
        Content = Content,
        MessageType = MessageType,
        AttachmentUrl = AttachmentUrl,
        FileName = FileName,
        FileSizeByte = FileSizeByte,
        FormattedFileSize = FormattedFileSize,
        SentAtLocal = SentAtLocal,
        IsMine = IsMine,
        IsRead = true,
        CanDelete = CanDelete,
        CanDeleteForEveryone = CanDeleteForEveryone,
        CanDeleteForSelf = CanDeleteForSelf,
        ReplyToMessageId = ReplyToMessageId,
        ReplyToSenderName = ReplyToSenderName,
        ReplyToPreview = ReplyToPreview,
        ReplyToIsDeleted = ReplyToIsDeleted,
        Reactions = Reactions
    };

    public ChatMessageViewModel WithReactions(IReadOnlyList<MessageReactionModel>? reactions) => new()
    {
        Id = Id,
        ThreadType = ThreadType,
        ThreadId = ThreadId,
        SenderId = SenderId,
        SenderName = SenderName,
        SenderAvatar = SenderAvatar,
        Content = Content,
        MessageType = MessageType,
        AttachmentUrl = AttachmentUrl,
        FileName = FileName,
        FileSizeByte = FileSizeByte,
        FormattedFileSize = FormattedFileSize,
        SentAtLocal = SentAtLocal,
        IsMine = IsMine,
        IsRead = IsRead,
        CanDelete = CanDelete,
        CanDeleteForEveryone = CanDeleteForEveryone,
        CanDeleteForSelf = CanDeleteForSelf,
        ReplyToMessageId = ReplyToMessageId,
        ReplyToSenderName = ReplyToSenderName,
        ReplyToPreview = ReplyToPreview,
        ReplyToIsDeleted = ReplyToIsDeleted,
        Reactions = reactions
    };
}
