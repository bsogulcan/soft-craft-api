using SoftCraft.Enums;
using Volo.Abp.Domain.Entities.Auditing;

namespace SoftCraft.Entities;

public class Property : FullAuditedEntity<long>
{
    public string Name { get; set; }
    public bool IsNullable { get; set; }
    public PropertyType Type { get; set; }
    public bool IsRelationalProperty { get; set; }
    public int? RelationalEntityId { get; set; }
    public virtual Entity RelationalEntity { get; set; }
    //public bool IsEnumProperty { get; set; }
}