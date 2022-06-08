using System.Collections.Generic;
using Volo.Abp.Domain.Entities.Auditing;

namespace SoftCraft.Entities;

public class Navigation : FullAuditedEntity<long>
{
    public Navigation()
    {
        Navigations = new HashSet<Navigation>();
    }

    public string Caption { get; set; }
    public int Index { get; set; }
    public bool Visible { get; set; }
    public long? ParentNavigationId { get; set; }
    public virtual Navigation ParentNavigation { get; set; }
    public string Icon { get; set; }

    public long? ProjectId { get; set; }
    public virtual Project Project { get; set; }
    public long? EntityId { get; set; }
    public virtual Entity Entity { get; set; }
    public virtual ICollection<Navigation> Navigations { get; set; }
}