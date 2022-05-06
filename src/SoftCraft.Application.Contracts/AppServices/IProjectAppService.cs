using SoftCraft.AppServices.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SoftCraft.AppServices;

public interface IProjectAppService : ICrudAppService<
    ProjectDto,
    long,
    PagedAndSortedResultRequestDto,
    CreateProjectDto,
    UpdateProjectDto>
{
}