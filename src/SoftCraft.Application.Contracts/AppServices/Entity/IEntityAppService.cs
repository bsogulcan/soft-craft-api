using System.Threading.Tasks;
using SoftCraft.AppServices.Entity.Dtos;
using SoftCraft.AppServices.GeneratedCodeResult;
using Volo.Abp.Application.Services;
using EntityDto = Volo.Abp.Application.Dtos.EntityDto;

namespace SoftCraft.AppServices.Entity;

public interface
    IEntityAppService : ICrudAppService<EntityPartOutput, long, GetEntityListInput, CreateEntityInput,
        UpdateEntityInput>
{
    Task<EntityCodeResultDto> GetCodeResult(long id);
}