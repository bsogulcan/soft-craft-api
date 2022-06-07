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
}