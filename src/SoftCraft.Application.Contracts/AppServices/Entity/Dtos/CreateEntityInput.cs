using System.Collections.Generic;
using SoftCraft.AppServices.Property.Dtos;
using SoftCraft.Enums;

namespace SoftCraft.AppServices.Entity.Dtos;

public class CreateEntityInput
{
    public long ProjectId { get; set; }
    public PrimaryKeyType PrimaryKeyType { get; set; }
    public string Name { get; set; }
    public string DisplayName { get; set; }
    public bool IsFullAudited { get; set; }
    public TenantType TenantType { get; set; }
}