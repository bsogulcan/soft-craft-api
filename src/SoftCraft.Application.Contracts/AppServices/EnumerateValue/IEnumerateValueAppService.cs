using SoftCraft.AppServices.EnumerateValue.Dtos;
using Volo.Abp.Application.Services;

namespace SoftCraft.AppServices.EnumerateValue;

public interface IEnumerateValueAppService:ICrudAppService<EnumerateValuePartOutput, long, GetEnumerateValueListInput, CreateEnumerateValueInput, UpdateEnumerateValueInput>
{
    
}