using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotNetCodeGenerator;
using Google.Protobuf.Collections;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using ProjectManager;
using SoftCraft.AppServices.Entity;
using SoftCraft.AppServices.Entity.Dtos;
using SoftCraft.AppServices.GeneratedCodeResult;
using SoftCraft.AppServices.Project.Dtos;
using SoftCraft.Entities;
using SoftCraft.Enums;
using SoftCraft.Manager.MicroServiceManager.DotNetCodeGeneratorServiceManager;
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

    public EntityAppService(IRepository<Entities.Entity, long> repository,
        IProjectRepository projectRepository,
        IConfiguration configuration,
        IDotNetCodeGeneratorServiceManager dotNetCodeGeneratorServiceManager) : base(repository)
    {
        _projectRepository = projectRepository;
        _configuration = configuration;
        _dotNetCodeGeneratorServiceManager = dotNetCodeGeneratorServiceManager;
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

        var createAppServiceResult = await _dotNetCodeGeneratorServiceManager.CreateAppServiceAsync(
            new AppServiceRequest()
            {
                EntityName = entity.Name,
                ProjectName = entity.Project.UniqueName
            });
        entityCodeResultDto.AppServiceResult.IAppServiceResult = createAppServiceResult.AppServiceInterfaceStringify;
        entityCodeResultDto.AppServiceResult.AppServiceResult = createAppServiceResult.AppServiceStringify;
        entityCodeResultDto.AppServiceResult.PermissionNamesResult = createAppServiceResult.PermissionNames;
        entityCodeResultDto.AppServiceResult.AuthorizationResult = createAppServiceResult.AuthorizationProviders;

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