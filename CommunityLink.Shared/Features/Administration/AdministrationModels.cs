namespace CommunityLink.Shared.Features.Administration;

public sealed record AdminDashboardStatsModel(
    int TotalUsers,
    int TotalCommunities,
    int TotalPosts,
    int TotalPolls,
    int ActiveConversations,
    int TotalAuditLogs);

public sealed record AuditLogModel(
    int AuditLogId,
    int? UserId,
    string? UserName,
    string Action,
    string EntityName,
    string? EntityId,
    string? Details,
    string? IpAddress,
    DateTime CreatedAt);