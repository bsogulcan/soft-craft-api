using SoftCraft.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace SoftCraft.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class SoftCraftController : AbpControllerBase
{
    protected SoftCraftController()
    {
        LocalizationResource = typeof(SoftCraftResource);
    }
}