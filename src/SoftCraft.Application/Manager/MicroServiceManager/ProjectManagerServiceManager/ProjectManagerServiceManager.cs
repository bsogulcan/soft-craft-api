using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using ProjectManager;
using SoftCraft.Entities;
using SoftCraft.Enums;
using FileInfo = ProjectManager.FileInfo;

namespace SoftCraft.Manager.MicroServiceManager.ProjectManagerServiceManager;

public class ProjectManagerServiceManager : IProjectManagerServiceManager
{
    private readonly IConfiguration _configuration;

    public ProjectManagerServiceManager(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<ProjectReply> CreateAbpBoilerplateProjectAsync(long projectId, string uniqueName,
        LogType logType, bool multiTenant, string projectName)
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
            MultiTenant = multiTenant,
            DisplayName = projectName
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

    public async Task<ProjectReply> AddTypeScriptDtosToExistingProjectAsync(AddDtosRequest addDtosRequest)
    {
        using var projectManagerChannel =
            GrpcChannel.ForAddress(_configuration["MicroServices:ProjectManagerUrl"]);
        var projectManagerClient =
            new ProjectManager.ProjectManager.ProjectManagerClient(projectManagerChannel);
        var result = await projectManagerClient.AddTypeScriptDtosToExistingProjectAsync(addDtosRequest);
        return result;
    }

    public async Task<ProjectReply> AddTypeScriptServiceToExistingProjectAsync(
        AddTypeScriptServiceRequest addTypeScriptServiceRequest)
    {
        using var projectManagerChannel =
            GrpcChannel.ForAddress(_configuration["MicroServices:ProjectManagerUrl"]);
        var projectManagerClient =
            new ProjectManager.ProjectManager.ProjectManagerClient(projectManagerChannel);
        var result = await projectManagerClient.AddTypeScriptServiceToExistingProjectAsync(addTypeScriptServiceRequest);
        return result;
    }

    public async Task<FileStream> GetProjectZipFile(Project project)
    {
        using var projectManagerChannel =
            GrpcChannel.ForAddress(_configuration["MicroServices:ProjectManagerUrl"]);
        var projectManagerClient =
            new ProjectManager.ProjectManager.ProjectManagerClient(projectManagerChannel);

        var fileInfo = new FileInfo
        {
            ProjectId = project.Id,
            FileExtension = ".zip",
            FileName = project.UniqueName
        };

        FileStream fileStream = null;

        var request = projectManagerClient.FileDownLoad(fileInfo);

        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        int count = 0;
        decimal chunkSize = 0;

        var fileName =
            @$"C:\SoftCraft\DownloadableProjects\{project.Id}-{project.UniqueName}.zip";

        if (File.Exists(fileName))
        {
            File.Delete(fileName);
        }


        while (await request.ResponseStream.MoveNext(cancellationTokenSource.Token))
        {
            if (count++ == 0)
            {
                fileStream =
                    new FileStream(
                        fileName,
                        FileMode.CreateNew);

                //Depolanacak yerde dosya boyutu kadar alan tahsis ediliyor.
                fileStream.SetLength(request.ResponseStream.Current.FileSize);
            }

            var buffer = request.ResponseStream.Current.Buffer.ToByteArray();

            await fileStream.WriteAsync(buffer, 0, request.ResponseStream.Current.ReadedByte);

            Console.WriteLine(
                $"{Math.Round(((chunkSize += request.ResponseStream.Current.ReadedByte) * 100) / request.ResponseStream.Current.FileSize)}%");
        }

        Console.WriteLine("Yüklendi...");

        await fileStream.DisposeAsync();
        fileStream.Close();

        return fileStream;
    }

    public async Task<ProjectReply> AddTypeScriptEnumToExistingProjectAsync(AddEnumRequest addEnumRequest)
    {
        using var projectManagerChannel =
            GrpcChannel.ForAddress(_configuration["MicroServices:ProjectManagerUrl"]);
        var projectManagerClient =
            new ProjectManager.ProjectManager.ProjectManagerClient(projectManagerChannel);
        var result = await projectManagerClient.AddTypeScriptEnumToExistingProjectAsync(addEnumRequest);
        return result;
    }

    public async Task<ProjectReply> AddTypeScriptComponentsToExistingProjectAsync(ComponentResult componentResult)
    {
        using var projectManagerChannel =
            GrpcChannel.ForAddress(_configuration["MicroServices:ProjectManagerUrl"]);
        var projectManagerClient =
            new ProjectManager.ProjectManager.ProjectManagerClient(projectManagerChannel);
        var result = await projectManagerClient.AddTypeScriptComponentsToExistingProjectAsync(componentResult);
        return result;
    }

    public async Task<ProjectReply> AddNavigationToExistingProjectAsync(AddStringToExistingProject input)
    {
        using var projectManagerChannel =
            GrpcChannel.ForAddress(_configuration["MicroServices:ProjectManagerUrl"]);
        var projectManagerClient =
            new ProjectManager.ProjectManager.ProjectManagerClient(projectManagerChannel);
        var result = await projectManagerClient.AddNavigationToExistingProjectAsync(input);
        return result;
    }
}