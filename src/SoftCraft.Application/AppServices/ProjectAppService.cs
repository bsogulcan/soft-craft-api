using System.Collections.Generic;
using System.Threading.Channels;
using System.Threading.Tasks;
using DotNetCodeGenerator;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using ProjectManager;
using SoftCraft.AppServices.Dtos;
using SoftCraft.Entities;
using SoftCraft.Repositories;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Users;

namespace SoftCraft.AppServices;

public class ProjectAppService : CrudAppService<Project, ProjectDto, long, GetListInput,
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


    public override async Task<PagedResultDto<ProjectDto>> GetListAsync(GetListInput input)
    {
        using var dotNetCodeGeneratorChannel =
            GrpcChannel.ForAddress(_configuration["MicroServices:DotNetCodeGeneratorUrl"]);
        var client =
            new DotNetCodeGenerator.DotNetCodeGenerator.DotNetCodeGeneratorClient(dotNetCodeGeneratorChannel);

        var result = await client.CreateEntityAsync(new Entity()
        {
            Name = "Factory",
            Namespace = "CBIMes.Domain.Entities",
            Usings =
            {
                "Abp.Domain.Entities.Auditing;",
                "System.Collections.Generic;",
                "Abp.Domain.Entities;"
            },
            FullAudited = true,
            PrimaryKeyType = PrimaryKeyType.Int,
            TenantType = TenantType.None,
            Properties =
            {
                new Property()
                {
                    Name = "Name",
                    Type = "string",
                },
                new Property()
                {
                    Name = "Test",
                    Type = "int"
                }
            }
        });

        var projects = await Repository.GetListAsync(x => x.CreatorId == _currentUser.GetId());
        return new PagedResultDto<ProjectDto>()
        {
            Items = ObjectMapper.Map<List<Project>, List<ProjectDto>>(projects),
            TotalCount = projects.Count
        };
    }

    public override async Task<ProjectDto> CreateAsync(CreateProjectDto input)
    {
        var project = await base.CreateAsync(input);

        using var dotNetCodeGeneratorChannel =
            GrpcChannel.ForAddress(_configuration["MicroServices:ProjectManagerUrl"]);
        var client =
            new ProjectManager.ProjectManager.ProjectManagerClient(dotNetCodeGeneratorChannel);

        var result = await client.CreateAbpBoilerplateProjectAsync(new ProjectRequest()
        {
            Id = project.Id.ToString()
        });

        if (result != null)
        {
            return project;
        }

        throw new UserFriendlyException("Project cannot be created");
    }
}