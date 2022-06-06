using System.Collections;
using System.Collections.Generic;
using SoftCraft.Enums;
using Volo.Abp.Domain.Entities.Auditing;

namespace SoftCraft.Entities;

public class Entity : FullAuditedEntity<long>
{
    public PrimaryKeyType PrimaryKeyType { get; set; }
    public string Name { get; set; }

    public string DisplayName { get; set; }

    // public bool AuditLog { get; set; }
    public bool IsFullAudited { get; set; }
    public TenantType TenantType { get; set; }

    public long ProjectId { get; set; }
    public virtual Project Project { get; set; }

    public virtual ICollection<Property> Properties { get; set; }
}