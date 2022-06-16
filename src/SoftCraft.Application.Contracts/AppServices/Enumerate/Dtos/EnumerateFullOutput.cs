using System.Collections.Generic;
using SoftCraft.AppServices.EnumerateValue.Dtos;
using SoftCraft.AppServices.Project.Dtos;
using Volo.Abp.Application.Dtos;

namespace SoftCraft.AppServices.Enumerate.Dtos;

public class EnumerateFullOutput : EntityDto<long>
{
    public long ProjectId { get; set; }

    public ProjectPartOutput Project { get; set; }

    public string Name { get; set; }

    public string DisplayName { get; set; }

    public List<EnumerateValuePartOutput> EnumerateValues { get; set; }
}