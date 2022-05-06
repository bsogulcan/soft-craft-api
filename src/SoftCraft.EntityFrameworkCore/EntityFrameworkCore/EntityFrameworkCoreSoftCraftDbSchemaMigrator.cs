using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SoftCraft.Data;
using Volo.Abp.DependencyInjection;

namespace SoftCraft.EntityFrameworkCore;

public class EntityFrameworkCoreSoftCraftDbSchemaMigrator
    : ISoftCraftDbSchemaMigrator, ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;

    public EntityFrameworkCoreSoftCraftDbSchemaMigrator(
        IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task MigrateAsync()
    {
        /* We intentionally resolving the SoftCraftDbContext
         * from IServiceProvider (instead of directly injecting it)
         * to properly get the connection string of the current tenant in the
         * current scope.
         */

        await _serviceProvider
            .GetRequiredService<SoftCraftDbContext>()
            .Database
            .MigrateAsync();
    }
}
