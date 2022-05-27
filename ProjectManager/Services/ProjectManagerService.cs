using Grpc.Core;
using ProjectManager;

namespace ProjectManager.Services;

public class ProjectManagerService : ProjectManager.ProjectManagerBase
{
    private readonly ILogger<ProjectManagerService> _logger;
    private readonly IConfiguration _configuration;

    public ProjectManagerService(ILogger<ProjectManagerService> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;

        if (!Directory.Exists(configuration["ProjectsFolderPath"]))
        {
            Directory.CreateDirectory(configuration["ProjectsFolderPath"]);
        }
    }

    public override async Task<ProjectReply> CreateAbpBoilerplateProject(ProjectRequest request,
        ServerCallContext context)
    {
        var baseAbpBoilerPlateProjectFilePath =
            Path.Combine(_configuration["BaseProjectsFolderPath"], "BaseAbpBoilerplateProject");
        if (!Directory.Exists(baseAbpBoilerPlateProjectFilePath))
        {
            throw new Exception("BaseAbpBoilerplateProject cannot be found!");
        }

        var projectFile = Path.Combine(_configuration["ProjectsFolderPath"], request.Id);

        if (Directory.Exists(projectFile))
        {
            throw new Exception("Base project all ready created");
        }

        Directory.CreateDirectory(projectFile);

        File.Copy(baseAbpBoilerPlateProjectFilePath + ".zip", projectFile + "/BaseAbpBoilerplateProject.zip");

        System.IO.Compression.ZipFile.ExtractToDirectory(projectFile + "/BaseAbpBoilerplateProject.zip", projectFile);
        
        File.Delete(projectFile + "/BaseAbpBoilerplateProject.zip");

        return new ProjectReply()
        {
            Id = request.Id
        };
    }
}