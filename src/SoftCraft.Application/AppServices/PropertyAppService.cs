using System.Collections.Generic;
using System.Threading.Tasks;
using Humanizer;
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
        var result = await base.CreateAsync(input);
        if (input.IsRelationalProperty && input.RelationType == Enums.RelationType.OneToOne)
        {
            var RelationalEntity = await _entityRepository.GetAsync(input.RelationalEntityId.Value);
            if ((RelationalEntity.Name == "User") || (RelationalEntity.Name == "Role"))
            {
                var EntityId = input.EntityId;
                var RelationalEntityId = RelationalEntity.Id;
                var Name = input.Name;
                var DisplayName = input.DisplayName;

                input.RelationalEntityId = EntityId;
                input.EntityId = RelationalEntityId;

                input.Name = string.Format("{0}{1}By{2}", result.Entity.Name, Name.Pluralize(), RelationalEntity.Name);
                input.DisplayName = string.Format("{0}{1}By{2}", result.Entity.Name, DisplayName.Pluralize(), RelationalEntity.Name);

                input.RelationType = Enums.RelationType.OneToMany;

                input.IsNullable = false;
                input.Required = false;

                await base.CreateAsync(input);
            }
        }
        return result;
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