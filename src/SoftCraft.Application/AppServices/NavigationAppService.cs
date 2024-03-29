﻿using System.Collections.Generic;
using System.Threading.Tasks;
using SoftCraft.AppServices.Navigations;
using SoftCraft.AppServices.Navigations.Dtos;
using SoftCraft.AppServices.Property.Dtos;
using SoftCraft.Entities;
using SoftCraft.Manager.MicroServiceManager.TypeScriptCodeGeneratorServiceManager;
using SoftCraft.Repositories;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SoftCraft.AppServices;

public class NavigationAppService : CrudAppService<Navigation, NavigationFullOutput, long, GetNavigationListInput,
    CreateNavigationInput, UpdateNavigationInput>, INavigationAppService
{
    private readonly ITypeScriptCodeGeneratorServiceManager _typeScriptCodeGeneratorServiceManager;

    public NavigationAppService(INavigationRepository repository,
        ITypeScriptCodeGeneratorServiceManager typeScriptCodeGeneratorServiceManager) : base(repository)
    {
        _typeScriptCodeGeneratorServiceManager = typeScriptCodeGeneratorServiceManager;
    }

    public override Task<NavigationFullOutput> GetAsync(long id)
    {
        return base.GetAsync(id);
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

    public async Task ReOrderAsync(List<NavigationPartOutput> navigations)
    {
        foreach (var navigationDto in navigations)
        {
            var navigation = await Repository.GetAsync(navigationDto.Id);
            navigation.Index = navigations.IndexOf(navigationDto);
            await Repository.UpdateAsync(navigation);
        }
    }
}