using SoftCraft.AppServices.Project.Dtos;
using Volo.Abp.Application.Services;

namespace SoftCraft.AppServices;

public interface IProjectAppService : ICrudAppService<
    ProjectPartOutput,
    long,
    GetListInput,
    CreateProjectDto,
    UpdateProjectDto>
{
}