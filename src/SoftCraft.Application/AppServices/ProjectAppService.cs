using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetCodeGenerator;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using ProjectManager;
using SoftCraft.AppServices.Project.Dtos;
using SoftCraft.Enums;
using SoftCraft.Repositories;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Users;
using PrimaryKeyType = DotNetCodeGenerator.PrimaryKeyType;
using RelationType = DotNetCodeGenerator.RelationType;
using TenantType = DotNetCodeGenerator.TenantType;

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

    public async Task Generate(GenerateProjectInput input)
    {
        try
        {
            var project = await Repository.GetAsync(input.Id);

            using var projectManagerChannel =
                GrpcChannel.ForAddress(_configuration["MicroServices:ProjectManagerUrl"]);
            var projectManagerClient =
                new ProjectManager.ProjectManager.ProjectManagerClient(projectManagerChannel);

            var result = await projectManagerClient.CreateAbpBoilerplateProjectAsync(new ProjectRequest()
            {
                Id = project.Id.ToString(),
                Name = project.UniqueName,
                LogManagement = (LogManagement) project.LogType,
                MultiTenant = project.MultiTenant
            });

            foreach (var entity in project.Entities)
            {
                using var dotNetCodeGeneratorChannel =
                    GrpcChannel.ForAddress(_configuration["MicroServices:DotNetCodeGeneratorUrl"]);
                var client =
                    new DotNetCodeGenerator.DotNetCodeGenerator.DotNetCodeGeneratorClient(dotNetCodeGeneratorChannel);

                var dotNetCodeGeneratorEntity = new DotNetCodeGenerator.Entity()
                {
                    Name = entity.Name,
                    FullAudited = entity.IsFullAudited,
                    PrimaryKeyType = (PrimaryKeyType) entity.PrimaryKeyType,
                    TenantType = (TenantType) entity.TenantType,
                    Namespace = $"{project.UniqueName}.Domain.Entities",
                    ProjectName = project.UniqueName,
                    Usings =
                    {
                        "Abp.Domain.Entities.Auditing;",
                        "System.Collections.Generic;",
                        "Abp.Domain.Entities;"
                    }
                };

                foreach (var entityProperty in entity.Properties)
                {
                    var property = new DotNetCodeGenerator.Property()
                    {
                        Name = entityProperty.Name,
                        //Type = GetNormalizedPropertyType(entityProperty.Type.Value),
                        Nullable = entityProperty.IsNullable,
                        IsRelationalProperty = entityProperty.IsRelationalProperty,
                        MaxLength = entityProperty.MaxLength,
                        ManyToMany = entityProperty.RelationalEntity != null &&
                                     entityProperty.RelationalEntity.Properties.Any(x =>
                                         x.IsRelationalProperty && x.RelationalEntityId == entity.Id
                                         &&x.RelationType==Enums.RelationType.OneToMany)
                    };

                    if (entityProperty.IsRelationalProperty && entityProperty.RelationalEntityId.HasValue)
                    {
                        property.RelationalEntityPrimaryKeyType =
                            (PrimaryKeyType) entityProperty.RelationalEntity.PrimaryKeyType;
                        property.RelationalEntityName = entityProperty.RelationalEntity.Name;

                        if (entityProperty.RelationType != null)
                        {
                            property.RelationType = (RelationType) entityProperty.RelationType;
                        }
                    }
                    else if (entityProperty.IsEnumProperty)
                    {
                        if (dotNetCodeGeneratorEntity.Usings.FindIndex(x =>
                                x.Contains($"{project.UniqueName}.Domain.EntityHelper;")) == -1)
                        {
                            dotNetCodeGeneratorEntity.Usings.Add($"{project.UniqueName}.Domain.EntityHelper;");
                        }

                        property.Type = entityProperty.Enumerate.Name;
                    }
                    else
                    {
                        property.Type = GetNormalizedPropertyType(entityProperty.Type);
                    }

                    dotNetCodeGeneratorEntity.Properties.Add(property);
                }

                var entityResult = await client.CreateEntityAsync(dotNetCodeGeneratorEntity);
                var createEntityResult = await projectManagerClient.AddEntityToExistingProjectAsync(
                    new AddEntityRequest()
                    {
                        Id = project.Id.ToString(),
                        EntityName = entity.Name,
                        ProjectName = project.UniqueName,
                        Stringified = entityResult.Stringified
                    });

                var createEntityConfigurationResult = await client.CreateConfigurationAsync(dotNetCodeGeneratorEntity);
                var createConfigurationResult = await projectManagerClient.AddConfigurationToExistingProjectAsync(
                    new AddEntityRequest()
                    {
                        Id = project.Id.ToString(),
                        EntityName = entity.Name,
                        ProjectName = project.UniqueName,
                        Stringified = createEntityConfigurationResult.Stringified
                    });
                var createRepositoryInput = new EntityForRepository
                {
                    Name = entity.Name,
                    PrimaryKeyType = (PrimaryKeyType) entity.PrimaryKeyType,
                    Namespace = $"{project.UniqueName}.EntityFrameworkCore.Repositories",
                    ProjectName = project.UniqueName,
                    Usings =
                    {
                        $"{project.UniqueName}.Domain.Entities;",
                    }
                };

                var repositoryInterfaceResult = await client.CreateRepositoryInterfaceAsync(createRepositoryInput);
                var repositoryResult = await client.CreateRepositoryAsync(createRepositoryInput);

                var createRepositoryResult = await projectManagerClient.AddRepositoryToExistingProjectAsync(
                    new AddRepositoryRequest()
                    {
                        Id = project.Id.ToString(),
                        EntityName = entity.Name,
                        ProjectName = project.UniqueName,
                        StringifiedRepositoryInterface = repositoryInterfaceResult.Stringified,
                        StringifiedRepository = repositoryResult.Stringified
                    });

                dotNetCodeGeneratorEntity.Namespace = $"{project.UniqueName}.Domain.{entity.Name}.Dtos";
                dotNetCodeGeneratorEntity.Usings.Clear();
                if (entity.Properties.Any(x => x.IsRelationalProperty))
                {
                    dotNetCodeGeneratorEntity.Usings.Add("System.Collections.Generic;");
                    foreach (var property in entity.Properties.Where(x => x.IsRelationalProperty))
                    {
                        dotNetCodeGeneratorEntity.Usings.Add(
                            $"{project.UniqueName}.Domain.{property.RelationalEntity.Name}.Dtos;");
                    }
                }

                if (entity.Properties.Any(x => x.IsEnumProperty))
                {
                    dotNetCodeGeneratorEntity.Usings.Add($"{project.UniqueName}.Domain.EntityHelper;");
                }

                var createDtosResult = await client.CreateDtosAsync(dotNetCodeGeneratorEntity);
                var addDtosToExistingProjectReply = await projectManagerClient.AddDtosToExistingProjectAsync(
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

                var createAppServiceResult = await client.CreateAppServiceAsync(new AppServiceRequest()
                {
                    EntityName = entity.Name,
                    ProjectName = project.UniqueName
                });


                var addAppServiceResult = await projectManagerClient.AddAppServiceToExistingProjectAsync(
                    new AddAppServiceRequest()
                    {
                        Id = project.Id.ToString(),
                        EntityName = entity.Name,
                        ProjectName = project.UniqueName,
                        AppServiceStringify = createAppServiceResult.AppServiceStringify,
                        AppServiceInterfaceStringify = createAppServiceResult.AppServiceInterfaceStringify
                    });
            }

            foreach (var enumerate in project.Enumerates)
            {
                using var dotNetCodeGeneratorChannel =
                    GrpcChannel.ForAddress(_configuration["MicroServices:DotNetCodeGeneratorUrl"]);
                var client =
                    new DotNetCodeGenerator.DotNetCodeGenerator.DotNetCodeGeneratorClient(dotNetCodeGeneratorChannel);

                var createEnumInput = new DotNetCodeGenerator.Enum()
                {
                    Name = enumerate.Name,
                    Namespace = $"{project.UniqueName}.Domain.EntityHelper",
                    Values = { }
                };

                foreach (var enumerateValue in enumerate.EnumerateValues)
                {
                    createEnumInput.Values.Add(new EnumValue()
                    {
                        Name = enumerateValue.Name,
                        Value = enumerateValue.Value
                    });
                }

                var createEnumResult = await client.CreateEnumAsync(createEnumInput);

                var createEntityResult = await projectManagerClient.AddEnumToExistingProjectAsync(
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