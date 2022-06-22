using System.Collections.Generic;
using SoftCraft.AppServices.Entity.Dtos;
using SoftCraft.AppServices.Project.Dtos;
using Volo.Abp.Application.Dtos;

namespace SoftCraft.AppServices.Navigations.Dtos;

public class NavigationFullOutput : EntityDto<long>
{
    public string Caption { get; set; }
    public int Index { get; set; }
    public bool Visible { get; set; }
    public long? ParentNavigationId { get; set; }
    public NavigationPartOutput ParentNavigation { get; set; }
    public string Icon { get; set; }
    public long? ProjectId { get; set; }
    public ProjectPartOutput Project { get; set; }
    public long? EntityId { get; set; }
    public EntityPartOutput Entity { get; set; }
    public List<NavigationFullOutput> Navigations { get; set; }
}