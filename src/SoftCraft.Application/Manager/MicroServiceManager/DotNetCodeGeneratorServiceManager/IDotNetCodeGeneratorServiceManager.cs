using System.Threading.Tasks;
using DotNetCodeGenerator;
using SoftCraft.Entities;
using Volo.Abp.DependencyInjection;
using Entity = SoftCraft.Entities.Entity;

namespace SoftCraft.Manager.MicroServiceManager.DotNetCodeGeneratorServiceManager;

public interface IDotNetCodeGeneratorServiceManager : ITransientDependency
{
    Task<EntityResult> CreateEntityAsync(Entity entity);
    Task<EntityResult> CreateConfigurationAsync(Entity entity);
    Task<EntityResult> CreateRepositoryInterfaceAsync(Entity entity);
    Task<EntityResult> CreateRepositoryAsync(Entity entity);
    Task<DtoResult> CreateDtosAsync(Entity entity);
    Task<AppServiceResult> CreateAppServiceAsync(AppServiceRequest appServiceRequest);
    Task<EntityResult> CreateEnumAsync(Enumerate enumerate);
    DotNetCodeGenerator.Entity EntityToGeneratorEntity(Entity entity);
    Task<EntityResult> CreateDefaultAbpConfiguration(Entity entity);
    Task<EntityResult> CreateProperties(Entity entity);
}