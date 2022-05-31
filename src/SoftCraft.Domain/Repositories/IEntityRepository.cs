using SoftCraft.Entities;
using Volo.Abp.Domain.Repositories;

namespace SoftCraft.Repositories;

public interface IEntityRepository : IRepository<Entity, long>
{
}