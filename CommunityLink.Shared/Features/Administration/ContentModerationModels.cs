using System;
using System.Collections.Generic;

namespace CommunityLink.Shared.Features.Administration;

public sealed record ContentReportModel(
    int ContentReportId,
    string ContentType, // "POST" or "POLL"
    int ContentId,
    int ReporterUserId,
    string ReporterName,
    string? ReporterAvatar,
    string ReasonCategory,
    string? Details,
    string Status, // "PENDING", "RESOLVED", "DISMISSED"
    int? HandledByAdminId,
    string? AdminNote,
    DateTime? HandledAt,
    DateTime CreatedAt,
    // Target Content Details
    int ContentAuthorId,
    string ContentAuthorName,
    string? ContentAuthorAvatar,
    string ContentSummary,
    string? CommunityName,
    string? GroupName,
    bool IsContentActive,
    bool IsContentPrivate,
    DateTime ContentCreatedAt);

public sealed record CreateContentReportRequestModel(
    string ContentType,
    int ContentId,
    string ReasonCategory,
    string? Details);

public sealed record ModerateContentRequestModel(
    string Action, // "BAN", "UNBAN", "SET_PRIVATE", "SET_PUBLIC", "DEACTIVATE", "ACTIVATE"
    string? ReasonCategory,
    string? ReasonNote,
    int? ReportId = null,
    string? ContentType = null,
    int? ContentId = null);

public sealed record ContentItemAdminModel(
    int Id,
    string ContentType, // "POST" or "POLL"
    int AuthorId,
    string AuthorName,
    string? AuthorAvatar,
    string Content,
    string? Question,
    DateTime CreatedAt,
    string? CommunityName,
    string? GroupName,
    bool IsActive,
    bool IsPrivate,
    string? ModerationReason,
    int ReportCount,
    IReadOnlyList<string>? ImageUrls = null,
    string? CodeSnippet = null,
    string? CodeFileName = null,
    IReadOnlyList<string>? PollOptions = null);

public sealed record GroupItemAdminModel(
    int GroupId,
    string Name,
    string Slug,
    string? Description,
    string? AvatarUrl,
    string? SubCommunityName,
    int CreatorId,
    string CreatorName,
    string Visibility,
    int MemberCount,
    int PostCount,
    bool IsActive,
    string? ModerationReason,
    int ReportCount,
    DateTime CreatedAt);
