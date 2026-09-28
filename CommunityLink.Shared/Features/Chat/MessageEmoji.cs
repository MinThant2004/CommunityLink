using System;
using System.Collections.Generic;
using System.Linq;

namespace CommunityLink.Shared.Features.Chat;

/// <summary>
/// The fixed emoji palette offered for message reactions, plus the server-side validator.
/// <para>
/// Keeping the list here means the reaction picker, the composer and the API all agree, and
/// the service can reject anything outside it rather than storing arbitrary user input in an
/// <c>Emoji</c> column that is later rendered in every participant's message list.
/// </para>
/// <para>
/// Glyphs are written as escapes on purpose: a literal emoji in a .cs file risks being mangled
/// by an editor or a diff tool that does not preserve UTF-8.
/// </para>
/// </summary>
public static class MessageEmoji
{
    public static readonly IReadOnlyList<string> Allowed =
    [
        "\U0001F44D", // thumbs up
        "\U0001F44E", // thumbs down
        "❤️",        // red heart
        "\U0001F525", // fire
        "\U0001F602", // joy
        "\U0001F62E", // astonished
        "\U0001F622", // crying
        "\U0001F64F", // folded hands
        "\U0001F44F", // clapping
        "\U0001F389", // party popper
        "\U0001F680", // rocket
        "\U0001F4A1", // light bulb
        "\U0001F60A"  // smiling
    ];

    private static readonly Dictionary<string, string> ByMatchKey =
        Allowed.ToDictionary(NormalizeKey, StringComparer.Ordinal);

    /// <summary>
    /// Maps a client-supplied emoji onto its canonical entry in <see cref="Allowed"/>, or
    /// null when it is not supported. Presentation selectors and zero-width joiners are
    /// ignored so "heart" and "heart with variant 16" are treated as the same reaction.
    /// </summary>
    public static string? Normalize(string? emoji)
    {
        if (string.IsNullOrWhiteSpace(emoji))
        {
            return null;
        }

        return ByMatchKey.TryGetValue(NormalizeKey(emoji.Trim()), out var canonical) ? canonical : null;
    }

    public static bool IsAllowed(string? emoji) => Normalize(emoji) is not null;

    private static string NormalizeKey(string value) =>
        new([.. value.Where(c => c is not '️' and not '‍')]);
}