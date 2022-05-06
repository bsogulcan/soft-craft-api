using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace SoftCraft;

[Dependency(ReplaceServices = true)]
public class SoftCraftBrandingProvider : DefaultBrandingProvider
{
    public override string AppName => "SoftCraft";
}
