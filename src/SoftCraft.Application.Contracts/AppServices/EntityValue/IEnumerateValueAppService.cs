using SoftCraft.AppServices.EntityValue.Dtos;
using SoftCraft.AppServices.Enumerate.Dtos;
using Volo.Abp.Application.Services;

namespace SoftCraft.AppServices.EntityValue;

public interface IEnumerateValueAppService:ICrudAppService<EnumerateValuePartOutput, long, GetEnumerateValueListInput, CreateEnumerateValueInput, UpdateEnumerateValueInput>
{
    
}