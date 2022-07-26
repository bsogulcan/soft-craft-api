using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Language;
using System.Threading.Tasks;
using DotNetCodeGenerator;
using Extensions;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using SoftCraft.Entities;
using SoftCraft.Enums;
using Entity = SoftCraft.Entities.Entity;
using PrimaryKeyType = DotNetCodeGenerator.PrimaryKeyType;
using RelationType = DotNetCodeGenerator.RelationType;
using TenantType = DotNetCodeGenerator.TenantType;

namespace SoftCraft.Manager.MicroServiceManager.DotNetCodeGeneratorServiceManager;

public class DotNetCodeGeneratorServiceManager : IDotNetCodeGeneratorServiceManager
{
    private readonly IConfiguration _configuration;

    public DotNetCodeGeneratorServiceManager(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<EntityResult> CreateEntityAsync(Entity entity)
    {
        using var dotNetCodeGeneratorChannel =
            GrpcChannel.ForAddress(_configuration["MicroServices:DotNetCodeGeneratorUrl"]);
        var client =
            new DotNetCodeGenerator.DotNetCodeGenerator.DotNetCodeGeneratorClient(dotNetCodeGeneratorChannel);

        var input = EntityToGeneratorEntity(entity);

        var entityResult = await client.CreateEntityAsync(input);
        return entityResult;
    }

    public async Task<EntityResult> CreateConfigurationAsync(Entity entity)
    {
        using var dotNetCodeGeneratorChannel =
            GrpcChannel.ForAddress(_configuration["MicroServices:DotNetCodeGeneratorUrl"]);
        var client =
            new DotNetCodeGenerator.DotNetCodeGenerator.DotNetCodeGeneratorClient(dotNetCodeGeneratorChannel);

        var input = EntityToGeneratorEntity(entity);

        var result = await client.CreateConfigurationAsync(input);
        return result;
    }

    public async Task<EntityResult> CreateRepositoryInterfaceAsync(Entity entity)
    {
        using var dotNetCodeGeneratorChannel =
            GrpcChannel.ForAddress(_configuration["MicroServices:DotNetCodeGeneratorUrl"]);
        var client =
            new DotNetCodeGenerator.DotNetCodeGenerator.DotNetCodeGeneratorClient(dotNetCodeGeneratorChannel);

        var createRepositoryInput = new EntityForRepository
        {
            Name = entity.Name,
            PrimaryKeyType = (PrimaryKeyType) entity.PrimaryKeyType,
            Namespace = $"{entity.Project.UniqueName}.EntityFrameworkCore.Repositories",
            ProjectName = entity.Project.UniqueName,
            Usings =
            {
                $"{entity.Project.UniqueName}.Domain.Entities;",
            }
        };
        var result = await client.CreateRepositoryInterfaceAsync(createRepositoryInput);
        return result;
    }

    public async Task<EntityResult> CreateRepositoryAsync(Entity entity)
    {
        using var dotNetCodeGeneratorChannel =
            GrpcChannel.ForAddress(_configuration["MicroServices:DotNetCodeGeneratorUrl"]);
        var client =
            new DotNetCodeGenerator.DotNetCodeGenerator.DotNetCodeGeneratorClient(dotNetCodeGeneratorChannel);

        var createRepositoryInput = new EntityForRepository
        {
            Name = entity.Name,
            PrimaryKeyType = (PrimaryKeyType) entity.PrimaryKeyType,
            Namespace = $"{entity.Project.UniqueName}.EntityFrameworkCore.Repositories",
            ProjectName = entity.Project.UniqueName,
            Usings =
            {
                $"{entity.Project.UniqueName}.Domain.Entities;",
            }
        };
        var result = await client.CreateRepositoryAsync(createRepositoryInput);
        return result;
    }

    public async Task<DtoResult> CreateDtosAsync(Entity entity)
    {
        using var dotNetCodeGeneratorChannel =
            GrpcChannel.ForAddress(_configuration["MicroServices:DotNetCodeGeneratorUrl"]);
        var client =
            new DotNetCodeGenerator.DotNetCodeGenerator.DotNetCodeGeneratorClient(dotNetCodeGeneratorChannel);

        var input = EntityToGeneratorEntity(entity);
        input.Namespace = $"{entity.Project.UniqueName}.Domain.{entity.Name}.Dtos";
        input.Usings.Clear();
        if (entity.Properties.Any(x => x.IsRelationalProperty))
        {
            input.Usings.Add("System.Collections.Generic;");
            foreach (var property in entity.Properties.Where(x => x.IsRelationalProperty))
            {
                input.Usings.Add(
                    $"{entity.Project.UniqueName}.Domain.{property.RelationalEntity.Name}.Dtos;");
            }
        }

        if (entity.Properties.Any(x => x.IsEnumProperty))
        {
            input.Usings.Add($"{entity.Project.UniqueName}.Domain.EntityHelper;");
        }

        var result = await client.CreateDtosAsync(input);
        return result;
    }

    public async Task<AppServiceResult> CreateAppServiceAsync(AppServiceRequest appServiceRequest)
    {
        using var dotNetCodeGeneratorChannel =
            GrpcChannel.ForAddress(_configuration["MicroServices:DotNetCodeGeneratorUrl"]);
        var client =
            new DotNetCodeGenerator.DotNetCodeGenerator.DotNetCodeGeneratorClient(dotNetCodeGeneratorChannel);

        var result = await client.CreateAppServiceAsync(appServiceRequest);
        return result;
    }

    public async Task<EntityResult> CreateEnumAsync(Enumerate enumerate)
    {
        using var dotNetCodeGeneratorChannel =
            GrpcChannel.ForAddress(_configuration["MicroServices:DotNetCodeGeneratorUrl"]);
        var client =
            new DotNetCodeGenerator.DotNetCodeGenerator.DotNetCodeGeneratorClient(dotNetCodeGeneratorChannel);

        var createEnumInput = new Enum()
        {
            Name = enumerate.Name,
            Namespace = $"{enumerate.Project.UniqueName}.Domain.EntityHelper",
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

        var result = await client.CreateEnumAsync(createEnumInput);
        return result;
    }

    private DotNetCodeGenerator.Entity EntityToGeneratorEntity(Entity entity)
    {
        var dotNetCodeGeneratorEntity = new DotNetCodeGenerator.Entity()
        {
            Name = entity.Name,
            FullAudited = entity.IsFullAudited,
            PrimaryKeyType = (PrimaryKeyType) entity.PrimaryKeyType,
            TenantType = (TenantType) entity.TenantType,
            Namespace = $"{entity.Project.UniqueName}.Domain.Entities",
            ProjectName = entity.Project.UniqueName,
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
                                                        && x.RelationType == Enums.RelationType.OneToMany),
                OneToOne = entityProperty.RelationalEntity != null &&
                           entityProperty.RelationalEntity.Properties.Any(x =>
                               x.IsRelationalProperty && x.RelationalEntityId == entity.Id
                                                      && x.RelationType == Enums.RelationType.OneToOne),
            };

            if (entityProperty.IsRelationalProperty && entityProperty.RelationalEntityId.HasValue)
            {
                property.RelationalEntityPrimaryKeyType =
                    (PrimaryKeyType) entityProperty.RelationalEntity.PrimaryKeyType;
                property.RelationalEntityName = entityProperty.RelationalEntity.Name;

                var relationalEntityForeignProperty = entityProperty.RelationalEntity.Properties.FirstOrDefault(x =>
                    x.RelationalEntityId == entity.Id);

                if (relationalEntityForeignProperty != null)
                {
                    property.RelationalPropertyName =
                        entityProperty.RelationalEntity.Properties.FirstOrDefault(x =>
                            x.RelationalEntityId == entity.Id).Name;
                }
                else
                {
                    property.RelationalPropertyName = property.RelationalEntityName;
                }


                if (entityProperty.RelationType != null)
                {
                    property.RelationType = (RelationType) entityProperty.RelationType;
                }
            }
            else if (entityProperty.IsEnumProperty)
            {
                if (dotNetCodeGeneratorEntity.Usings.FindIndex(x =>
                        x.Contains($"{entity.Project.UniqueName}.Domain.EntityHelper;")) == -1)
                {
                    dotNetCodeGeneratorEntity.Usings.Add($"{entity.Project.UniqueName}.Domain.EntityHelper;");
                }

                property.Type = entityProperty.Enumerate.Name;
            }
            else
            {
                property.Type = Extensions.PropertyTypeExtensions.GetNormalizedPropertyType(entityProperty.Type);
            }

            dotNetCodeGeneratorEntity.Properties.Add(property);
        }

        return dotNetCodeGeneratorEntity;
    }
}