using Volo.Abp.Application.Dtos;

namespace SoftCraft.AppServices.Enumerate.Dtos;

public class EnumeratePartOutput:EntityDto<long>
{
    public long ProjectId { get; set; }
    
    public string Name { get; set; }

    public string DisplayName { get; set; }
}