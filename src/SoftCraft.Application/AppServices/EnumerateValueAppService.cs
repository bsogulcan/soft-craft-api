using System.Collections.Generic;
using System.Threading.Tasks;
using SoftCraft.AppServices.EntityValue;
using SoftCraft.AppServices.EntityValue.Dtos;
using SoftCraft.AppServices.Enumerate.Dtos;
using SoftCraft.Entities;
using SoftCraft.Repositories;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SoftCraft.AppServices;

public class EnumerateValueAppService:CrudAppService<
    Entities.EnumerateValue,
    EnumerateValuePartOutput,
    long,GetEnumerateValueListInput,CreateEnumerateValueInput,UpdateEnumerateValueInput>,IEnumerateValueAppService
{
    public EnumerateValueAppService(IRepository<EnumerateValue, long> repository) : base(repository)
    {
    }

    /*    
    public async Task<PagedResultDto<EnumerateValuePartOutput>> GetListAsync(GetEnumerateValueListInput input)
    {
        var enumerateValues = await Repository.GetListAsync(x => x.EnumerateId == input.EnumerateId);
        return new PagedResultDto<EnumerateValuePartOutput>()
        {
            Items = ObjectMapper.Map<List<Entities.EnumerateValue>, List<EnumerateValuePartOutput>>(enumerateValues),
            TotalCount = enumerateValues.Count
        };
    }*/
}