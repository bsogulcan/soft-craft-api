using SoftCraft.AppServices.Entity.Dtos;
using SoftCraft.Enums;
using Volo.Abp.Application.Dtos;

namespace SoftCraft.AppServices.Property.Dtos;

public class PropertyPartOutput : EntityDto<long>
{
    public string Name { get; set; }
    public string DisplayName { get; set; }
    public bool IsNullable { get; set; }
    public PropertyType Type { get; set; }
    public bool IsRelationalProperty { get; set; }
    public int? RelationalEntityId { get; set; }
}