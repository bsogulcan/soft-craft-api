﻿using JetBrains.Annotations;
using SoftCraft.Enums;
using Volo.Abp.Domain.Entities.Auditing;

namespace SoftCraft.Entities;

public class Property : FullAuditedEntity<long>
{
    public string Name { get; set; }
    public string DisplayName { get; set; }
    public bool IsNullable { get; set; }
    public PropertyType? Type { get; set; }
    public bool IsRelationalProperty { get; set; }
    public long? RelationalEntityId { get; set; }

    [CanBeNull] public virtual Entity RelationalEntity { get; set; }

    public RelationType? RelationType { get; set; }
    public bool IsEnumProperty { get; set; }
    public long? EnumerateId { get; set; }
    public virtual Enumerate Enumerate { get; set; }

    public string ToolTip { get; set; }
    public bool Required { get; set; }
    public bool Indexed { get; set; }
    public int? MaxLength { get; set; }
    public bool Unique { get; set; }
    public long EntityId { get; set; }
    public virtual Entity Entity { get; set; }
    public bool DisplayOnList { get; set; }
    public bool FilterOnList { get; set; }
    public long? LinkedPropertyId { get; set; }
    public virtual Property LinkedProperty { get; set; }
}