namespace CommunityLink.Shared.Features.ChatGroup;

/// <summary>
/// Result of a Chat Group avatar upload. Only <see cref="Url"/> is persisted on the group row;
/// the other fields are echoed back so the client can confirm what actually landed.
/// </summary>
public sealed record ChatGroupImageUploadResponse(
    string Url,
    string FileName,
    long FileSizeByte);
