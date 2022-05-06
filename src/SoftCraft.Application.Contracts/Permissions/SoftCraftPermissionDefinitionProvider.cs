using SoftCraft.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace SoftCraft.Permissions;

public class SoftCraftPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(SoftCraftPermissions.GroupName);
        //Define your own permissions here. Example:
        //myGroup.AddPermission(SoftCraftPermissions.MyPermission1, L("Permission:MyPermission1"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<SoftCraftResource>(name);
    }
}
