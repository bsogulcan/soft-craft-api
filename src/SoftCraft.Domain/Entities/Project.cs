using Volo.Abp.Domain.Entities.Auditing;

namespace SoftCraft.Entities;

public class Project : FullAuditedEntity<long>
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string NormalizedName { get; set; }
    public int Port { get; set; }

    public byte[] RowVersion { get; set; }
}