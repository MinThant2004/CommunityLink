namespace CommunityLink.App.Components.Shared.Chat;

using CommunityLink.Shared.Features.Chat;
using CommunityLink.Shared.Features.ChatGroup;

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

    /// <summary>
    /// True when the viewer may pin this message: either they sent it, or they hold
    /// CanPinMessages. The server enforces the same rule, so this only decides whether the
    /// control is rendered.
    /// </summary>
    public bool CanPin { get; init; }

    /// <summary>True when this message is the group's pinned one.</summary>
    public bool IsPinned { get; init; }

    /// <summary>True when the viewer may clear the pin. Distinct from <see cref="CanPin"/>:
    /// unpinning someone else's message is moderation, pinning your own never is.</summary>
    public bool CanUnpin { get; init; }

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
        Reactions = Reactions,
        CanPin = CanPin,
        IsPinned = IsPinned,
        CanUnpin = CanUnpin
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
        Reactions = reactions,
        CanPin = CanPin,
        IsPinned = IsPinned,
        CanUnpin = CanUnpin
    };

    /// <summary>
    /// Recomputes the moderation flags after the viewer's own permission set changes. Derived
    /// purely from the new set and message ownership rather than from the previously mapped flags,
    /// which would still carry the powers the viewer has just been stripped of.
    /// </summary>
    public ChatMessageViewModel WithViewerPermissions(
        ChatGroupPermissionSet? permissions,
        int currentUserId,
        int? pinnedMessageId)
    {
        var isMine = SenderId == currentUserId;
        var canDelete = isMine || (permissions?.CanDeleteMessages ?? false);
        var canPin = isMine || (permissions?.CanPinMessages ?? false);

        return new ChatMessageViewModel
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
            IsMine = isMine,
            IsRead = IsRead,
            CanDelete = canDelete,
            CanDeleteForEveryone = canDelete,
            CanDeleteForSelf = CanDeleteForSelf,
            ReplyToMessageId = ReplyToMessageId,
            ReplyToSenderName = ReplyToSenderName,
            ReplyToPreview = ReplyToPreview,
            ReplyToIsDeleted = ReplyToIsDeleted,
            Reactions = Reactions,
            CanPin = canPin,
            CanUnpin = canPin,
            IsPinned = pinnedMessageId.HasValue && pinnedMessageId.Value == Id
        };
    }

    /// <summary>
    /// Flips the pin state on one message without disturbing the rest of the list, so the bubble
    /// and the header banner stay in step from a single source of truth.
    /// </summary>
    public ChatMessageViewModel WithPinnedState(bool isPinned) => new()
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
        Reactions = Reactions,
        CanPin = CanPin,
        IsPinned = isPinned,
        CanUnpin = CanUnpin
    };
}
