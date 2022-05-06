using Volo.Abp.Application.Dtos;

namespace SoftCraft.AppServices.Dtos;

public class ProjectDto : EntityDto<long>
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string NormalizedName { get; set; }
    public int Port { get; set; }
    public byte[] RowVersion { get; set; }
}