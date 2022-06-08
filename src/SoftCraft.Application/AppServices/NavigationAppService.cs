using System.Collections.Generic;
using System.Threading.Tasks;
using SoftCraft.AppServices.Navigations;
using SoftCraft.AppServices.Navigations.Dtos;
using SoftCraft.AppServices.Property.Dtos;
using SoftCraft.Entities;
using SoftCraft.Repositories;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SoftCraft.AppServices;

public class NavigationAppService : CrudAppService<Navigation, NavigationFullOutput, long, GetNavigationListInput,
    CreateNavigationInput, UpdateNavigationInput>, INavigationAppService
{
    public NavigationAppService(INavigationRepository repository) : base(repository)
    {
    }

    public override async Task<PagedResultDto<NavigationFullOutput>> GetListAsync(GetNavigationListInput input)
    {
        var navigations = await Repository.GetListAsync(x => x.ProjectId == input.ProjectId);
        return new PagedResultDto<NavigationFullOutput>()
        {
            Items = ObjectMapper.Map<List<Navigation>, List<NavigationFullOutput>>(navigations),
            TotalCount = navigations.Count
        };
    }
}