using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace SoftCraft.Data;

/* This is used if database provider does't define
 * ISoftCraftDbSchemaMigrator implementation.
 */
public class NullSoftCraftDbSchemaMigrator : ISoftCraftDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
