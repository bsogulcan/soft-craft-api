using Volo.Abp.Application.Dtos;

namespace SoftCraft.AppServices.Dtos;

public class UpdateProjectDto : EntityDto<long>
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string NormalizedName { get; set; }
    public int Port { get; set; }
}