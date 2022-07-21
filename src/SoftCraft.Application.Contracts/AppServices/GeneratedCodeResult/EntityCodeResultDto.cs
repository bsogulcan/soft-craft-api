using SoftCraft.AppServices.GeneratedCodeResult.Dtos;

namespace SoftCraft.AppServices.GeneratedCodeResult;

public class EntityCodeResultDto
{
    public EntityCodeResultDto()
    {
        DtoResult = new DtoResultDto();
        RepositoryResult = new RepositoryResultDto();
        AppServiceResult = new AppServiceResultDto();
        TypeScriptDtoResult = new TypeScriptDtoResultDto();
        TypeScriptComponentResult = new TypeScriptComponentResultDto();
        TypeScriptCreateComponentResult = new TypeScriptComponentResultDto();
        TypeScriptEditComponentResult = new TypeScriptComponentResultDto();
    }

    public long EntityId { get; set; }
    public string EntityName { get; set; }
    public string EntityResult { get; set; }
    public DtoResultDto DtoResult { get; set; }
    public RepositoryResultDto RepositoryResult { get; set; }
    public AppServiceResultDto AppServiceResult { get; set; }
    public string ConfigurationResult { get; set; }
    public TypeScriptDtoResultDto TypeScriptDtoResult { get; set; }
    public string TypeScriptServiceResult { get; set; }
    public TypeScriptComponentResultDto TypeScriptComponentResult { get; set; }
    public TypeScriptComponentResultDto TypeScriptCreateComponentResult { get; set; }
    public TypeScriptComponentResultDto TypeScriptEditComponentResult { get; set; }
}