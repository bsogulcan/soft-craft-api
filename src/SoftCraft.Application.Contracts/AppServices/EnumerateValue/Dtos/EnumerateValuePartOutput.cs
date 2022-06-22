using SoftCraft.AppServices.Enumerate.Dtos;
using Volo.Abp.Application.Dtos;

namespace SoftCraft.AppServices.EnumerateValue.Dtos;

public class EnumerateValuePartOutput : EntityDto<long>
{
    public float EnumerateId { get; set; }

    public string Name { get; set; }

    public string DisplayName { get; set; }

    public int Value { get; set; }
    public EnumeratePartOutput Enumerate { get; set; }
}