using System;

namespace CommunityLink.Shared.Features.Activity;

public sealed record UserActivityModel(
    long ActivityId,
    int UserId,
    string ActivityType,
    string Description,
    string? TargetEntityType,
    int? TargetEntityId,
    DateTime CreatedAt);
