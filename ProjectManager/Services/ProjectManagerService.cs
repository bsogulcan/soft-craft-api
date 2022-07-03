using System.Data.Common;
using System.IO.Compression;
using System.Management.Automation;
using System.Text;
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

        var webHostFolderPath = Path.Combine(projectFile, "aspnet-core", "src",
            request.Name + ".Web.Host");

        var coreFolderPath = Path.Combine(projectFile, "aspnet-core", "src",
            request.Name + ".Core");

        if (request.LogManagement == LogManagement.ElasticSearch)
        {
            await File.WriteAllTextAsync(Path.Combine(webHostFolderPath, "log4net.config"),
                HelperClass.HelperClass.GetElasticSearchConfiguration(request.Name));
            await File.WriteAllTextAsync(Path.Combine(webHostFolderPath, "log4net.Production.config"),
                HelperClass.HelperClass.GetElasticSearchConfiguration(request.Name));
        }

        if (!request.MultiTenant)
        {
            var constFileText = await File.ReadAllTextAsync(Path.Combine(coreFolderPath, request.Name + "Consts.cs"));
            var replacedConstFileText =
                constFileText.Replace("MultiTenancyEnabled = true", "MultiTenancyEnabled = false");
            await File.WriteAllTextAsync(Path.Combine(coreFolderPath, request.Name + "Consts.cs"),
                replacedConstFileText);
        }

        return new ProjectReply()
        {
            Id = request.Id
        };
    }

    public override async Task<ProjectReply> AddEntityToExistingProject(AddEntityRequest request,
        ServerCallContext context)
    {
        var projectFolderPath = Path.Combine(_configuration["ProjectsFolderPath"], request.Id);
        var entityFolderPath = Path.Combine(projectFolderPath,
            $"aspnet-core\\src\\{request.ProjectName}.Core\\Domain\\Entities");

        if (!Directory.Exists(entityFolderPath))
        {
            Directory.CreateDirectory(entityFolderPath);
        }

        await File.WriteAllTextAsync(Path.Combine(entityFolderPath, request.EntityName + ".cs"), request.Stringified);

        var dbContextFilePath = Path.Combine(projectFolderPath,
            $"aspnet-core\\src\\{request.ProjectName}.EntityFrameworkCore\\EntityFrameworkCore\\{request.ProjectName}DbContext.cs");

        var dbContext = await HelperClass.HelperClass.AddEntitiesNamespace(dbContextFilePath, request.ProjectName);
        await File.WriteAllTextAsync(dbContextFilePath, dbContext.ToString());
        dbContext = await HelperClass.HelperClass.AddEntityToDbContext(dbContextFilePath, request.ProjectName,
            request.EntityName);
        await File.WriteAllTextAsync(dbContextFilePath, dbContext.ToString());

        return new ProjectReply()
        {
            Id = request.Id
        };
    }

    public override async Task<ProjectReply> AddRepositoryToExistingProject(AddRepositoryRequest request,
        ServerCallContext context)
    {
        var projectFolderPath = Path.Combine(_configuration["ProjectsFolderPath"], request.Id);
        var repositoriesFolderPath = Path.Combine(projectFolderPath,
            $"aspnet-core\\src\\{request.ProjectName}.EntityFrameworkCore\\EntityFrameworkCore\\Repositories\\Extensions\\{request.EntityName}");

        if (!Directory.Exists(repositoriesFolderPath))
        {
            Directory.CreateDirectory(repositoriesFolderPath);
        }

        await File.WriteAllTextAsync(
            Path.Combine(repositoriesFolderPath, "I" + request.EntityName + "Repository" + ".cs"),
            request.StringifiedRepositoryInterface);
        await File.WriteAllTextAsync(Path.Combine(repositoriesFolderPath, request.EntityName + "Repository" + ".cs"),
            request.StringifiedRepository);

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