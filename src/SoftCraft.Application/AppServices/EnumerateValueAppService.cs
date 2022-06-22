using SoftCraft.AppServices.EnumerateValue;
using SoftCraft.AppServices.EnumerateValue.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SoftCraft.AppServices;

public class EnumerateValueAppService : CrudAppService<
    Entities.EnumerateValue,
    EnumerateValuePartOutput,
    long, GetEnumerateValueListInput, CreateEnumerateValueInput, UpdateEnumerateValueInput>, IEnumerateValueAppService
{
    public EnumerateValueAppService(IRepository<Entities.EnumerateValue, long> repository) : base(repository)
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