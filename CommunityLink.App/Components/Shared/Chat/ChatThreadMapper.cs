using CommunityLink.Shared.Features.Chat;
using CommunityLink.Shared.Features.ChatGroup;

namespace CommunityLink.App.Components.Shared.Chat;

/// <summary>
/// Normalizes the two backend chat contracts into the single UI contract used by the Chat page.
/// The 1:1 and group stacks share no DTO properties (MessageText/Content, SentAt/CreatedAt,
/// MessageId/ChatGroupMessageId), so every read/write goes through here.
/// </summary>
public static class ChatThreadMapper
{
    public static ChatThreadViewModel ToThread(this ConversationModel model, int currentUserId)
    {
        var isMine = model.OtherUserId != currentUserId;
        var title = isMine
            ? "You and " + model.OtherDisplayName
            : model.OtherDisplayName;

        return new ChatThreadViewModel
        {
            Type = ChatThreadType.Private,
            Id = model.ConversationId,
            Title = title,
            AvatarUrl = model.OtherAvatarUrl,
            Subtitle = "@" + model.OtherUserName,
            LastMessagePreview = model.LastMessagePreview,
            LastActivityAt = model.LastMessageAt?.ToLocalTime() ?? model.LastMessageAt,
            DirectUserId = model.OtherUserId,
            IsUnlocked = true
        };
    }

    public static ChatThreadViewModel ToThread(this ChatGroupModel model) => new()
    {
        Type = ChatThreadType.Group,
        Id = model.ChatGroupId,
        Title = model.Name,
        AvatarUrl = model.AvatarUrl,
        Description = model.Description,
        CreatorName = model.CreatorName,
        CommissionPercentage = model.CommissionPercentageSnapshot,
        Subtitle = model.MemberCount + (model.MemberCount == 1 ? " member" : " members"),
        MemberCount = model.MemberCount,
        IsJoined = model.IsJoined,
        UserRole = model.UserRole,
        RequiresPayment = string.Equals(model.ChatType, "PAID", StringComparison.OrdinalIgnoreCase),
        FeeLinkDrops = model.JoinFeeLinkDrops,
        IsUnlocked = model.IsJoined,
        Badge = string.Equals(model.ChatType, "PAID", StringComparison.OrdinalIgnoreCase) ? "Paid" : "Free"
    };

    public static ChatThreadViewModel WithPreview(
        this ChatThreadViewModel thread,
        ChatGroupPreviewModel? preview)
    {
        if (thread.IsDirect || preview is null)
        {
            return thread;
        }

        thread.LastMessagePreview = preview.LastMessagePreview;
        thread.LastActivityAt = preview.LastMessageAt?.ToLocalTime() ?? preview.LastMessageAt;
        return thread;
    }

    public static ChatMessageViewModel ToMessage(
        this ChatMessageModel model,
        ChatThreadType threadType,
        int threadId,
        int currentUserId) => new()
    {
        Id = model.MessageId,
        ThreadType = threadType,
        ThreadId = threadId,
        SenderId = model.SenderId,
        SenderName = model.SenderName,
        SenderAvatar = model.SenderAvatar,
        Content = model.MessageText,
        SentAtLocal = model.SentAt.ToLocalTime(),
        IsMine = model.SenderId == currentUserId,
        IsRead = model.IsRead,
        CanDelete = false
    };

    public static ChatMessageViewModel ToMessage(
        this ChatGroupMessageModel model,
        int threadId,
        int currentUserId,
        string currentUserRole)
    {
        var canDelete = model.SenderId == currentUserId
            || currentUserRole is "OWNER" or "ADMIN";

        return new ChatMessageViewModel
        {
            Id = model.ChatGroupMessageId,
            ThreadType = ChatThreadType.Group,
            ThreadId = threadId,
            SenderId = model.SenderId,
            SenderName = string.IsNullOrWhiteSpace(model.SenderDisplayName)
                ? model.SenderName
                : model.SenderDisplayName,
            SenderAvatar = model.SenderAvatar,
            Content = model.Content,
            SentAtLocal = model.CreatedAt.ToLocalTime(),
            IsMine = model.SenderId == currentUserId,
            CanDelete = canDelete
        };
    }

    public static string FormatActivityTime(DateTime? utcActivity)
    {
        if (utcActivity is null)
        {
            return string.Empty;
        }

        var local = utcActivity.Value.ToLocalTime();
        var today = DateTime.Now.Date;

        if (local.Date == today)
        {
            return local.ToString("h:mm tt");
        }

        if (local.Date == today.AddDays(-1))
        {
            return "Yesterday";
        }

        return local.Date.Year == today.Year
            ? local.ToString("MMM d")
            : local.ToString("MMM d, yyyy");
    }

    public static string FormatDateSeparator(DateTime localDate)
    {
        var today = DateTime.Today;

        if (localDate == today)
        {
            return "Today";
        }

        if (localDate == today.AddDays(-1))
        {
            return "Yesterday";
        }

        return localDate.Year == today.Year
            ? localDate.ToString("MMMM d")
            : localDate.ToString("MMMM d, yyyy");
    }

    public static string FormatPreview(string? preview)
    {
        if (string.IsNullOrWhiteSpace(preview))
        {
            return "No messages yet";
        }

        var single = preview.ReplaceLineEndings(" ").Trim();
        return single.Length <= 90 ? single : single[..90] + "…";
    }
}
