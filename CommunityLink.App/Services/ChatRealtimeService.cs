using CommunityLink.App.Components.Shared.Chat;
using CommunityLink.Shared.Features.Chat;
using CommunityLink.Shared.Features.ChatGroup;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.SignalR.Client;
using System.Security.Claims;

namespace CommunityLink.App.Services;

/// <summary>
/// Owns both SignalR connections behind the unified Chat page and normalizes the two
/// incompatible server contracts into <see cref="ChatMessageViewModel"/>.
/// <para>
/// The 1:1 hub sends <c>ChatMessageModel</c> on <c>ReceivePrivateMessage</c>; the group hub
/// sends <c>ChatGroupMessageModel</c> on <c>ReceiveChatGroupMessage</c> to a SignalR group
/// room. Neither event name nor payload shape is changed - they are adapted here instead.
/// </para>
/// </summary>
public sealed class ChatRealtimeService
{
    private readonly IConfiguration _config;
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly ILogger<ChatRealtimeService> _logger;

    private HubConnection? _privateConnection;
    private HubConnection? _groupConnection;
    private TaskCompletionSource<bool>? _starting;
    private int _currentUserId;
    private int? _subscribedGroupId;
    private ChatGroupPermissionSet? _subscribedGroupPermissions;

    public ChatRealtimeService(
        IConfiguration config,
        AuthenticationStateProvider authStateProvider,
        ILogger<ChatRealtimeService> logger)
    {
        _config = config;
        _authStateProvider = authStateProvider;
        _logger = logger;
    }

    public string PrivateStatus { get; private set; } = "Offline";

    public string GroupStatus { get; private set; } = "Offline";

    public string OverallStatus
    {
        get
        {
            if (PrivateStatus == "Live" || GroupStatus == "Live")
            {
                return "Live";
            }

            if (PrivateStatus == "Reconnecting…" || GroupStatus == "Reconnecting…")
            {
                return "Reconnecting…";
            }

            return "Offline";
        }
    }

    public event Func<ChatMessageViewModel, Task>? PrivateMessageReceived;

    public event Func<ChatMessageViewModel, Task>? GroupMessageReceived;

    public event Func<int, bool, Task>? TypingChanged;

    public event Func<int, int, bool, Task>? GroupTypingChanged;

    public event Func<int, Task>? ConversationRead;

    public event Func<int, int, Task>? MessageDeleted;

    public event Func<int, int, List<MessageReactionModel>, Task>? ReactionUpdated;

    public event Func<int, Task>? UserOnline;

    public event Func<int, DateTime?, Task>? UserOffline;

    public event Func<int, Task>? GroupMembersUpdated;

    /// <summary>Raised when an Owner changes group settings, so the header and panel can re-render.</summary>
    public event Func<ChatGroupModel, Task>? GroupUpdated;

    /// <summary>Raised when an Owner replaces the group avatar, carrying the new public URL.</summary>
    public event Func<int, string, Task>? GroupImageUpdated;

    /// <summary>
    /// Raised when the group itself is deleted. Carries the group id so the view can close the
    /// thread instead of leaving a dead conversation on screen.
    /// </summary>
    public event Func<int, Task>? GroupDeleted;

    /// <summary>
    /// Raised when a member is removed or banned. Carries the group id and the affected user
    /// id; the client that matches the second argument must leave the room immediately.
    /// </summary>
    public event Func<int, int, Task>? GroupMemberRemoved;

    /// <summary>
    /// Raised when the group's single pinned message changes. Carries the group id and the new
    /// pin, which is null when the group has nothing pinned. The banner takes the resulting state
    /// rather than the action, so clients cannot drift out of step with the server.
    /// </summary>
    public event Func<int, ChatGroupPinnedMessageModel?, Task>? GroupPinnedMessageChanged;

    /// <summary>
    /// Raised when the owner edits an admin's permission set. Carries the group id, the affected
    /// user id and the new set. Only that user's client acts on it, so an admin's own cached
    /// permissions converge without a manual reload.
    /// </summary>
    public event Func<int, int, ChatGroupPermissionSet, Task>? GroupPermissionsChanged;

