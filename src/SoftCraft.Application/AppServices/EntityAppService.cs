using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetCodeGenerator;
using Extensions;
using Google.Protobuf.Collections;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using ProjectManager;
using SoftCraft.AppServices.Entity;
using SoftCraft.AppServices.Entity.Dtos;
using SoftCraft.AppServices.GeneratedCodeResult;
using SoftCraft.AppServices.GeneratedCodeResult.Dtos;
using SoftCraft.AppServices.Project.Dtos;
using SoftCraft.Entities;
using SoftCraft.Enums;
using SoftCraft.Manager.MicroServiceManager.DotNetCodeGeneratorServiceManager;
using SoftCraft.Manager.MicroServiceManager.TypeScriptCodeGeneratorServiceManager;
using SoftCraft.Repositories;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using PrimaryKeyType = DotNetCodeGenerator.PrimaryKeyType;
using RelationType = DotNetCodeGenerator.RelationType;
using TenantType = DotNetCodeGenerator.TenantType;

namespace SoftCraft.AppServices;

public class EntityAppService : CrudAppService<Entities.Entity, EntityPartOutput, long, GetEntityListInput,
    CreateEntityInput,
    UpdateEntityInput>, IEntityAppService
{
    private readonly IProjectRepository _projectRepository;
    private readonly IConfiguration _configuration;
    private readonly IDotNetCodeGeneratorServiceManager _dotNetCodeGeneratorServiceManager;
    private readonly ITypeScriptCodeGeneratorServiceManager _typeScriptCodeGeneratorServiceManager;

    public EntityAppService(IRepository<Entities.Entity, long> repository,
        IProjectRepository projectRepository,
        IConfiguration configuration,
        IDotNetCodeGeneratorServiceManager dotNetCodeGeneratorServiceManager,
        ITypeScriptCodeGeneratorServiceManager typeScriptCodeGeneratorServiceManager) : base(repository)
    {
        _projectRepository = projectRepository;
        _configuration = configuration;
        _dotNetCodeGeneratorServiceManager = dotNetCodeGeneratorServiceManager;
        _typeScriptCodeGeneratorServiceManager = typeScriptCodeGeneratorServiceManager;
    }

    public override async Task<EntityPartOutput> CreateAsync(CreateEntityInput input)
    {
        try
        {
            var entity = await base.CreateAsync(input);

            return entity;
            // if (entity.Project == null)
            // {
            //     var project = await _projectRepository.GetAsync(entity.ProjectId);
            //     entity.Project = ObjectMapper.Map<Entities.Project, ProjectPartOutput>(project);
            // }
            //
            // using var dotNetCodeGeneratorChannel =
            //     GrpcChannel.ForAddress(_configuration["MicroServices:DotNetCodeGeneratorUrl"]);
            // var client =
            //     new DotNetCodeGenerator.DotNetCodeGenerator.DotNetCodeGeneratorClient(dotNetCodeGeneratorChannel);
            //
            // var dotNetCodeGeneratorEntity = new DotNetCodeGenerator.Entity()
            // {
            //     Name = entity.Name,
            //     FullAudited = entity.IsFullAudited,
            //     PrimaryKeyType = (PrimaryKeyType) entity.PrimaryKeyType,
            //     TenantType = (TenantType) entity.TenantType,
            //     Namespace = $"{entity.Project.Name}.Domain.Entities",
            //     Usings =
            //     {
            //         "Abp.Domain.Entities.Auditing;",
            //         "System.Collections.Generic;",
            //         "Abp.Domain.Entities;"
            //     }
            // };
            //
            // foreach (var createPropertyInput in input.Properties)
            // {
            //     var property = new DotNetCodeGenerator.Property()
            //     {
            //         Name = createPropertyInput.Name,
            //         Type = GetNormalizedPropertyType(createPropertyInput.Type),
            //         Nullable = createPropertyInput.IsNullable,
            //         //TODO:RelationalProperty
            //         IsRelationalProperty = createPropertyInput.IsRelationalProperty,
            //     };
            //
            //     if (createPropertyInput.IsRelationalProperty && createPropertyInput.RelationalEntityId.HasValue)
            //     {
            //         var relationalEntity = await Repository.GetAsync(createPropertyInput.RelationalEntityId.Value);
            //         property.RelationalEntityPrimaryKeyType = (PrimaryKeyType) relationalEntity.PrimaryKeyType;
            //         property.RelationalEntityName = relationalEntity.Name;
            //
            //         if (createPropertyInput.RelationType != null)
            //         {
            //             property.RelationType = (RelationType) createPropertyInput.RelationType;
            //         }
            //     }
            //
            //
            //     dotNetCodeGeneratorEntity.Properties.Add(property);
            // }
            //
            // var entityResult = await client.CreateEntityAsync(dotNetCodeGeneratorEntity);
            //
            // using var projectManagerService =
            //     GrpcChannel.ForAddress(_configuration["MicroServices:ProjectManagerUrl"]);
            // var projectManagerClient =
            //     new ProjectManager.ProjectManager.ProjectManagerClient(projectManagerService);
            // var createEntityResult = await projectManagerClient.AddEntityToExistingProjectAsync(new AddEntityRequest()
            // {
            //     Id = entity.ProjectId.ToString(),
            //     EntityName = entity.Name,
            //     ProjectName = entity.Project.NormalizedName,
            //     Stringified = entityResult.Stringified
            // });
            //
            // if (createEntityResult != null)
            // {
            //     return entity;
            // }
            //
            // throw new Exception("CreateEntityError!");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<EntityCodeResultDto> GetCodeResult(long id)
    {
        var entity = await Repository.GetAsync(id);
        var entityCodeResultDto = new EntityCodeResultDto
        {
            EntityId = entity.Id,
            EntityName = entity.Name
        };

        var typeScripDtosResult = await _typeScriptCodeGeneratorServiceManager.CreateDtosAsync(entity);
        entityCodeResultDto.TypeScriptDtoResult.FullOutput = typeScripDtosResult.FullOutputStringify;
        entityCodeResultDto.TypeScriptDtoResult.PartOutput = typeScripDtosResult.PartOutputStringify;
        entityCodeResultDto.TypeScriptDtoResult.CreateInput = typeScripDtosResult.CreateInputStringify;
        entityCodeResultDto.TypeScriptDtoResult.UpdateInput = typeScripDtosResult.UpdateInputStringify;
        entityCodeResultDto.TypeScriptDtoResult.GetInput = typeScripDtosResult.GetInputStringify;
        entityCodeResultDto.TypeScriptDtoResult.DeleteInput = typeScripDtosResult.DeleteInputStringify;

        var typeScriptServiceResult = await _typeScriptCodeGeneratorServiceManager.CreateServiceAsync(entity);
        entityCodeResultDto.TypeScriptServiceResult = typeScriptServiceResult.Stringify;

        var entityResult =
            await _dotNetCodeGeneratorServiceManager.CreateEntityAsync(entity);
        entityCodeResultDto.EntityResult = entityResult.Stringified;

        var createEntityConfigurationResult =
            await _dotNetCodeGeneratorServiceManager.CreateConfigurationAsync(entity);
        entityCodeResultDto.ConfigurationResult = createEntityConfigurationResult.Stringified;

        var repositoryInterfaceResult =
            await _dotNetCodeGeneratorServiceManager.CreateRepositoryInterfaceAsync(entity);
        entityCodeResultDto.RepositoryResult.IRepositoryResult = repositoryInterfaceResult.Stringified;

        var repositoryResult = await _dotNetCodeGeneratorServiceManager.CreateRepositoryAsync(entity);
        entityCodeResultDto.RepositoryResult.RepositoryResult = repositoryResult.Stringified;

        var createDtosResult = await _dotNetCodeGeneratorServiceManager.CreateDtosAsync(entity);
        entityCodeResultDto.DtoResult.CreateInput = createDtosResult.CreateInputStringify;
        entityCodeResultDto.DtoResult.UpdateInput = createDtosResult.UpdateInputStringify;
        entityCodeResultDto.DtoResult.GetInput = createDtosResult.GetInputStringify;
        entityCodeResultDto.DtoResult.DeleteInput = createDtosResult.DeleteInputStringify;
        entityCodeResultDto.DtoResult.FullOutput = createDtosResult.FullOutputStringify;
        entityCodeResultDto.DtoResult.PartOutput = createDtosResult.PartOutputStringify;
        entityCodeResultDto.DtoResult.DtosToDomainMapResult = createDtosResult.DtosToDomainStringify;
        entityCodeResultDto.DtoResult.DomainToDtosMapResult = createDtosResult.DomainToDtosStringify;


        var entityFromGenerator = _dotNetCodeGeneratorServiceManager.EntityToGeneratorEntity(entity);


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
                RelationType = (RelationType) relationalProperty.RelationType,
                Name = relationalProperty.Name,
                Type = PropertyTypeExtensions.ConvertPrimaryKeyToDotNetDataType(relationalProperty.Entity
                    .PrimaryKeyType),
            });
        }

        var createAppServiceResult =
            await _dotNetCodeGeneratorServiceManager.CreateAppServiceAsync(createAppServiceInput);

        entityCodeResultDto.AppServiceResult.IAppServiceResult = createAppServiceResult.AppServiceInterfaceStringify;
        entityCodeResultDto.AppServiceResult.AppServiceResult = createAppServiceResult.AppServiceStringify;
        entityCodeResultDto.AppServiceResult.PermissionNamesResult = createAppServiceResult.PermissionNames;
        entityCodeResultDto.AppServiceResult.AuthorizationResult = createAppServiceResult.AuthorizationProviders;

        //TODO: Get TS Enums

        var createComponentResult = await _typeScriptCodeGeneratorServiceManager.CreateComponentsAsync(entity);
        entityCodeResultDto.TypeScriptComponentResult.ComponentTsStringify =
            createComponentResult.ListComponent.ComponentTsStringify;
        entityCodeResultDto.TypeScriptComponentResult.ComponentHtmlStringify =
            createComponentResult.ListComponent.ComponentHtmlStringify;
        entityCodeResultDto.TypeScriptComponentResult.ComponentCssStringify =
            createComponentResult.ListComponent.ComponentCssStringify;

        entityCodeResultDto.TypeScriptCreateComponentResult.ComponentTsStringify =
            createComponentResult.CreateComponent.ComponentTsStringify;
        entityCodeResultDto.TypeScriptCreateComponentResult.ComponentHtmlStringify =
            createComponentResult.CreateComponent.ComponentHtmlStringify;
        entityCodeResultDto.TypeScriptCreateComponentResult.ComponentCssStringify =
            createComponentResult.CreateComponent.ComponentCssStringify;

        // entityCodeResultDto.TypeScriptEditComponentResult.ComponentTsStringify =
        //     createComponentResult.EditComponent.ComponentTsStringify;
        // entityCodeResultDto.TypeScriptEditComponentResult.ComponentHtmlStringify =
        //     createComponentResult.EditComponent.ComponentHtmlStringify;
        // entityCodeResultDto.TypeScriptEditComponentResult.ComponentCssStringify =
        //     createComponentResult.EditComponent.ComponentCssStringify;
        return entityCodeResultDto;
    }

    public override async Task<PagedResultDto<EntityPartOutput>> GetListAsync(GetEntityListInput input)
    {
        var entities = await Repository.GetListAsync(x => x.ProjectId == input.ProjectId);
        return new PagedResultDto<EntityPartOutput>()
        {
            Items = ObjectMapper.Map<List<Entities.Entity>, List<EntityPartOutput>>(entities),
            TotalCount = entities.Count
        };
    }

    private static string GetNormalizedPropertyType(PropertyType type)
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
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}