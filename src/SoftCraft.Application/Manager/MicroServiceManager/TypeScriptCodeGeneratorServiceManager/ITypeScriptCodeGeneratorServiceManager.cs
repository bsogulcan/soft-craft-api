using System.Threading.Tasks;
using ProjectManager;
using TypeScriptCodeGenerator;
using Volo.Abp.DependencyInjection;

namespace SoftCraft.Manager.MicroServiceManager.TypeScriptCodeGeneratorServiceManager;

public interface ITypeScriptCodeGeneratorServiceManager : ITransientDependency
{
    Task<DtoResult> CreateDtosAsync(Entities.Entity entity);
    Task<ServiceResult> CreateServiceAsync(Entities.Entity entity);
}