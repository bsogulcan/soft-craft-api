using System.Collections.Generic;
using System.Threading.Tasks;
using SoftCraft.AppServices.Entity.Dtos;
using SoftCraft.AppServices.Property;
using SoftCraft.AppServices.Property.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SoftCraft.AppServices;

public class PropertyAppService : CrudAppService<Entities.Property, PropertyFullOutput, long, GetPropertyListInput,
    CreatePropertyInput, UpdatePropertyInput>, IPropertyAppService
{
    public PropertyAppService(IRepository<Entities.Property, long> repository) : base(repository)
    {
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