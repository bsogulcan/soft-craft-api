using System.Collections.Generic;
using SoftCraft.AppServices.Project.Dtos;
using SoftCraft.AppServices.Property.Dtos;
using SoftCraft.Enums;
using Volo.Abp.Application.Dtos;

namespace SoftCraft.AppServices.Entity.Dtos;

public class EntityFullOutput : EntityDto<long>
{
    public long ProjectId { get; set; }
    public ProjectPartOutput Project { get; set; }
    public PrimaryKeyType PrimaryKeyType { get; set; }
    public string Name { get; set; }
    public string DisplayName { get; set; }
    public bool IsFullAudited { get; set; }
    public TenantType TenantType { get; set; }
    public List<PropertyFullOutput> Properties { get; set; }
}