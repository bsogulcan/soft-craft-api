using System.Collections.Generic;
using System.Threading.Tasks;
using Humanizer;
using Newtonsoft.Json;
using SoftCraft.AppServices.Entity.Dtos;
using SoftCraft.AppServices.Property;
using SoftCraft.AppServices.Property.Dtos;
using SoftCraft.Repositories;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SoftCraft.AppServices;

public class PropertyAppService : CrudAppService<Entities.Property, PropertyFullOutput, long, GetPropertyListInput,
    CreatePropertyInput, UpdatePropertyInput>, IPropertyAppService
{
    private readonly IEntityRepository _entityRepository;
    public PropertyAppService(IPropertyRepository repository,IEntityRepository entityRepository) : base(repository)
    {
        this._entityRepository = entityRepository;
    }

    public override async Task<PropertyFullOutput> CreateAsync(CreatePropertyInput input)
    {
        if (input.IsRelationalProperty && input.RelationType == Enums.RelationType.ManyToMany)
        {
            var currentEntity = await this._entityRepository.GetAsync(input.EntityId);
            var releationalEntity = await this._entityRepository.GetAsync(input.RelationalEntityId.Value);

            Entities.Entity intermediateTable = new Entities.Entity
            {
                DisplayName = input.IntermediateTableName,
                Name = input.IntermediateTableName,
                PrimaryKeyType = currentEntity.PrimaryKeyType,
                IsFullAudited = true,
                ProjectId = currentEntity.ProjectId,
                TenantType = currentEntity.TenantType,
            };

            intermediateTable = await _entityRepository.InsertAsync(intermediateTable, true);

            input.RelationalEntityId = intermediateTable.Id;
            input.RelationType = Enums.RelationType.OneToMany;

            var currentEntityResult = await base.CreateAsync(input);

            input.Name = input.RelationalName;
            input.DisplayName = input.RelationalDisplayName;
            input.ToolTip = input.RelationalToolTip;
            input.EntityId = releationalEntity.Id;
            var releationalEntityResult = await base.CreateAsync(input);

            input.Name = currentEntity.Name;
            input.DisplayName = currentEntity.Name;
            input.ToolTip = currentEntity.Name;

            input.RelationalEntityId = currentEntity.Id;
            input.RelationType = Enums.RelationType.OneToOne;

            input.EntityId = intermediateTable.Id;

            var intermediateCurrentEntityResult = await base.CreateAsync(input);

            input.Name = releationalEntity.Name;
            input.DisplayName = releationalEntity.Name;
            input.ToolTip = releationalEntity.Name;

            input.RelationalEntityId = releationalEntity.Id;
            input.RelationType = Enums.RelationType.OneToOne;

            input.EntityId = intermediateTable.Id;

            var intermediateRelationalEntityResult = await base.CreateAsync(input);

            UpdatePropertyInput updatePropertyInput = JsonConvert.DeserializeObject<UpdatePropertyInput>(JsonConvert.SerializeObject(currentEntityResult));
            updatePropertyInput.LinkedPropertyId = intermediateCurrentEntityResult.Id;

            await base.UpdateAsync(updatePropertyInput.Id, updatePropertyInput);

            updatePropertyInput = JsonConvert.DeserializeObject<UpdatePropertyInput>(JsonConvert.SerializeObject(intermediateCurrentEntityResult));
            updatePropertyInput.LinkedPropertyId = currentEntityResult.Id;

            await base.UpdateAsync(updatePropertyInput.Id, updatePropertyInput);

            updatePropertyInput = JsonConvert.DeserializeObject<UpdatePropertyInput>(JsonConvert.SerializeObject(releationalEntityResult));
            updatePropertyInput.LinkedPropertyId = intermediateRelationalEntityResult.Id;

            await base.UpdateAsync(updatePropertyInput.Id, updatePropertyInput);

            updatePropertyInput = JsonConvert.DeserializeObject<UpdatePropertyInput>(JsonConvert.SerializeObject(intermediateRelationalEntityResult));
            updatePropertyInput.LinkedPropertyId = releationalEntityResult.Id;

            await base.UpdateAsync(updatePropertyInput.Id, updatePropertyInput);


            return intermediateCurrentEntityResult;
        } 
        else if (input.IsRelationalProperty && input.RelationType == Enums.RelationType.OneToMany)
        {
            var currentEntity = await this._entityRepository.GetAsync(input.EntityId);
            var releationalEntity = await this._entityRepository.GetAsync(input.RelationalEntityId.Value);

            input.RelationType = Enums.RelationType.OneToOne;
            var currentEntityResult = await base.CreateAsync(input);

            input.Name = input.RelationalName;
            input.DisplayName = input.RelationalDisplayName;
            input.ToolTip = input.RelationalToolTip;

            input.EntityId = releationalEntity.Id;
            input.RelationalEntityId = currentEntity.Id;

            input.RelationType = Enums.RelationType.OneToMany;

            var releationalEntityResult = await base.CreateAsync(input);

            UpdatePropertyInput updatePropertyInput = JsonConvert.DeserializeObject<UpdatePropertyInput>(JsonConvert.SerializeObject(currentEntityResult));
            updatePropertyInput.LinkedPropertyId = releationalEntityResult.Id;

            await base.UpdateAsync(updatePropertyInput.Id, updatePropertyInput);

            updatePropertyInput = JsonConvert.DeserializeObject<UpdatePropertyInput>(JsonConvert.SerializeObject(releationalEntityResult));
            updatePropertyInput.LinkedPropertyId = currentEntityResult.Id;

            await base.UpdateAsync(updatePropertyInput.Id, updatePropertyInput);

            return currentEntityResult;
        }
        else if (input.IsRelationalProperty && input.RelationType == Enums.RelationType.OneToOne)
        {
            var currentEntity = await this._entityRepository.GetAsync(input.EntityId);
            var releationalEntity = await this._entityRepository.GetAsync(input.RelationalEntityId.Value);

            var currentEntityResult = await base.CreateAsync(input);

            input.Name = input.RelationalName;
            input.DisplayName = input.RelationalDisplayName;
            input.ToolTip = input.RelationalToolTip;

            input.EntityId = releationalEntity.Id;
            input.RelationalEntityId = currentEntity.Id;

            var releationalEntityResult = await base.CreateAsync(input);

            UpdatePropertyInput updatePropertyInput = JsonConvert.DeserializeObject<UpdatePropertyInput>(JsonConvert.SerializeObject(currentEntityResult));
            updatePropertyInput.LinkedPropertyId = releationalEntityResult.Id;

            await base.UpdateAsync(updatePropertyInput.Id, updatePropertyInput);

            updatePropertyInput = JsonConvert.DeserializeObject<UpdatePropertyInput>(JsonConvert.SerializeObject(releationalEntityResult));
            updatePropertyInput.LinkedPropertyId = currentEntityResult.Id;

            await base.UpdateAsync(updatePropertyInput.Id, updatePropertyInput);

            return currentEntityResult;
        }
        else
        {
            return await base.CreateAsync(input);
        }
    }

    public override async Task<PagedResultDto<PropertyFullOutput>> GetListAsync(GetPropertyListInput input)
    {
        var properties = await Repository.GetListAsync(x => x.EntityId == input.EntityId);
        return new PagedResultDto<PropertyFullOutput>()
        {
            Items = ObjectMapper.Map<List<Entities.Property>, List<PropertyFullOutput>>(properties),
            TotalCount = properties.Count
        };
    }
}