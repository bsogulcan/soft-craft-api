using System.Threading.Tasks;

namespace SoftCraft.Data;

public interface ISoftCraftDbSchemaMigrator
{
    Task MigrateAsync();
}
