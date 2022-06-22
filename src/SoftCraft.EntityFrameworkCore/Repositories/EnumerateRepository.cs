using SoftCraft.Entities;
using SoftCraft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace SoftCraft.Repositories;

public class EnumerateRepository:EfCoreRepository<SoftCraftDbContext, Enumerate, long>, IEnumerateRepository
{
    public EnumerateRepository(IDbContextProvider<SoftCraftDbContext> dbContextProvider) : base(dbContextProvider)
    {
    }
}