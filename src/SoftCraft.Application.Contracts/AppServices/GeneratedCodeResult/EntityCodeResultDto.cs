using SoftCraft.AppServices.GeneratedCodeResult.Dtos;

namespace SoftCraft.AppServices.GeneratedCodeResult;

public class EntityCodeResultDto
{
    public EntityCodeResultDto()
    {
        DtoResult = new DtoResultDto();
        RepositoryResult = new RepositoryResultDto();
        AppServiceResult = new AppServiceResultDto();
    }

    public long EntityId { get; set; }
    public string EntityName { get; set; }
    public string EntityResult { get; set; }
    public DtoResultDto DtoResult { get; set; }
    public RepositoryResultDto RepositoryResult { get; set; }
    public AppServiceResultDto AppServiceResult { get; set; }
    public string ConfigurationResult { get; set; }
}