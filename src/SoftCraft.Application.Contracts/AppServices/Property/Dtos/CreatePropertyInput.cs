using SoftCraft.Enums;

namespace SoftCraft.AppServices.Property.Dtos;

public class CreatePropertyInput
{
    public string Name { get; set; }
    public string DisplayName { get; set; }
    public bool IsNullable { get; set; }
    public PropertyType Type { get; set; }
    public bool IsRelationalProperty { get; set; }
    public long? RelationalEntityId { get; set; }
    public RelationType? RelationType { get; set; }
    public long EntityId { get; set; }
    public string ToolTip { get; set; }
    public bool Required { get; set; }
    public bool Indexed { get; set; }
    public int MaxLength { get; set; }
    public bool Unique { get; set; }
}