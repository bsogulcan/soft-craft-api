using Volo.Abp.Modularity;

namespace SoftCraft;

[DependsOn(
    typeof(SoftCraftApplicationModule),
    typeof(SoftCraftDomainTestModule)
    )]
public class SoftCraftApplicationTestModule : AbpModule
{

}
