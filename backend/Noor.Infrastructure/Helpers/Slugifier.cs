using System.Text.RegularExpressions;

namespace Noor.Infrastructure.Helpers;

public static class Slugifier
{
    public static string Slugify(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;

        var normalized = value.Trim().ToLowerInvariant();
        normalized = normalized
            .Replace("&", " and ")
            .Replace("'", "")
            .Replace("’", "");

        normalized = Regex.Replace(normalized, @"[^a-z0-9]+", "-");
        normalized = Regex.Replace(normalized, "-{2,}", "-").Trim('-');

        return normalized.Length > 160 ? normalized[..160].Trim('-') : normalized;
    }

    public static string UniqueSlug(string value, Func<string, bool> exists, string fallback = "item")
    {
        var baseSlug = Slugify(value);
        if (string.IsNullOrWhiteSpace(baseSlug)) baseSlug = fallback;
        var slug = baseSlug;
        var i = 2;
        while (exists(slug))
        {
            slug = $"{baseSlug}-{i++}";
        }
        return slug;
    }
}

public static class Money
{
    public static decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
}
