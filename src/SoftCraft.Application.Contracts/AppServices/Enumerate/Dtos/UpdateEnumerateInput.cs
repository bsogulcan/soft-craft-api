using Volo.Abp.Application.Dtos;

namespace SoftCraft.AppServices.Enumerate.Dtos;

public class UpdateEnumerateInput:EntityDto<long>
{
    public long ProjectId { get; set; }

    public string Name { get; set; }
    
}