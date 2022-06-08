using Volo.Abp.Application.Dtos;

namespace SoftCraft.AppServices.EntityValue.Dtos;

public class UpdateEnumerateValueInput:EntityDto<long>
{
    public long ProjectId { get; set; }

    public string Name { get; set; }
    
    public float EnumerateId { get; set; }
    
    public string DisplayName { get; set; }

    public string Value { get; set; }
}