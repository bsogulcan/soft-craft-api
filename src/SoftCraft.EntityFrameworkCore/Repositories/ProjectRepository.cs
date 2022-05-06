using SoftCraft.Entities;
using SoftCraft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace SoftCraft.Repositories;

public class ProjectRepository : EfCoreRepository<SoftCraftDbContext, Project, long>, IProjectRepository
{
    public ProjectRepository(IDbContextProvider<SoftCraftDbContext> dbContextProvider) : base(dbContextProvider)
    {
    }
}