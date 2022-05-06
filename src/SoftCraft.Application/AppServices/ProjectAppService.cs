using SoftCraft.AppServices.Dtos;
using SoftCraft.Entities;
using SoftCraft.Repositories;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SoftCraft.AppServices;

public class ProjectAppService : CrudAppService<Project, ProjectDto, long, PagedAndSortedResultRequestDto,
    CreateProjectDto, UpdateProjectDto>
{
    public ProjectAppService(IProjectRepository projectRepository) : base(projectRepository)
    {
    }
}