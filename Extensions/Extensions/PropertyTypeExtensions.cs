using SoftCraft.Enums;

namespace Extensions;

public static class PropertyTypeExtensions
{
    public static string GetNormalizedPropertyType(PropertyType? type)
    {
        return type switch
        {
            PropertyType.String => "string",
            PropertyType.Int => "int",
            PropertyType.Bool => "bool",
            PropertyType.Long => "long",
            PropertyType.Float => "float",
            PropertyType.Double => "double",
            PropertyType.Decimal => "decimal",
            PropertyType.DateTime => "DateTime",
            _ => "UndefinedType"
        };
    }

    public static string ConvertPrimaryKeyToTypeScriptDataType(int type)
    {
        var primaryKeyType = (PrimaryKeyType) type;

        return primaryKeyType switch
        {
            PrimaryKeyType.Int => "number",
            PrimaryKeyType.Long => "number",
            PrimaryKeyType.Guid => "string",
            _ => "UndefinedType"
        };
    }
}