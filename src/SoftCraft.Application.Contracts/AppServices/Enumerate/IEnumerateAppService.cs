using SoftCraft.AppServices.Enumerate.Dtos;
using Volo.Abp.Application.Services;

namespace SoftCraft.AppServices.Enumerate;

public interface IEnumerateAppService:ICrudAppService<EnumerateFullOutput, long, GetEnumerateListInput, CreateEnumerateInput, UpdateEnumerateInput>
{
    
}