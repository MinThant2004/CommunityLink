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
    string? OldValues,
    string? NewValues,
    string? ChangedColumns,
    string? Details,
    string? IpAddress,
    DateTime CreatedAt,
    DateTime? UpdatedAt = null);

public sealed record CreateAdminInviteRequestModel(
    string Email,
    string? FullName = null,
    bool IsSuperAdmin = false);

public sealed record VerifyAdminInviteResponseModel(
    string Email,
    string? FullName,
    string RoleName,
    bool IsSuperAdmin,
    DateTime ExpiresAtUtc);

public sealed record SetupAdminPasswordRequestModel(
    string Token,
    string Password,
    string? ConfirmPassword = null);

public sealed record AdminAccountModel(
    int AdminId,
    string Email,
    string FullName,
    string RoleCode,
    string RoleName,
    bool IsSuperAdmin,
    bool IsActive,
    DateTime? LastLoginAt,
    DateTime CreatedAt);