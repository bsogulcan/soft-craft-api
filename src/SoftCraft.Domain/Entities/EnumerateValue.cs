using Volo.Abp.Domain.Entities.Auditing;

namespace SoftCraft.Entities;

public class EnumerateValue : FullAuditedEntity<long>
{
    public string Name { get; set; }

    public string DisplayName { get; set; }

    public int Value { get; set; }

    public long EnumerateId { get; set; }

    public virtual Enumerate Enumerate { get; set; }
}