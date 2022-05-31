using SoftCraft.AppServices.Property.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SoftCraft.AppServices;

public class PropertyAppService : CrudAppService<Entities.Property, PropertyDto, long, GetPropertyListInput,
    CreatePropertyInput, UpdatePropertyInput>
{
    public PropertyAppService(IRepository<Entities.Property, long> repository) : base(repository)
    {
    }
}