using System;
using System.Collections.Generic;

namespace CommunityLink.Shared.Features.Administration;

public sealed record AdminDashboardStatsModel(
    int TotalUsers,
    int TotalCommunities,
    int TotalPosts,
    int TotalPolls,
    int ActiveConversations,
    int TotalAuditLogs);

public sealed record TrendPointModel(
    DateTime Date,
    int NewUsers,
    int NewPosts,
    int ActiveUsers);

public sealed record JoinRequestModel(
    int RequestId,
    int CommunityId,
    string CommunityName,
    int UserId,
    string UserName,
    string UserEmail,
    string Status,
    DateTime CreatedAt);

public sealed record AdminDashboardModel(
    AdminDashboardStatsModel Stats,
    IReadOnlyList<TrendPointModel> Trends,
    IReadOnlyList<JoinRequestModel> PendingJoinRequests);

public sealed record AuditLogModel(
    int AuditLogId,
    int? UserId,
    string? UserName,
    string? ActorType,
    string Action,
    string EntityName,
    string? EntityId,
    string? Details,
    string? IpAddress,
    DateTime CreatedAt);