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
    private string? _subscribedGroupRole;

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

    public event Func<int, Task>? GroupMembersUpdated;

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
                _subscribedGroupRole ?? "MEMBER"));
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

    /// <summary>Moves the SignalR room subscription to the given group, or clears it for 1:1 threads.</summary>
    /// <summary>
    /// Tracks the room the page is currently showing. The viewer's role is supplied by the
    /// page because only it knows the membership loaded for the open thread; a hard-coded
    /// role would hide the delete affordance from owners and admins on live messages.
    /// </summary>
    public void SetSubscribedGroupRole(string? role) => _subscribedGroupRole = role;

    public async Task SetSubscribedGroupAsync(int? chatGroupId)
    {
        if (chatGroupId.HasValue)
        {
            _subscribedGroupRole = null;
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
