using Volo.Abp.Application.Dtos;

namespace SoftCraft.AppServices.Navigations.Dtos;

public class UpdateNavigationInput : EntityDto<long>
{
    public string Caption { get; set; }
    public int Index { get; set; }
    public bool Visible { get; set; }
    public long? ParentNavigationId { get; set; }
    public string Icon { get; set; }
    public long? ProjectId { get; set; }
    public long? EntityId { get; set; }
}