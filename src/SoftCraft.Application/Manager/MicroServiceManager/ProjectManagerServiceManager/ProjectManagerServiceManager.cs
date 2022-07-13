using System.Threading.Tasks;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using ProjectManager;
using SoftCraft.Entities;
using SoftCraft.Enums;

namespace SoftCraft.Manager.MicroServiceManager.ProjectManagerServiceManager;

public class ProjectManagerServiceManager : IProjectManagerServiceManager
{
    private readonly IConfiguration _configuration;

    public ProjectManagerServiceManager(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<ProjectReply> CreateAbpBoilerplateProjectAsync(long projectId, string uniqueName,
        LogType logType, bool multiTenant)
    {
        using var projectManagerChannel =
            GrpcChannel.ForAddress(_configuration["MicroServices:ProjectManagerUrl"]);
        var projectManagerClient =
            new ProjectManager.ProjectManager.ProjectManagerClient(projectManagerChannel);

        var result = await projectManagerClient.CreateAbpBoilerplateProjectAsync(new ProjectRequest()
        {
            Id = projectId.ToString(),
            Name = uniqueName,
            LogManagement = (LogManagement) logType,
            MultiTenant = multiTenant
        });

        return result;
    }

    public async Task<ProjectReply> AddEntityToExistingProjectAsync(AddEntityRequest addEntityRequest)
    {
        using var projectManagerChannel =
            GrpcChannel.ForAddress(_configuration["MicroServices:ProjectManagerUrl"]);
        var projectManagerClient =
            new ProjectManager.ProjectManager.ProjectManagerClient(projectManagerChannel);
        var result = await projectManagerClient.AddEntityToExistingProjectAsync(addEntityRequest);
        return result;
    }

    public async Task<ProjectReply> AddConfigurationToExistingProjectAsync(AddEntityRequest addEntityRequest)
    {
        using var projectManagerChannel =
            GrpcChannel.ForAddress(_configuration["MicroServices:ProjectManagerUrl"]);
        var projectManagerClient =
            new ProjectManager.ProjectManager.ProjectManagerClient(projectManagerChannel);
        var result = await projectManagerClient.AddConfigurationToExistingProjectAsync(addEntityRequest);
        return result;
    }

    public async Task<ProjectReply> AddRepositoryToExistingProjectAsync(AddRepositoryRequest addRepositoryRequest)
    {
        using var projectManagerChannel =
            GrpcChannel.ForAddress(_configuration["MicroServices:ProjectManagerUrl"]);
        var projectManagerClient =
            new ProjectManager.ProjectManager.ProjectManagerClient(projectManagerChannel);
        var result = await projectManagerClient.AddRepositoryToExistingProjectAsync(addRepositoryRequest);
        return result;
    }

    public async Task<ProjectReply> AddDtosToExistingProjectAsync(AddDtosRequest addDtosRequest)
    {
        using var projectManagerChannel =
            GrpcChannel.ForAddress(_configuration["MicroServices:ProjectManagerUrl"]);
        var projectManagerClient =
            new ProjectManager.ProjectManager.ProjectManagerClient(projectManagerChannel);
        var result = await projectManagerClient.AddDtosToExistingProjectAsync(addDtosRequest);
        return result;
    }

    public async Task<ProjectReply> AddAppServiceToExistingProjectAsync(AddAppServiceRequest addAppServiceRequest)
    {
        using var projectManagerChannel =
            GrpcChannel.ForAddress(_configuration["MicroServices:ProjectManagerUrl"]);
        var projectManagerClient =
            new ProjectManager.ProjectManager.ProjectManagerClient(projectManagerChannel);
        var result = await projectManagerClient.AddAppServiceToExistingProjectAsync(addAppServiceRequest);
        return result;
    }

    public async Task<ProjectReply> AddEnumToExistingProjectAsync(AddEnumRequest addEnumRequest)
    {
        using var projectManagerChannel =
            GrpcChannel.ForAddress(_configuration["MicroServices:ProjectManagerUrl"]);
        var projectManagerClient =
            new ProjectManager.ProjectManager.ProjectManagerClient(projectManagerChannel);
        var result = await projectManagerClient.AddEnumToExistingProjectAsync(addEnumRequest);
        return result;
    }
}