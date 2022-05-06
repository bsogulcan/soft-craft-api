using SoftCraft.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Modularity;

namespace SoftCraft.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(SoftCraftEntityFrameworkCoreModule),
    typeof(SoftCraftApplicationContractsModule)
    )]
public class SoftCraftDbMigratorModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpBackgroundJobOptions>(options => options.IsJobExecutionEnabled = false);
    }
}
