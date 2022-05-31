using SoftCraft.AppServices.Dtos;
using SoftCraft.Enums;
using Volo.Abp.Application.Dtos;

namespace SoftCraft.AppServices.Entity.Dtos;

public class EntityDto : EntityDto<long>
{
    public long ProjectId { get; set; }
    public ProjectDto Project { get; set; }
    public PrimaryKeyType PrimaryKeyType { get; set; }
    public string Name { get; set; }
    public bool IsFullAudited { get; set; }
    public TenantType TenantType { get; set; }
}