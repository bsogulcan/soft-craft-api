using System.Collections.Generic;
using System.Threading.Tasks;
using ProjectManager;
using SoftCraft.Entities;
using TypeScriptCodeGenerator;
using Volo.Abp.DependencyInjection;
using ComboBoxWrapper = SoftCraft.Manager.MicroServiceManager.Helpers.Modals.ComboBoxWrapper;
using ComponentResult = TypeScriptCodeGenerator.ComponentResult;
using Entity = SoftCraft.Entities.Entity;

namespace SoftCraft.Manager.MicroServiceManager.TypeScriptCodeGeneratorServiceManager;

public interface ITypeScriptCodeGeneratorServiceManager : ITransientDependency
{
    Task<DtoResult> CreateDtosAsync(Entities.Entity entity);
    Task<ServiceResult> CreateServiceAsync(Entities.Entity entity);
    Task<StringifyResult> CreateEnumAsync(Enumerate enumerate);
    Task<ComponentResult> CreateComponentsAsync(Entity entity, List<ComboBoxWrapper> comboBoxWrappers);
    Task<StringifyResult> CreateNavigationItems(List<Navigation> navigations);
}