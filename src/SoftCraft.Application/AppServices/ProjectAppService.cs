using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using SoftCraft.AppServices.Project.Dtos;
using SoftCraft.Repositories;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Users;

namespace SoftCraft.AppServices;

public class ProjectAppService : CrudAppService<Entities.Project, ProjectPartOutput, long, GetListInput,
    CreateProjectDto, UpdateProjectDto>, IProjectAppService
{
    private readonly ICurrentUser _currentUser;
    private readonly IConfiguration _configuration;

    public ProjectAppService(IProjectRepository projectRepository,
        ICurrentUser currentUser,
        IConfiguration configuration
    ) : base(projectRepository)
    {
        _currentUser = currentUser;
        _configuration = configuration;
    }

    public override Task<ProjectPartOutput> UpdateAsync(long id, UpdateProjectDto input)
    {
        return base.UpdateAsync(id, input);
    }

    public override async Task<PagedResultDto<ProjectPartOutput>> GetListAsync(GetListInput input)
    {
        return await base.GetListAsync(input);
        // var projects = await Repository.GetListAsync(x => x.CreatorId == _currentUser.GetId());
        // return new PagedResultDto<ProjectDto>()
        // {
        //     Items = ObjectMapper.Map<List<Project>, List<ProjectDto>>(projects),
        //     TotalCount = projects.Count
        // };
    }

    public override async Task<ProjectPartOutput> CreateAsync(CreateProjectDto input)
    {
        return await base.CreateAsync(input);
        // using var dotNetCodeGeneratorChannel =
        //     GrpcChannel.ForAddress(_configuration["MicroServices:ProjectManagerUrl"]);
        // var client =
        //     new ProjectManager.ProjectManager.ProjectManagerClient(dotNetCodeGeneratorChannel);
        //
        // var result = await client.CreateAbpBoilerplateProjectAsync(new ProjectRequest()
        // {
        //     Id = project.Id.ToString(),
        //     Name = project.NormalizedName
        // });
    }
}