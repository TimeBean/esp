using System.Text.RegularExpressions;

namespace RepeatDataSender.Domain.Metrics;

/// <summary>
/// Immutable snapshot of every collected system value, keyed by the
/// placeholder name (without braces). Used to resolve metric templates.
/// </summary>
public sealed record MetricSnapshot
{
    public static readonly MetricSnapshot Empty = new(new Dictionary<string, string>());

    private static readonly Regex PlaceholderRegex = new(@"\{(\w+)\}");

    public IReadOnlyDictionary<string, string> Values { get; }

    public MetricSnapshot(IReadOnlyDictionary<string, string> values)
    {
        Values = values;
    }

    /// <summary>
    /// Replaces every <c>{name}</c> occurrence with the collected value.
    /// Unknown placeholders are left untouched so a typo stays visible.
    /// </summary>
    public string Resolve(string template)
    {
        return PlaceholderRegex.Replace(template, match =>
            Values.TryGetValue(match.Groups[1].Value, out var value) ? value : match.Value);
    }
}