using System.Collections.Generic;
using Volo.Abp.Domain.Entities.Auditing;

namespace SoftCraft.Entities;

public class Enumerate:FullAuditedEntity<long>
{
    public string Name { get; set; }

    public string DisplayName { get; set; }

    public long ProjectId { get; set; }

    public virtual Project Project { get; set; }

    public virtual ICollection<EnumerateValue> EnumerateValues { get; set; }
    
}