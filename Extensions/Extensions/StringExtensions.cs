using System.Text.RegularExpressions;
using Humanizer;
using SoftCraft.Enums;

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

    public static string ToCamelCase(this string input)
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

    public static string ToTypeScriptDataType(this string referenceType, bool nullable = false,
        bool isRelationalProperty = false, int relationType = 0, bool isEnumerateProperty = false)
    {
        var lowerCaseReferenceType = referenceType.Trim().ToLower();

        string type = lowerCaseReferenceType switch
        {
            "string" or "guid" => "string",
            "int" or "long" or "float" or "double" or "decimal" => "number",
            "bool" => "boolean",
            "datetime" => "Date",
            _ => string.Empty
        };

        if (string.IsNullOrEmpty(type) && isRelationalProperty)
        {
            if ((RelationType) relationType == RelationType.OneToOne)
            {
                type = referenceType + "PartOutput";
            }
            else
            {
                type = $"Array<{referenceType}PartOutput>";
            }
        }

        if (isEnumerateProperty)
        {
            type = referenceType;
        }

        if (nullable)
        {
            type += " | undefined";
        }

        return type + ";";
    }

    public static string ToTypeScriptDataGridColumnType(this string referenceType)
    {
        var lowerCaseReferenceType = referenceType.Trim().ToLower();

        var type = lowerCaseReferenceType switch
        {
            "string" => "text",
            "int" or "long" or "float" or "double" or "decimal" => "numeric",
            "boolean" => "boolean",
            "datetime" => "date",
            _ => string.Empty
        };

        return type;
    }
}