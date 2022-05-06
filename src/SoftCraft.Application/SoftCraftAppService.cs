using System;
using System.Collections.Generic;
using System.Text;
using SoftCraft.Localization;
using Volo.Abp.Application.Services;

namespace SoftCraft;

/* Inherit your application services from this class.
 */
public abstract class SoftCraftAppService : ApplicationService
{
    protected SoftCraftAppService()
    {
        LocalizationResource = typeof(SoftCraftResource);
    }
}
