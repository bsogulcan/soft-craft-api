using SoftCraft.AppServices.Property.Dtos;
using Volo.Abp.Application.Services;

namespace SoftCraft.AppServices.Property;

public interface IPropertyAppService : ICrudAppService<PropertyFullOutput, long, GetPropertyListInput,
    CreatePropertyInput,
    UpdatePropertyInput>
{
}