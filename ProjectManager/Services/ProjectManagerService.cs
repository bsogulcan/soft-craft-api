using System.IO.Compression;
using System.Management.Automation;
using Grpc.Core;

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
        ZipFile.ExtractToDirectory(projectFile + "/BaseAbpBoilerplateProject.zip", projectFile);
        File.Delete(projectFile + "/BaseAbpBoilerplateProject.zip");

        var renamePsScript = await File.ReadAllTextAsync(Path.Combine(projectFile, "rename.ps1"));
        var replacedNamePsScript = renamePsScript.Replace("{{NewProjectName}}", request.Name);
        await File.WriteAllTextAsync(Path.Combine(projectFile, "rename.ps1"), replacedNamePsScript);

        //Running rename.ps1 script for changing BaseAbpBoilerplateProject
        await RunScript(replacedNamePsScript, Path.Combine(projectFile));

        return new ProjectReply()
        {
            Id = request.Id
        };
    }

    private async Task RunScript(string scriptContents, string folderPath)
    {
        using var ps = PowerShell.Create();
        ps.AddCommand("Set-Location").AddParameter("Path", folderPath + @"\");
        ps.AddScript(scriptContents);
        var pipelineObjects = await ps.InvokeAsync().ConfigureAwait(false);
        // foreach (var item in pipelineObjects)
        // {
        //     Console.WriteLine(item.BaseObject.ToString());
        // }
    }
}