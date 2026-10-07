namespace CommunityLink.Shared.Utils;

public static class FileSizeFormatter
{
    private static readonly string[] Units = ["B", "KB", "MB", "GB", "TB"];

    public static string FormatFileSize(long? bytes)
    {
        if (!bytes.HasValue || bytes.Value <= 0)
        {
            return "0 B";
        }

        double size = bytes.Value;
        int unitIndex = 0;

        while (size >= 1024 && unitIndex < Units.Length - 1)
        {
            size /= 1024;
            unitIndex++;
        }

        return unitIndex == 0
            ? $"{bytes.Value} B"
            : $"{size:0.#} {Units[unitIndex]}";
    }
}
