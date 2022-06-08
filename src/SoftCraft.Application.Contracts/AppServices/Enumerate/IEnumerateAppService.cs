using SoftCraft.AppServices.Enumerate.Dtos;
using Volo.Abp.Application.Services;

namespace SoftCraft.AppServices.Enumerate;

public interface IEnumerateAppService:ICrudAppService<EnumeratePartOutput, long, GetEnumerateListInput, CreateEnumerateInput, UpdateEnumerateInput>
{
    
}