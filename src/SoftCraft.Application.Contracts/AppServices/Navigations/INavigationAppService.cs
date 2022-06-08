using SoftCraft.AppServices.Navigations.Dtos;
using Volo.Abp.Application.Services;

namespace SoftCraft.AppServices.Navigations;

public interface INavigationAppService : ICrudAppService<NavigationFullOutput, long, GetNavigationListInput,
    CreateNavigationInput, UpdateNavigationInput>
{
}