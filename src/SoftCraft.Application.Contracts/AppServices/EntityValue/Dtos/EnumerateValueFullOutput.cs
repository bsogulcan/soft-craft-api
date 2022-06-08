using SoftCraft.AppServices.Enumerate.Dtos;
using Volo.Abp.Application.Dtos;

namespace SoftCraft.AppServices.EntityValue.Dtos;

public class EnumerateValueFullOutput:EntityDto<long>
{
    public float EnumerateId { get; set; }

    public string Name { get; set; }

    public string DisplayName { get; set; }

    public string Value { get; set; }

    public EnumeratePartOutput EnumeratePartOutput { get; set; }
}