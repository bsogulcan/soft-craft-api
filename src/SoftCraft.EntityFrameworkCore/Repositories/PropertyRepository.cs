using SoftCraft.Entities;
using SoftCraft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace SoftCraft.Repositories;

public class PropertyRepository : EfCoreRepository<SoftCraftDbContext, Property, long>, IPropertyRepository
{
    public PropertyRepository(IDbContextProvider<SoftCraftDbContext> dbContextProvider) : base(dbContextProvider)
    {
    }
}