    public event Action? StateChanged;

    public async Task EnsureConnectedAsync()
    {
        if (_starting is not null)
        {
            await _starting.Task;
            return;
        }

        _starting = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        try
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            var token = authState.User.FindFirst("access_token")?.Value;
            var nameId = authState.User.FindFirst("sub")?.Value
                ?? authState.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (int.TryParse(nameId, out var parsedUserId))
            {
                _currentUserId = parsedUserId;
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                PrivateStatus = "Offline";
                GroupStatus = "Offline";
                return;
            }

            var baseUrl = (_config["CommunityApi:BaseUrl"] ?? "http://localhost:5000").TrimEnd('/');
            var reconnectPolicy = new[]
            {
                TimeSpan.Zero,
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(10),
                TimeSpan.FromSeconds(30)
            };

            _privateConnection = BuildPrivateConnection(baseUrl, token, reconnectPolicy);
            _groupConnection = BuildGroupConnection(baseUrl, token, reconnectPolicy);

            await Task.WhenAll(StartPrivateAsync(), StartGroupAsync());
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Chat realtime connections could not be started.");
            PrivateStatus = "Offline";
            GroupStatus = "Offline";
        }
        finally
        {
            _starting.TrySetResult(true);
            _starting = null;
            StateChanged?.Invoke();
        }
    }

    private async Task StartPrivateAsync()
    {
        try
        {
            await _privateConnection!.StartAsync();
            PrivateStatus = "Live";
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "/hubs/chat connection failed; 1:1 chat falls back to manual refresh.");
            PrivateStatus = "Offline";
        }
    }

    private async Task StartGroupAsync()
    {
        try
        {
            await _groupConnection!.StartAsync();
            GroupStatus = "Live";
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "/hubs/chat-groups connection failed; group chat falls back to manual refresh.");
            GroupStatus = "Offline";
        }
    }

    private HubConnection BuildPrivateConnection(string baseUrl, string token, TimeSpan[] reconnectPolicy)
    {
        var connection = new HubConnectionBuilder()
            .WithUrl($"{baseUrl}/hubs/chat", options =>
            {
                options.AccessTokenProvider = () => Task.FromResult<string?>(token);
            })
            .WithAutomaticReconnect(reconnectPolicy)
            .Build();

        connection.On<ChatMessageModel>("ReceivePrivateMessage", async message =>
        {
            if (PrivateMessageReceived is null)
            {
                return;
            }

            await PrivateMessageReceived.Invoke(message.ToMessage(
                ChatThreadType.Private,
                message.ConversationId,
                _currentUserId));
        });

        connection.On<int, bool>("Typing", (senderId, isTyping) =>
        {
            if (TypingChanged is not null)
            {
                return TypingChanged.Invoke(senderId, isTyping);
            }

            return Task.CompletedTask;
        });

        connection.On<int, int>("MessageRead", (conversationId, _) =>
        {
            if (ConversationRead is not null)
            {
                return ConversationRead.Invoke(conversationId);
            }

            return Task.CompletedTask;
        });

        connection.On<int, int>("MessageDeleted", (conversationId, messageId) =>
        {
            if (MessageDeleted is not null)
            {
                return MessageDeleted.Invoke(conversationId, messageId);
            }

            return Task.CompletedTask;
        });

        connection.On<int, int, List<MessageReactionModel>>("MessageReactionUpdated", (conversationId, messageId, reactions) =>
        {
            if (ReactionUpdated is not null)
            {
                return ReactionUpdated.Invoke(conversationId, messageId, reactions);
            }

            return Task.CompletedTask;
        });

        connection.On<int>("UserOnline", userId =>
        {
            if (UserOnline is not null)
            {
                return UserOnline.Invoke(userId);
            }

            return Task.CompletedTask;
        });

        connection.On<int, DateTime?>("UserOffline", (userId, lastActiveAt) =>
        {
            if (UserOffline is not null)
            {
                return UserOffline.Invoke(userId, lastActiveAt);
            }

            return Task.CompletedTask;
        });

        connection.Reconnecting += _ =>
        {
            PrivateStatus = "Reconnecting…";
            StateChanged?.Invoke();
            return Task.CompletedTask;
        };

        connection.Reconnected += async _ =>
        {
            PrivateStatus = "Live";
            StateChanged?.Invoke();
            if (_subscribedGroupId.HasValue)
            {
                await TryInvokeGroupAsync("JoinChatGroup", _subscribedGroupId.Value);
            }
        };

        connection.Closed += _ =>
        {
            PrivateStatus = "Offline";
            StateChanged?.Invoke();
            return Task.CompletedTask;
        };

        return connection;
    }

    private HubConnection BuildGroupConnection(string baseUrl, string token, TimeSpan[] reconnectPolicy)
    {
        var connection = new HubConnectionBuilder()
            .WithUrl($"{baseUrl}/hubs/chat-groups", options =>
            {
                options.AccessTokenProvider = () => Task.FromResult<string?>(token);
            })
            .WithAutomaticReconnect(reconnectPolicy)
            .Build();

        connection.On<ChatGroupMessageModel>("ReceiveChatGroupMessage", async message =>
        {
            if (GroupMessageReceived is null)
            {
                return;
            }

            await GroupMessageReceived.Invoke(message.ToMessage(
                message.ChatGroupId,
                _currentUserId,
                _subscribedGroupPermissions));
        });

        connection.On<int, int, bool>("GroupTyping", (chatGroupId, senderId, isTyping) =>
        {
            if (GroupTypingChanged is not null)
            {
                return GroupTypingChanged.Invoke(chatGroupId, senderId, isTyping);
            }

            return Task.CompletedTask;
        });

        connection.On<int, int>("ChatGroupMessageDeleted", (chatGroupId, messageId) =>
        {
            if (MessageDeleted is not null)
            {
                return MessageDeleted.Invoke(chatGroupId, messageId);
            }

            return Task.CompletedTask;
        });

        connection.On<int, int, List<MessageReactionModel>>("ChatGroupReactionUpdated", (chatGroupId, messageId, reactions) =>
        {
            if (ReactionUpdated is not null)
            {
                return ReactionUpdated.Invoke(chatGroupId, messageId, reactions);
            }

            return Task.CompletedTask;
        });

        connection.On<int>("GroupMemberUpdated", chatGroupId =>
        {
            if (GroupMembersUpdated is not null)
            {
                return GroupMembersUpdated.Invoke(chatGroupId);
            }

            return Task.CompletedTask;
        });

        connection.On<ChatGroupModel>("ChatGroupUpdated", group =>
        {
            if (GroupUpdated is not null)
            {
                return GroupUpdated.Invoke(group);
            }

            return Task.CompletedTask;
        });

        connection.On<int, string>("ChatGroupImageUpdated", (chatGroupId, imageUrl) =>
        {
            if (GroupImageUpdated is not null)
            {
                return GroupImageUpdated.Invoke(chatGroupId, imageUrl);
            }

            return Task.CompletedTask;
        });

        connection.On<int>("ChatGroupDeleted", chatGroupId =>
        {
            if (GroupDeleted is not null)
            {
                return GroupDeleted.Invoke(chatGroupId);
            }

            return Task.CompletedTask;
        });

        connection.On<int, int>("ChatGroupMemberRemoved", (chatGroupId, userId) =>
        {
            if (GroupMemberRemoved is not null)
            {
                return GroupMemberRemoved.Invoke(chatGroupId, userId);
            }

            return Task.CompletedTask;
        });

        // The pin is broadcast as the resulting state, null included: a JSON null body arrives as
        // a null argument here rather than as a missing payload, so unpin needs no special case.
        connection.On<int, ChatGroupPinnedMessageModel?>("ChatGroupPinnedMessageChanged", (chatGroupId, pinned) =>
        {
            if (GroupPinnedMessageChanged is not null)
            {
                return GroupPinnedMessageChanged.Invoke(chatGroupId, pinned);
            }

            return Task.CompletedTask;
        });

        connection.On<int, int, ChatGroupPermissionSet>("ChatGroupPermissionsChanged",
            (chatGroupId, userId, permissions) =>
            {
                if (GroupPermissionsChanged is not null)
                {
                    return GroupPermissionsChanged.Invoke(chatGroupId, userId, permissions);
                }

                return Task.CompletedTask;
            });

        connection.Reconnecting += _ =>
        {
            GroupStatus = "Reconnecting…";
            StateChanged?.Invoke();
            return Task.CompletedTask;
        };

        connection.Reconnected += async _ =>
        {
            GroupStatus = "Live";
            StateChanged?.Invoke();
            if (_subscribedGroupId.HasValue)
            {
                await TryInvokeGroupAsync("JoinChatGroup", _subscribedGroupId.Value);
            }
        };

        connection.Closed += _ =>
        {
            GroupStatus = "Offline";
            StateChanged?.Invoke();
            return Task.CompletedTask;
        };

        return connection;
    }

    /// <summary>
    /// Tracks the room the page is currently showing. The viewer's permission set is supplied by
    /// the page because only it knows the membership loaded for the open thread; a hard-coded
    /// role would hide the moderation affordances from owners and grant them to stripped admins
    /// on live messages.
    /// </summary>
    public void SetSubscribedGroupPermissions(ChatGroupPermissionSet? permissions) =>
        _subscribedGroupPermissions = permissions;

    public async Task SetSubscribedGroupAsync(int? chatGroupId)
    {
        if (chatGroupId.HasValue)
        {
            _subscribedGroupPermissions = null;
        }

        if (_subscribedGroupId == chatGroupId)
        {
            return;
        }

        if (_subscribedGroupId.HasValue)
        {
            await TryInvokeGroupAsync("LeaveChatGroup", _subscribedGroupId.Value);
        }

        _subscribedGroupId = chatGroupId;

        if (chatGroupId.HasValue)
        {
            await TryInvokeGroupAsync("JoinChatGroup", chatGroupId.Value);
        }
    }

    /// <summary>
    /// Leaves the current group room without changing the subscription, used when this client
    /// is the one being removed. <see cref="SetSubscribedGroupAsync"/> would also work but reads
    /// as "unsubscribe", which is not what happens when the caller keeps the group open.
    /// </summary>
    public async Task LeaveGroupRoomAsync(int chatGroupId)
    {
        if (_subscribedGroupId == chatGroupId)
        {
            await TryInvokeGroupAsync("LeaveChatGroup", chatGroupId);
        }
    }

    public async Task SendTypingAsync(int recipientId, bool isTyping)
    {
        if (_privateConnection is null || _privateConnection.State != HubConnectionState.Connected)
        {
            return;
        }

        try
        {
            await _privateConnection.InvokeAsync("SendTyping", recipientId, isTyping);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Typing indicator could not be sent.");
        }
    }

    public async Task SendGroupTypingAsync(int chatGroupId, bool isTyping)
    {
        if (_groupConnection is null || _groupConnection.State != HubConnectionState.Connected)
        {
            return;
        }

        try
        {
            await _groupConnection.InvokeAsync("SendGroupTyping", chatGroupId, isTyping);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Group typing indicator could not be sent.");
        }
    }

    public async Task MarkAsReadAsync(int conversationId, int recipientId)
    {
        if (_privateConnection is null || _privateConnection.State != HubConnectionState.Connected)
        {
            return;
        }

        try
        {
            await _privateConnection.InvokeAsync("MarkAsRead", conversationId, recipientId);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Read receipt could not be sent.");
        }
    }

    private async Task TryInvokeGroupAsync(string method, int chatGroupId)
    {
        if (_groupConnection is null || _groupConnection.State != HubConnectionState.Connected)
        {
            return;
        }

        try
        {
            await _groupConnection.InvokeAsync(method, chatGroupId);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "SignalR group room call {Method} failed.", method);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_privateConnection is not null)
        {
            await _privateConnection.DisposeAsync();
        }

        if (_groupConnection is not null)
        {
            await _groupConnection.DisposeAsync();
        }
    }
}
