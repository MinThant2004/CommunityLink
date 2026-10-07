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
        return new ChatThreadViewModel
        {
            Type = ChatThreadType.Private,
            Id = model.ConversationId,
            Title = model.OtherDisplayName,
            AvatarUrl = model.OtherAvatarUrl,
            Subtitle = "@" + model.OtherUserName,
            LastMessagePreview = model.LastMessagePreview,
            LastActivityAt = model.LastMessageAt?.ToLocalTime() ?? model.LastMessageAt,
            DirectUserId = model.OtherUserId,
            IsUnlocked = true,
            IsBlockedByMe = model.IsBlockedByMe,
            IsBlockedByTarget = model.IsBlockedByTarget,
            IsOnline = model.IsOnline,
            OtherUserLastActiveAt = model.LastActiveAt
        };
    }

    public static string FormatPresenceText(bool isOnline, DateTime? lastActiveAt)
    {
        if (isOnline)
        {
            return "online";
        }

        if (!lastActiveAt.HasValue)
        {
            return "offline";
        }

        var localTime = lastActiveAt.Value.ToLocalTime();
        var now = DateTime.Now;
        var diff = now - localTime;

        if (diff.TotalMinutes < 1)
        {
            return "last seen just now";
        }
        if (diff.TotalMinutes < 60)
        {
            int mins = (int)diff.TotalMinutes;
            return $"last seen {mins} {(mins == 1 ? "minute" : "minutes")} ago";
        }
        if (localTime.Date == now.Date)
        {
            return $"last seen at {localTime:h:mm tt}";
        }
        if (localTime.Date == now.Date.AddDays(-1))
        {
            return $"last seen yesterday at {localTime:h:mm tt}";
        }

        return $"last seen {localTime:MMM d, yyyy}";
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
        IsBanned = model.IsBanned,
        UserRole = model.UserRole,
        ViewerPermissions = model.ViewerPermissions,
        IsInvited = model.IsInvited,
        InviteFeeLinkDrops = model.InviteFeeLinkDrops,
        InvitedByName = model.InvitedByName,
        RequiresPayment = string.Equals(model.ChatType, "PAID", StringComparison.OrdinalIgnoreCase),
        // An invitee is shown the price they were quoted, not the group's current fee, so a fee
        // change after the invite cannot silently reprice it. Falls back to the live fee when
        // there is no invite (the ordinary self-service join path).
        FeeLinkDrops = model.IsInvited && model.InviteFeeLinkDrops.HasValue
            ? model.InviteFeeLinkDrops.Value
            : model.JoinFeeLinkDrops,
        IsUnlocked = model.IsJoined && !model.IsBanned,
        AccessMode = model.AccessMode ?? "PUBLIC",
        Badge = model.IsBanned ? "Banned" : (string.Equals(model.ChatType, "PAID", StringComparison.OrdinalIgnoreCase) ? "Paid" : "Free")
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
        MessageType = model.MessageType ?? "TEXT",
        AttachmentUrl = model.AttachmentUrl,
        FileName = model.FileName,
        FileSizeByte = model.FileSizeByte,
        FormattedFileSize = model.FormattedFileSize,
        SentAtLocal = model.SentAt.ToLocalTime(),
        IsMine = model.SenderId == currentUserId,
        IsRead = model.IsRead,
        CanDelete = model.SenderId == currentUserId,
        CanDeleteForEveryone = model.CanDeleteForEveryone || model.SenderId == currentUserId,
        CanDeleteForSelf = true,
        ReplyToMessageId = model.ReplyToMessageId,
        ReplyToSenderName = model.ReplyToSenderName,
        ReplyToPreview = model.ReplyToPreview,
        ReplyToIsDeleted = model.ReplyToIsDeleted,
        Reactions = model.Reactions
    };

    public static ChatMessageViewModel ToMessage(
        this ChatGroupMessageModel model,
        int threadId,
        int currentUserId,
        ChatGroupPermissionSet? viewerPermissions,
        int? pinnedMessageId = null)
    {
        // Permissions, not role: an admin stripped of CanDeleteMessages/CanPinMessages keeps the
        // ADMIN label but must not be offered the controls. The owner is reported as holding
        // everything, so this covers that case too.
        var canDelete = model.SenderId == currentUserId
            || (model.CanDeleteForEveryone)
            || (viewerPermissions?.CanDeleteMessages ?? false);

        var canPin = model.SenderId == currentUserId
            || (viewerPermissions?.CanPinMessages ?? false);

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
            MessageType = model.MessageType ?? "TEXT",
            AttachmentUrl = model.AttachmentUrl,
            FileName = model.FileName,
            FileSizeByte = model.FileSizeByte,
            FormattedFileSize = model.FormattedFileSize,
            SentAtLocal = model.CreatedAt.ToLocalTime(),
            IsMine = model.SenderId == currentUserId,
            CanDelete = canDelete,
            CanDeleteForEveryone = model.CanDeleteForEveryone || canDelete,
            CanDeleteForSelf = true,
            ReplyToMessageId = model.ReplyToChatGroupMessageId,
            ReplyToSenderName = model.ReplyToSenderName,
            ReplyToPreview = model.ReplyToPreview,
            ReplyToIsDeleted = model.ReplyToIsDeleted,
            Reactions = model.Reactions,
            CanPin = canPin,
            CanUnpin = canPin,
            IsPinned = pinnedMessageId.HasValue && pinnedMessageId.Value == model.ChatGroupMessageId
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
