using System.Reflection;

namespace CommunityLink.Shared;

public sealed record PropertyDiff(string PropertyName, object? OldValue, object? NewValue);

public static class ChangeRecord
{
    public static IReadOnlyList<PropertyDiff> ComputeDiff<T>(T original, T updated) where T : class
    {
        var diffs = new List<PropertyDiff>();
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var prop in properties)
        {
            if (!prop.CanRead) continue;
            var oldVal = prop.GetValue(original);
            var newVal = prop.GetValue(updated);

            if (!Equals(oldVal, newVal))
            {
                diffs.Add(new PropertyDiff(prop.Name, oldVal, newVal));
            }
        }

        return diffs;
    }
}