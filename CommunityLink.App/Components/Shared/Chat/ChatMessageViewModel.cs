namespace CommunityLink.App.Components.Shared.Chat;

public sealed class ChatMessageViewModel
{
    public int Id { get; init; }

    public ChatThreadType ThreadType { get; init; }

    public int ThreadId { get; init; }

    public int SenderId { get; init; }

    public string SenderName { get; init; } = string.Empty;

    public string? SenderAvatar { get; init; }

    public string Content { get; init; } = string.Empty;

    public DateTime SentAtLocal { get; init; }

    public bool IsMine { get; init; }

    public bool IsRead { get; init; }

    public bool CanDelete { get; init; }

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
        SentAtLocal = SentAtLocal,
        IsMine = IsMine,
        IsRead = true,
        CanDelete = CanDelete
    };
}
