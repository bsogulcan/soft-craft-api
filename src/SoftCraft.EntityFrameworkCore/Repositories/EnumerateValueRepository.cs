using SoftCraft.Entities;
using SoftCraft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace SoftCraft.Repositories;

public class EnumerateValueRepository:EfCoreRepository<SoftCraftDbContext,EnumerateValue,long>,IEnumerateValueRepository
{
    public EnumerateValueRepository(IDbContextProvider<SoftCraftDbContext> dbContextProvider) : base(dbContextProvider)
    {
    }
}