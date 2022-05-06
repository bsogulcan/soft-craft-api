using SoftCraft.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace SoftCraft;

[DependsOn(
    typeof(SoftCraftEntityFrameworkCoreTestModule)
    )]
public class SoftCraftDomainTestModule : AbpModule
{

}
