namespace CommunityLink.Shared.Features.Chat;

/// <summary>
/// A single person's reaction to a message. The per-message list is flat on purpose: the
/// client groups it by <see cref="Emoji"/> into chips, and tests can assert on the exact
/// reactors. One person holds at most one reaction per message.
/// </summary>
public sealed record MessageReactionModel(int UserId, string UserName, string Emoji);
