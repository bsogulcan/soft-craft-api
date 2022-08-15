using SoftCraft.AppServices.Project.Dtos;
using SoftCraft.Enums;
using Volo.Abp.Application.Dtos;

namespace SoftCraft.AppServices.Entity.Dtos;

public class EntityPartOutput : EntityDto<long>
{
    public long ProjectId { get; set; }
    public ProjectPartOutput Project { get; set; }
    public PrimaryKeyType PrimaryKeyType { get; set; }
    public string Name { get; set; }
    public string DisplayName { get; set; }
    public bool IsFullAudited { get; set; }
    public TenantType TenantType { get; set; }
    public bool IsDefaultAbpEntity { get; set; }
}