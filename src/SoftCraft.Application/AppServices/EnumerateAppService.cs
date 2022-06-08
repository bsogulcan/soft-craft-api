using System.Collections.Generic;
using System.Threading.Tasks;
using SoftCraft.AppServices.Enumerate;
using SoftCraft.AppServices.Enumerate.Dtos;
using SoftCraft.Repositories;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SoftCraft.AppServices;

public class EnumerateAppService:CrudAppService<Entities.Enumerate,EnumeratePartOutput,long,GetEnumerateListInput,CreateEnumerateInput,UpdateEnumerateInput>,IEnumerateAppService
{
  
    
    public EnumerateAppService(IRepository<Entities.Enumerate, long> repository,IProjectRepository projectRepository) : base(repository)
    {
       
        
    }

    public override async Task<PagedResultDto<EnumeratePartOutput>> GetListAsync(GetEnumerateListInput input)
    {
        var enumerates = await Repository.GetListAsync(x => x.ProjectId == input.ProjectId);
        return new PagedResultDto<EnumeratePartOutput>()
        {
            Items = ObjectMapper.Map<List<Entities.Enumerate>, List<EnumeratePartOutput>>(enumerates),
            TotalCount = enumerates.Count
        };
    }
}