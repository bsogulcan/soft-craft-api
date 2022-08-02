using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Extensions;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using ProjectManager;
using SoftCraft.AppServices.Navigations.Dtos;
using SoftCraft.Entities;
using TypeScriptCodeGenerator;
using Entity = Volo.Abp.Domain.Entities.Entity;
using Property = TypeScriptCodeGenerator.Property;

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
            Name = entity.Name,
            PrimaryKeyType = (PrimaryKeyType) entity.PrimaryKeyType,
            Properties = { }
        };

        foreach (var property in entity.Properties.Where(x =>
                     x.IsRelationalProperty && x.RelationType == Enums.RelationType.OneToOne))
        {
            input.Properties.Add(new Property()
            {
                Name = property.Name,
                Type = PropertyTypeExtensions.ConvertPrimaryKeyToTypeScriptDataType(
                    (int) property.Entity.PrimaryKeyType),
                IsRelationalProperty = property.IsRelationalProperty,
                RelationType = (RelationType) property.RelationType
            });
        }

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
        var client = new TypeScriptCodeGenerator.TypeScriptCodeGenerator.TypeScriptCodeGeneratorClient(
            typeScriptCodeGeneratorChannel);

        var input = EntityToGeneratorEntity(entity);

        var entityResult = await client.CreateComponentsAsync(input);
        return entityResult;
    }

    public async Task<StringifyResult> CreateNavigationItems(List<Navigation> navigations)
    {
        using var typeScriptCodeGeneratorChannel =
            GrpcChannel.ForAddress(_configuration["MicroServices:TypeScriptCodeGeneratorUrl"]);
        var client =
            new TypeScriptCodeGenerator.TypeScriptCodeGenerator.TypeScriptCodeGeneratorClient(
                typeScriptCodeGeneratorChannel);

        var navigationInputs = new CreateNavigationItemRequest();

        foreach (var navigation in navigations.Where(x => !x.ParentNavigationId.HasValue).OrderBy(x => x.Index))
        {
            var input = new NavigationItemRequest()
            {
                Caption = navigation.Caption,
                Icon = navigation.Icon,
                Index = navigation.Index
            };

            if (navigation.Entity != null)
            {
                input.EntityName = navigation.Entity.Name;
            }

            foreach (var navigationOfNavigation in navigation.Navigations)
            {
                input.Navigations.Add(GetNavigationInputs(navigationOfNavigation));
            }

            navigationInputs.Navigations.Add(input);
        }

        var result = await client.CreateNavigationItemsAsync(navigationInputs);
        return result;
    }

    private NavigationItemRequest GetNavigationInputs(Navigation navigation)
    {
        var input = new NavigationItemRequest()
        {
            Caption = navigation.Caption,
            Icon = navigation.Icon,
            Index = navigation.Index
        };

        if (navigation.Entity != null)
        {
            input.EntityName = navigation.Entity.Name;
        }


        foreach (var navigationOfNavigation in navigation.Navigations)
        {
            input.Navigations.Add(GetNavigationInputs(navigationOfNavigation));
        }

        return input;
    }

    private TypeScriptCodeGenerator.Entity EntityToGeneratorEntity(Entities.Entity entity)
    {
        var dotNetCodeGeneratorEntity = new TypeScriptCodeGenerator.Entity()
        {
            Name = entity.Name,
            PrimaryKeyType = (PrimaryKeyType) entity.PrimaryKeyType,
            ProjectName = entity.Project.UniqueName,
            ProjectDisplayName = entity.Project.Name
        };

        foreach (var relationalEntity in entity.Properties.Where(x =>
                     x.IsRelationalProperty && x.RelationType == Enums.RelationType.OneToOne))
        {
            dotNetCodeGeneratorEntity.ParentEntities.Add(EntityToGeneratorEntity(relationalEntity.RelationalEntity));
        }

        foreach (var entityProperty in entity.Properties)
        {
            var property = new TypeScriptCodeGenerator.Property()
            {
                Name = entityProperty.Name,
                //Type = GetNormalizedPropertyType(entityProperty.Type.Value),
                Nullable = entityProperty.IsNullable,
                IsRelationalProperty = entityProperty.IsRelationalProperty,
                DisplayOnList = entityProperty.DisplayOnList,
                FilterOnList = entityProperty.FilterOnList
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