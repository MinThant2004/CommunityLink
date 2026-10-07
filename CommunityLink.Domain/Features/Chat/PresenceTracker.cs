using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace CommunityLink.Domain.Features.Chat;

public class PresenceTracker
{
    private readonly ConcurrentDictionary<int, HashSet<string>> _onlineUsers = new();

    /// <summary>
    /// Tracks a user connection. Returns true if this is the user's first active connection (they just came online).
    /// </summary>
    public bool UserConnected(int userId, string connectionId)
    {
        bool isFirstConnection = false;

        _onlineUsers.AddOrUpdate(
            userId,
            _ =>
            {
                isFirstConnection = true;
                return new HashSet<string> { connectionId };
            },
            (_, connections) =>
            {
                lock (connections)
                {
                    if (connections.Count == 0)
                    {
                        isFirstConnection = true;
                    }
                    connections.Add(connectionId);
                }
                return connections;
            });

        return isFirstConnection;
    }

    /// <summary>
    /// Removes a user connection. Returns true if this was the user's last connection (they just went offline).
    /// </summary>
    public bool UserDisconnected(int userId, string connectionId)
    {
        bool isLastConnection = false;

        if (_onlineUsers.TryGetValue(userId, out var connections))
        {
            lock (connections)
            {
                connections.Remove(connectionId);
                if (connections.Count == 0)
                {
                    isLastConnection = true;
                }
            }

            if (isLastConnection)
            {
                _onlineUsers.TryRemove(userId, out _);
            }
        }

        return isLastConnection;
    }

    /// <summary>
    /// Returns true if the specified user is currently online.
    /// </summary>
    public bool IsUserOnline(int userId)
    {
        if (_onlineUsers.TryGetValue(userId, out var connections))
        {
            lock (connections)
            {
                return connections.Count > 0;
            }
        }
        return false;
    }

    /// <summary>
    /// Returns the IDs of all users currently online.
    /// </summary>
    public HashSet<int> GetOnlineUserIds()
    {
        return _onlineUsers.Keys.ToHashSet();
    }
}
