using System.Threading.Tasks;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using ProjectManager;
using SoftCraft.Entities;
using TypeScriptCodeGenerator;
using Entity = Volo.Abp.Domain.Entities.Entity;

namespace SoftCraft.Manager.MicroServiceManager.TypeScriptCodeGeneratorServiceManager;

public class TypeScriptCodeGeneratorServiceManager : ITypeScriptCodeGeneratorServiceManager
{
    private readonly IConfiguration _configuration;

    public TypeScriptCodeGeneratorServiceManager(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<DtoResult> CreateDtosAsync(Entities.Entity entity)
    {
        using var typeScriptCodeGeneratorChannel =
            GrpcChannel.ForAddress(_configuration["MicroServices:TypeScriptCodeGeneratorUrl"]);
        var client =
            new TypeScriptCodeGenerator.TypeScriptCodeGenerator.TypeScriptCodeGeneratorClient(
                typeScriptCodeGeneratorChannel);

        var input = EntityToGeneratorEntity(entity);

        var entityResult = await client.CreateDtosAsync(input);
        return entityResult;
    }

    public async Task<ServiceResult> CreateServiceAsync(Entities.Entity entity)
    {
        using var typeScriptCodeGeneratorChannel =
            GrpcChannel.ForAddress(_configuration["MicroServices:TypeScriptCodeGeneratorUrl"]);
        var client =
            new TypeScriptCodeGenerator.TypeScriptCodeGenerator.TypeScriptCodeGeneratorClient(
                typeScriptCodeGeneratorChannel);

        var input = new TypeScriptCodeGenerator.Entity()
        {
            Name = entity.Name
        };

        var entityResult = await client.CreateServiceAsync(input);
        return entityResult;
    }

    public async Task<StringifyResult> CreateEnumAsync(Enumerate enumerate)
    {
        using var typeScriptCodeGeneratorChannel =
            GrpcChannel.ForAddress(_configuration["MicroServices:TypeScriptCodeGeneratorUrl"]);
        var client =
            new TypeScriptCodeGenerator.TypeScriptCodeGenerator.TypeScriptCodeGeneratorClient(
                typeScriptCodeGeneratorChannel);

        var input = new EnumRequest
        {
            Name = enumerate.Name
        };

        foreach (var enumerateValue in enumerate.EnumerateValues)
        {
            input.Values.Add(new EnumValue
            {
                Name = enumerateValue.Name,
                Value = enumerateValue.Value
            });
        }

        var enumResult = await client.CreateEnumAsync(input);
        return enumResult;
    }

    public async Task<TypeScriptCodeGenerator.ComponentResult> CreateComponentsAsync(Entities.Entity entity)
    {
        using var typeScriptCodeGeneratorChannel =
            GrpcChannel.ForAddress(_configuration["MicroServices:TypeScriptCodeGeneratorUrl"]);
        var client =
            new TypeScriptCodeGenerator.TypeScriptCodeGenerator.TypeScriptCodeGeneratorClient(
                typeScriptCodeGeneratorChannel);

        var input = EntityToGeneratorEntity(entity);

        var entityResult = await client.CreateComponentsAsync(input);
        return entityResult;
    }

    private TypeScriptCodeGenerator.Entity EntityToGeneratorEntity(Entities.Entity entity)
    {
        var dotNetCodeGeneratorEntity = new TypeScriptCodeGenerator.Entity()
        {
            Name = entity.Name,
            PrimaryKeyType = (PrimaryKeyType) entity.PrimaryKeyType,
            ProjectName = entity.Project.UniqueName,
        };

        foreach (var entityProperty in entity.Properties)
        {
            var property = new TypeScriptCodeGenerator.Property()
            {
                Name = entityProperty.Name,
                //Type = GetNormalizedPropertyType(entityProperty.Type.Value),
                Nullable = entityProperty.IsNullable,
                IsRelationalProperty = entityProperty.IsRelationalProperty,
            };

            if (entityProperty.IsRelationalProperty && entityProperty.RelationalEntityId.HasValue)
            {
                property.RelationalEntityPrimaryKeyType =
                    (PrimaryKeyType) entityProperty.RelationalEntity.PrimaryKeyType;
                property.RelationalEntityName = entityProperty.RelationalEntity.Name;
                property.Type = property.RelationalEntityName;
                if (entityProperty.RelationType != null)
                {
                    property.RelationType = (RelationType) entityProperty.RelationType;
                }
            }
            else if (entityProperty.IsEnumProperty)
            {
                property.Type = entityProperty.Enumerate.Name;
                property.IsEnumerateProperty = true;
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