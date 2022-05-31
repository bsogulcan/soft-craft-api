using SoftCraft.Entities;
using SoftCraft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace SoftCraft.Repositories;

public class EntityRepository : EfCoreRepository<SoftCraftDbContext, Entity, long>, IEntityRepository
{
    public EntityRepository(IDbContextProvider<SoftCraftDbContext> dbContextProvider) : base(dbContextProvider)
    {
    }
}