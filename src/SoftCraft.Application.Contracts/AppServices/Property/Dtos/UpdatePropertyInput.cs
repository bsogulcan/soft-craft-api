using SoftCraft.Enums;
using Volo.Abp.Application.Dtos;

namespace SoftCraft.AppServices.Property.Dtos;

public class UpdatePropertyInput : EntityDto<long>
{
    public string Name { get; set; }
    public string DisplayName { get; set; }
    public bool IsNullable { get; set; }
    public PropertyType? Type { get; set; }
    public bool IsRelationalProperty { get; set; }
    public long? RelationalEntityId { get; set; }
    public RelationType? RelationType { get; set; }
    public long EntityId { get; set; }
    public string ToolTip { get; set; }
    public bool Required { get; set; }
    public bool Indexed { get; set; }
    public int MaxLength { get; set; }
    public bool Unique { get; set; }
    public bool IsEnumProperty { get; set; }
    public long? EnumerateId { get; set; }
    public bool DisplayOnList { get; set; }
    public bool FilterOnList { get; set; }
}