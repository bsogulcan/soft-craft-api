using System.Collections.Generic;
using System.Threading.Tasks;
using SoftCraft.AppServices.Navigations.Dtos;
using Volo.Abp.Application.Services;

namespace SoftCraft.AppServices.Navigations;

public interface INavigationAppService : ICrudAppService<NavigationFullOutput, long, GetNavigationListInput,
    CreateNavigationInput, UpdateNavigationInput>
{
    Task ReOrderAsync(List<NavigationPartOutput> navigations);
}