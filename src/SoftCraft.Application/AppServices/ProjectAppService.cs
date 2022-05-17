using System.Collections.Generic;
using System.Threading.Channels;
using System.Threading.Tasks;
using DotNetCodeGenerator;
using Grpc.Net.Client;
using SoftCraft.AppServices.Dtos;
using SoftCraft.Entities;
using SoftCraft.Repositories;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Users;

namespace SoftCraft.AppServices;

public class ProjectAppService : CrudAppService<Project, ProjectDto, long, GetListInput,
    CreateProjectDto, UpdateProjectDto>, IProjectAppService
{
    private readonly ICurrentUser _currentUser;

    public ProjectAppService(IProjectRepository projectRepository,
        ICurrentUser currentUser
    ) : base(projectRepository)
    {
        _currentUser = currentUser;
    }


    public override async Task<PagedResultDto<ProjectDto>> GetListAsync(GetListInput input)
    {
        using var dotNetCodeGeneratorChannel = GrpcChannel.ForAddress("http://localhost:5252");
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
}