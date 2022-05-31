using System;
using System.Threading.Tasks;
using Google.Protobuf.Collections;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using ProjectManager;
using SoftCraft.AppServices.Dtos;
using SoftCraft.AppServices.Entity.Dtos;
using SoftCraft.Entities;
using SoftCraft.Enums;
using SoftCraft.Repositories;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using PrimaryKeyType = DotNetCodeGenerator.PrimaryKeyType;
using TenantType = DotNetCodeGenerator.TenantType;

namespace SoftCraft.AppServices;

public class EntityAppService : CrudAppService<Entities.Entity, EntityDto, long, GetEntityListInput, CreateEntityInput,
    UpdateEntityInput>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IConfiguration _configuration;

    public EntityAppService(IRepository<Entities.Entity, long> repository,
        IProjectRepository projectRepository,
        IConfiguration configuration) : base(repository)
    {
        _projectRepository = projectRepository;
        _configuration = configuration;
    }

    public override async Task<EntityDto> CreateAsync(CreateEntityInput input)
    {
        try
        {
            var entity = await base.CreateAsync(input);

            if (entity.Project == null)
            {
                var project = await _projectRepository.GetAsync(entity.ProjectId);
                entity.Project = ObjectMapper.Map<Project, ProjectDto>(project);
            }

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
                Namespace = $"{entity.Project.Name}.Domain.Entities",
                Usings =
                {
                    "Abp.Domain.Entities.Auditing;",
                    "System.Collections.Generic;",
                    "Abp.Domain.Entities;"
                }
            };

            foreach (var createPropertyInput in input.Properties)
            {
                dotNetCodeGeneratorEntity.Properties.Add(new DotNetCodeGenerator.Property()
                {
                    Name = createPropertyInput.Name,
                    Type = GetNormalizedPropertyType(createPropertyInput.Type),
                    Nullable = createPropertyInput.IsNullable,
                    //TODO:RelationalProperty
                    IsRelationalProperty = false
                });
            }

            var entityResult = await client.CreateEntityAsync(dotNetCodeGeneratorEntity);

            using var projectManagerService =
                GrpcChannel.ForAddress(_configuration["MicroServices:ProjectManagerUrl"]);
            var projectManagerClient =
                new ProjectManager.ProjectManager.ProjectManagerClient(projectManagerService);
            var createEntityResult = await projectManagerClient.AddEntityToExistingProjectAsync(new AddEntityRequest()
            {
                Id = entity.ProjectId.ToString(),
                EntityName = entity.Name,
                ProjectName = entity.Project.NormalizedName,
                Stringified = entityResult.Stringified
            });

            if (createEntityResult != null)
            {
                return entity;
            }

            throw new Exception("CreateEntityError!");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
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