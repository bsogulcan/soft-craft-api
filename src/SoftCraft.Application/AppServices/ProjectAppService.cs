using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using DotNetCodeGenerator;
using Extensions;
using Microsoft.AspNetCore.Mvc;
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
using Volo.Abp.Identity.Settings;
using Volo.Abp.Users;
using PrimaryKeyType = SoftCraft.Enums.PrimaryKeyType;
using RelationType = TypeScriptCodeGenerator.RelationType;

namespace SoftCraft.AppServices;

public class ProjectAppService : CrudAppService<Entities.Project, ProjectPartOutput, long, GetListInput,
    CreateProjectDto, UpdateProjectDto>, IProjectAppService
{
    private readonly ICurrentUser _currentUser;
    private readonly IConfiguration _configuration;
    private readonly IProjectManagerServiceManager _projectManagerServiceManager;
    private readonly IDotNetCodeGeneratorServiceManager _dotNetCodeGeneratorServiceManager;
    private readonly ITypeScriptCodeGeneratorServiceManager _typeScriptCodeGeneratorServiceManager;
    private readonly INavigationRepository _navigationRepository;
    private readonly IEntityRepository _entityRepository;

    public ProjectAppService(IProjectRepository projectRepository,
        ICurrentUser currentUser,
        IConfiguration configuration,
        IProjectManagerServiceManager projectManagerServiceManager,
        IDotNetCodeGeneratorServiceManager dotNetCodeGeneratorServiceManager,
        ITypeScriptCodeGeneratorServiceManager typeScriptCodeGeneratorServiceManager,
        INavigationRepository navigationRepository,
        IEntityRepository entityRepository
    ) : base(projectRepository)
    {
        _currentUser = currentUser;
        _configuration = configuration;
        _projectManagerServiceManager = projectManagerServiceManager;
        _dotNetCodeGeneratorServiceManager = dotNetCodeGeneratorServiceManager;
        _typeScriptCodeGeneratorServiceManager = typeScriptCodeGeneratorServiceManager;
        _navigationRepository = navigationRepository;
        _entityRepository = entityRepository;
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
                project.UniqueName, project.LogType, project.MultiTenant, project.Name);

            foreach (var entity in project.Entities.Where(x => x.IsDefaultAbpEntity == false))
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


                var createAppServiceInput = new AppServiceRequest()
                {
                    EntityName = entity.Name,
                    ProjectName = entity.Project.UniqueName,
                    EntityType = PropertyTypeExtensions.ConvertPrimaryKeyToDotNetDataType(entity.PrimaryKeyType),
                    Properties = { }
                };

                foreach (var relationalProperty in entity.Properties.Where(x =>
                             x.IsRelationalProperty && x.RelationType == Enums.RelationType.OneToOne))
                {
                    createAppServiceInput.Properties.Add(new DotNetCodeGenerator.Property()
                    {
                        IsRelationalProperty = relationalProperty.IsRelationalProperty,
                        RelationType = (DotNetCodeGenerator.RelationType) relationalProperty.RelationType,
                        Name = relationalProperty.Name,
                        Type = PropertyTypeExtensions.ConvertPrimaryKeyToDotNetDataType(relationalProperty.Entity
                            .PrimaryKeyType),
                    });
                }

                var createAppServiceResult =
                    await _dotNetCodeGeneratorServiceManager.CreateAppServiceAsync(createAppServiceInput);


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

                var createComponentResult = await _typeScriptCodeGeneratorServiceManager.CreateComponentsAsync(entity);
                var componentResult = new ProjectManager.ComponentResult()
                {
                    ProjectId = project.Id,
                    EntityName = entity.Name,
                    ListComponent = new ComponentResultEto()
                    {
                        ComponentCssStringify = createComponentResult.ListComponent.ComponentCssStringify,
                        ComponentHtmlStringify = createComponentResult.ListComponent.ComponentHtmlStringify,
                        ComponentTsStringify = createComponentResult.ListComponent.ComponentTsStringify
                    },
                    CreateComponent = new ComponentResultEto()
                    {
                        ComponentCssStringify = createComponentResult.CreateComponent.ComponentCssStringify,
                        ComponentHtmlStringify = createComponentResult.CreateComponent.ComponentHtmlStringify,
                        ComponentTsStringify = createComponentResult.CreateComponent.ComponentTsStringify
                    },
                    EditComponent = new ComponentResultEto()
                    {
                        ComponentCssStringify = createComponentResult.EditComponent.ComponentCssStringify,
                        ComponentHtmlStringify = createComponentResult.EditComponent.ComponentHtmlStringify,
                        ComponentTsStringify = createComponentResult.EditComponent.ComponentTsStringify
                    },
                };

                var addComponentResult =
                    await _projectManagerServiceManager.AddTypeScriptComponentsToExistingProjectAsync(componentResult);
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

                var createTypeScriptEnumResult =
                    await _typeScriptCodeGeneratorServiceManager.CreateEnumAsync(enumerate);
                await _projectManagerServiceManager.AddTypeScriptEnumToExistingProjectAsync(
                    new AddEnumRequest()
                    {
                        Id = project.Id.ToString(),
                        EnumName = enumerate.Name,
                        ProjectName = project.UniqueName,
                        Stringified = createTypeScriptEnumResult.Stringify
                    });
            }

            var navigations = await _navigationRepository.GetListAsync(x => x.ProjectId == project.Id);
            var tsNavigationResult = await _typeScriptCodeGeneratorServiceManager.CreateNavigationItems(navigations);
            await _projectManagerServiceManager.AddNavigationToExistingProjectAsync(
                new AddStringToExistingProject()
                {
                    ProjectId = project.Id,
                    Stringify = tsNavigationResult.Stringify
                });

            await _projectManagerServiceManager.GetProjectZipFile(project);
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
        var projectDto = await base.CreateAsync(input);

        List<Entities.Property> properties = new List<Entities.Property>();

        properties.Add(new Entities.Property
        {
            Name = "UserName",
            DisplayName = "UserName",
            DisplayOnList = true,
            FilterOnList = true,
            Type = PropertyType.String,
            MaxLength = 256
        });
        properties.Add(new Entities.Property
        {
            Name = "Name",
            DisplayName = "Name",
            DisplayOnList = true,
            FilterOnList = true,
            Type = PropertyType.String,
            MaxLength = 64
        });
        properties.Add(new Entities.Property
        {
            Name = "Surname",
            DisplayName = "Surname",
            DisplayOnList = true,
            FilterOnList = true,
            Type = PropertyType.String,
            MaxLength = 64
        });

        var defaultUserEntity = new Entities.Entity()
        {
            IsDefaultAbpEntity = true,
            Name = "User",
            DisplayName = "User",
            ProjectId = projectDto.Id,
            PrimaryKeyType = PrimaryKeyType.Long,
            Properties = properties
        };

        await _entityRepository.InsertAsync(defaultUserEntity);

        properties = new List<Entities.Property>();

        properties.Add(new Entities.Property
        {
            Name = "Name",
            DisplayName = "Name",
            DisplayOnList = true,
            FilterOnList = true,
            Type = PropertyType.String,
            MaxLength = 64
        });

        var defaultRoleEntity = new Entities.Entity()
        {
            IsDefaultAbpEntity = true,
            Name = "Role",
            DisplayName = "Role",
            ProjectId = projectDto.Id,
            PrimaryKeyType = PrimaryKeyType.Int,
            Properties = properties
        };

        await _entityRepository.InsertAsync(defaultRoleEntity);

        return projectDto;

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