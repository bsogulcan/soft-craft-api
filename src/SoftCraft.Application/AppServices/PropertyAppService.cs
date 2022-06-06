using SoftCraft.AppServices.Property;
using SoftCraft.AppServices.Property.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SoftCraft.AppServices;

public class PropertyAppService : CrudAppService<Entities.Property, PropertyPartOutput, long, GetPropertyListInput,
    CreatePropertyInput, UpdatePropertyInput>, IPropertyAppService
{
    public PropertyAppService(IRepository<Entities.Property, long> repository) : base(repository)
    {
    }
}