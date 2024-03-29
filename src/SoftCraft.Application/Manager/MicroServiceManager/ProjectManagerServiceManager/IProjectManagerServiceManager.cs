﻿using System.IO;
using System.Threading.Tasks;
using ProjectManager;
using SoftCraft.Entities;
using SoftCraft.Enums;
using Volo.Abp.DependencyInjection;

namespace SoftCraft.Manager.MicroServiceManager.ProjectManagerServiceManager;

public interface IProjectManagerServiceManager : ITransientDependency
{
    Task<ProjectReply> CreateAbpBoilerplateProjectAsync(long projectId, string uniqueName,
        LogType logType, bool multiTenant, string projectName);

    Task<ProjectReply> AddEntityToExistingProjectAsync(AddEntityRequest addEntityRequest);
    Task<ProjectReply> AddConfigurationToExistingProjectAsync(AddEntityRequest addEntityRequest);
    Task<ProjectReply> AddRepositoryToExistingProjectAsync(AddRepositoryRequest addRepositoryRequest);
    Task<ProjectReply> AddDtosToExistingProjectAsync(AddDtosRequest addDtosRequest);
    Task<ProjectReply> AddAppServiceToExistingProjectAsync(AddAppServiceRequest addAppServiceRequest);
    Task<ProjectReply> AddEnumToExistingProjectAsync(AddEnumRequest addEnumRequest);
    Task<ProjectReply> AddTypeScriptDtosToExistingProjectAsync(AddDtosRequest addDtosRequest);

    Task<ProjectReply> AddTypeScriptServiceToExistingProjectAsync(
        AddTypeScriptServiceRequest addTypeScriptServiceRequest);

    Task<FileStream> GetProjectZipFile(Project project);
    Task<ProjectReply> AddTypeScriptEnumToExistingProjectAsync(AddEnumRequest addEnumRequest);
    Task<ProjectReply> AddTypeScriptComponentsToExistingProjectAsync(ComponentResult componentResult);
    Task<ProjectReply> AddNavigationToExistingProjectAsync(AddStringToExistingProject input);

    Task<ProjectReply> AddDefaultAbpConfigurationToExistingProject(AddEntityRequest addEntityRequest);
    Task<ProjectReply> AddEntityPropertiesToExistingProject(AddEntityRequest addEntityRequest);

}