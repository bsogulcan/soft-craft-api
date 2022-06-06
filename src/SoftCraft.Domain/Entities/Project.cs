using System.Collections.Generic;
using JetBrains.Annotations;
using SoftCraft.Enums;
using Volo.Abp.Domain.Entities.Auditing;

namespace SoftCraft.Entities;

public class Project : FullAuditedEntity<long>
{
    public string Name { get; set; }

    public string UniqueName { get; set; }

    //public AuthType AuthType  { get; set; }
    public bool MultiTenant { get; set; }

    [CanBeNull] public string WebAddress { get; set; }
    public int? Port { get; set; }

    //public string Logo { get; set; }
    public LogType LogType { get; set; }

    public virtual ICollection<Entity> Entities { get; set; }
    public byte[] RowVersion { get; set; }
}