using System.Text.RegularExpressions;
using Humanizer;

namespace Extensions;

public static class StringExtensions
{
    public static string ToTitle(this string input)
    {
        return input.Humanize(LetterCasing.Title);
    }

    public static string ToFlatCase(this string input)
    {
        return input.ToLower();
    }

    public static string ToUpperCase(this string input)
    {
        return input.ToUpper();
    }

    public static string ToCamelCase(string input)
    {
        var x = input.Replace("_", "");
        if (x.Length == 0) return input;
        x = Regex.Replace(x, "([A-Z])([A-Z]+)($|[A-Z])",
            m => m.Groups[1].Value + m.Groups[2].Value.ToLower() + m.Groups[3].Value);
        return char.ToLower(x[0]) + x.Substring(1);
    }

    public static string ToPascalCase(this string input)
    {
        return string.Join("", input.Split('_')
            .Select(w => w.Trim())
            .Where(w => w.Length > 0)
            .Select(w => w.Substring(0, 1).ToUpper() + w.Substring(1).ToLower()));
    }

    public static string ToSnakeCase(string input)
    {
        return input == null
            ? input
            : string.Join("_", string.Concat(string.Join("_", input.Split(new char[] { },
                        StringSplitOptions.RemoveEmptyEntries))
                    .Select(c => char.IsUpper(c)
                        ? $"_{c}".ToLower()
                        : $"{c}"))
                .Split(new[] {'_'}, StringSplitOptions.RemoveEmptyEntries));
    }
}