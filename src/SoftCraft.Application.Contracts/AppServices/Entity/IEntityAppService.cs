using SoftCraft.AppServices.Entity.Dtos;
using Volo.Abp.Application.Services;
using EntityDto = Volo.Abp.Application.Dtos.EntityDto;

namespace SoftCraft.AppServices.Entity;

public interface
    IEntityAppService : ICrudAppService<EntityDto, long, GetEntityListInput, CreateEntityInput, UpdateEntityInput>
{
}