using Volo.Abp.Settings;

namespace SoftCraft.Settings;

public class SoftCraftSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        //Define your own settings here. Example:
        //context.Add(new SettingDefinition(SoftCraftSettings.MySetting1));
    }
}
