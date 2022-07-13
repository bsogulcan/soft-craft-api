using System;
using System.Threading.Tasks;
using DotNetCodeGenerator;
using Microsoft.Extensions.Configuration;
using ProjectManager;
using SoftCraft.AppServices.Project.Dtos;
using SoftCraft.Enums;
using SoftCraft.Manager.MicroServiceManager.DotNetCodeGeneratorServiceManager;
using SoftCraft.Manager.MicroServiceManager.ProjectManagerServiceManager;
using SoftCraft.Manager.MicroServiceManager.TypeScriptCodeGeneratorServiceManager;
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
    private readonly IProjectManagerServiceManager _projectManagerServiceManager;
    private readonly IDotNetCodeGeneratorServiceManager _dotNetCodeGeneratorServiceManager;
    private readonly ITypeScriptCodeGeneratorServiceManager _typeScriptCodeGeneratorServiceManager;

    public ProjectAppService(IProjectRepository projectRepository,
        ICurrentUser currentUser,
        IConfiguration configuration,
        IProjectManagerServiceManager projectManagerServiceManager,
        IDotNetCodeGeneratorServiceManager dotNetCodeGeneratorServiceManager,
        ITypeScriptCodeGeneratorServiceManager typeScriptCodeGeneratorServiceManager
    ) : base(projectRepository)
    {
        _currentUser = currentUser;
        _configuration = configuration;
        _projectManagerServiceManager = projectManagerServiceManager;
        _dotNetCodeGeneratorServiceManager = dotNetCodeGeneratorServiceManager;
        _typeScriptCodeGeneratorServiceManager = typeScriptCodeGeneratorServiceManager;
    }

    public override Task<ProjectPartOutput> UpdateAsync(long id, UpdateProjectDto input)
    {
        return base.UpdateAsync(id, input);
    }

    public async Task Generate(GenerateProjectInput input)
    {
        try
        {
            var project = await Repository.GetAsync(input.Id);

            var result = await _projectManagerServiceManager.CreateAbpBoilerplateProjectAsync(project.Id,
                project.UniqueName, project.LogType, project.MultiTenant);

            foreach (var entity in project.Entities)
            {
                var entityResult =
                    await _dotNetCodeGeneratorServiceManager.CreateEntityAsync(entity);
                var createEntityResult = await _projectManagerServiceManager.AddEntityToExistingProjectAsync(
                    new AddEntityRequest()
                    {
                        Id = project.Id.ToString(),
                        EntityName = entity.Name,
                        ProjectName = project.UniqueName,
                        Stringified = entityResult.Stringified
                    });

                var createEntityConfigurationResult =
                    await _dotNetCodeGeneratorServiceManager.CreateConfigurationAsync(entity);
                var createConfigurationResult =
                    await _projectManagerServiceManager.AddConfigurationToExistingProjectAsync(
                        new AddEntityRequest()
                        {
                            Id = project.Id.ToString(),
                            EntityName = entity.Name,
                            ProjectName = project.UniqueName,
                            Stringified = createEntityConfigurationResult.Stringified
                        });


                var repositoryInterfaceResult =
                    await _dotNetCodeGeneratorServiceManager.CreateRepositoryInterfaceAsync(entity);
                var repositoryResult = await _dotNetCodeGeneratorServiceManager.CreateRepositoryAsync(entity);

                var createRepositoryResult = await _projectManagerServiceManager.AddRepositoryToExistingProjectAsync(
                    new AddRepositoryRequest()
                    {
                        Id = project.Id.ToString(),
                        EntityName = entity.Name,
                        ProjectName = project.UniqueName,
                        StringifiedRepositoryInterface = repositoryInterfaceResult.Stringified,
                        StringifiedRepository = repositoryResult.Stringified
                    });

                var createDtosResult = await _dotNetCodeGeneratorServiceManager.CreateDtosAsync(entity);
                var addDtosToExistingProjectReply = await _projectManagerServiceManager.AddDtosToExistingProjectAsync(
                    new AddDtosRequest()
                    {
                        Id = project.Id.ToString(),
                        EntityName = entity.Name,
                        ProjectName = project.UniqueName,
                        CreateInputStringify = createDtosResult.CreateInputStringify,
                        UpdateInputStringify = createDtosResult.UpdateInputStringify,
                        GetInputStringify = createDtosResult.GetInputStringify,
                        DeleteInputStringify = createDtosResult.DeleteInputStringify,
                        FullOutputStringify = createDtosResult.FullOutputStringify,
                        PartOutputStringify = createDtosResult.PartOutputStringify,
                        DtosToDomainStringify = createDtosResult.DtosToDomainStringify,
                        DomainToDtosStringify = createDtosResult.DomainToDtosStringify
                    });

                var createAppServiceResult = await _dotNetCodeGeneratorServiceManager.CreateAppServiceAsync(
                    new AppServiceRequest()
                    {
                        EntityName = entity.Name,
                        ProjectName = project.UniqueName
                    });


                var addAppServiceResult = await _projectManagerServiceManager.AddAppServiceToExistingProjectAsync(
                    new AddAppServiceRequest()
                    {
                        Id = project.Id.ToString(),
                        EntityName = entity.Name,
                        ProjectName = project.UniqueName,
                        AppServiceStringify = createAppServiceResult.AppServiceStringify,
                        AppServiceInterfaceStringify = createAppServiceResult.AppServiceInterfaceStringify,
                        PermissionNames = createAppServiceResult.PermissionNames,
                        AuthorizationProviders = createAppServiceResult.AuthorizationProviders
                    });

                var typeScriptDtosResult = await _typeScriptCodeGeneratorServiceManager.CreateDtosAsync(entity);
                var addDtpRequestInput = new AddDtosRequest
                {
                    Id = project.Id.ToString(),
                    EntityName = entity.Name,
                    FullOutputStringify = typeScriptDtosResult.FullOutputStringify,
                    PartOutputStringify = typeScriptDtosResult.PartOutputStringify,
                    CreateInputStringify = typeScriptDtosResult.CreateInputStringify,
                    UpdateInputStringify = typeScriptDtosResult.UpdateInputStringify,
                    GetInputStringify = typeScriptDtosResult.GetInputStringify,
                    DeleteInputStringify = typeScriptDtosResult.DeleteInputStringify
                };

                var addTypeScriptDtosToExistingProjectResult = await _projectManagerServiceManager
                    .AddTypeScriptDtosToExistingProjectAsync(addDtpRequestInput);

                var createTypeScriptServiceResult =
                    await _typeScriptCodeGeneratorServiceManager.CreateServiceAsync(entity);
                var addTypeScrtipServiceInput = new AddTypeScriptServiceRequest()
                {
                    EntityName = entity.Name,
                    Id = project.Id.ToString(),
                    ServiceStringify = createTypeScriptServiceResult.Stringify
                };
                var addTypeScriptServiceResult =
                    await _projectManagerServiceManager.AddTypeScriptServiceToExistingProjectAsync(
                        addTypeScrtipServiceInput);
            }

            foreach (var enumerate in project.Enumerates)
            {
                var createEnumResult = await _dotNetCodeGeneratorServiceManager.CreateEnumAsync(enumerate);
                var createEntityResult = await _projectManagerServiceManager.AddEnumToExistingProjectAsync(
                    new AddEnumRequest()
                    {
                        Id = project.Id.ToString(),
                        EnumName = enumerate.Name,
                        ProjectName = project.UniqueName,
                        Stringified = createEnumResult.Stringified
                    });
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
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

    private static string GetNormalizedPropertyType(PropertyType? type)
    {
        return type switch
        {
            PropertyType.String => "string",
            PropertyType.Int => "int",
            PropertyType.Bool => "bool",
            PropertyType.Long => "long",
            PropertyType.Float => "float",
            PropertyType.Double => "double",
            PropertyType.Decimal => "decimal",
            PropertyType.DateTime => "DateTime",
            _ => "UndefinedType"
        };
    }
}