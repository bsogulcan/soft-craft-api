using System.IO.Compression;
using System.Management.Automation;
using System.Text;
using Extensions;
using Google.Protobuf;
using Grpc.Core;
using Humanizer;
using ProjectManager.HelperClass;

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
            RecursiveDelete(new DirectoryInfo(projectFile));
            //throw new Exception("Base project all ready created");
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

        var applicationFolderPath = Path.Combine(projectFile, "aspnet-core", "src",
            request.Name + ".Application");
        var applicationCsProjContent =
            await File.ReadAllTextAsync(Path.Combine(applicationFolderPath, request.Name + ".Application.csproj"));

        var applicationCsProjContentStringBuild = new StringBuilder(applicationCsProjContent);
        var coreCsProjString = $"<ProjectReference Include=\"..\\{request.Name}.Core\\{request.Name}.Core.csproj\" />";
        var projectReferenceIndex =
            applicationCsProjContent.IndexOf(coreCsProjString, StringComparison.Ordinal);


        applicationCsProjContentStringBuild.Insert(projectReferenceIndex + coreCsProjString.Length,
            $"\r\n    <ProjectReference Include=\"..\\{request.Name}.EntityFrameworkCore\\{request.Name}.EntityFrameworkCore.csproj\" />");

        await File.WriteAllTextAsync(Path.Combine(applicationFolderPath, request.Name + ".Application.csproj"),
            applicationCsProjContentStringBuild.ToString());

        await RenameAngularProjectName(projectFile, request);
        return new ProjectReply()
        {
            Id = request.Id
        };
    }

    private async Task RenameAngularProjectName(string projectFile, ProjectRequest projectName)
    {
        var angularProjectFolder = Path.Combine(projectFile, "angular");

        var accountHeaderFilePath =
            Path.Combine(angularProjectFolder, "src", "account", "layout", "account-header.component.html");
        var accountHeaderFileContent = await File.ReadAllTextAsync(accountHeaderFilePath);
        await File.WriteAllTextAsync(accountHeaderFilePath,
            accountHeaderFileContent.Replace("BaseAbpBoilerplateProject", projectName.DisplayName));

        var footerFilePath =
            Path.Combine(angularProjectFolder, "src", "app", "layout", "footer.component.html");
        var footerFileContent = await File.ReadAllTextAsync(footerFilePath);
        await File.WriteAllTextAsync(footerFilePath,
            footerFileContent.Replace("BaseAbpBoilerplateProject", projectName.DisplayName));

        var sidebarFilePath = Path.Combine(angularProjectFolder, "src", "app", "layout", "sidebar-logo.component.html");
        var sidebarFileContent = await File.ReadAllTextAsync(sidebarFilePath);
        await File.WriteAllTextAsync(sidebarFilePath,
            sidebarFileContent.Replace("BaseAbpBoilerplateProject", projectName.DisplayName));

        var indexFilePath = Path.Combine(angularProjectFolder, "src", "index.html");
        var indexFileContent = await File.ReadAllTextAsync(indexFilePath);
        await File.WriteAllTextAsync(indexFilePath,
            indexFileContent.Replace("BaseAbpBoilerplateProject", projectName.DisplayName));
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

    public override async Task<ProjectReply> AddEnumToExistingProject(AddEnumRequest request, ServerCallContext context)
    {
        var projectFolderPath = Path.Combine(_configuration["ProjectsFolderPath"], request.Id);
        var enumFolderPath = Path.Combine(projectFolderPath,
            $"aspnet-core\\src\\{request.ProjectName}.Core\\Domain\\EntityHelper");

        if (!Directory.Exists(enumFolderPath))
        {
            Directory.CreateDirectory(enumFolderPath);
        }

        await File.WriteAllTextAsync(
            Path.Combine(enumFolderPath, request.EnumName + ".cs"),
            request.Stringified);

        return new ProjectReply()
        {
            Id = request.Id
        };
    }

    public override async Task<ProjectReply> AddDtosToExistingProject(AddDtosRequest request, ServerCallContext context)
    {
        var projectFolderPath = Path.Combine(_configuration["ProjectsFolderPath"], request.Id);
        var dtosFolderPath = Path.Combine(projectFolderPath,
            $"aspnet-core\\src\\{request.ProjectName}.Application\\Domain\\{request.EntityName}\\Dtos");

        if (!Directory.Exists(dtosFolderPath))
        {
            Directory.CreateDirectory(dtosFolderPath);
        }

        await File.WriteAllTextAsync(
            Path.Combine(dtosFolderPath, request.EntityName + "FullOutput.cs"),
            request.FullOutputStringify);
        await File.WriteAllTextAsync(
            Path.Combine(dtosFolderPath, request.EntityName + "PartOutput.cs"),
            request.PartOutputStringify);
        await File.WriteAllTextAsync(
            Path.Combine(dtosFolderPath, "Create" + request.EntityName + "Input.cs"),
            request.CreateInputStringify);
        await File.WriteAllTextAsync(
            Path.Combine(dtosFolderPath, "Update" + request.EntityName + "Input.cs"),
            request.UpdateInputStringify);
        await File.WriteAllTextAsync(
            Path.Combine(dtosFolderPath, "Get" + request.EntityName + "Input.cs"),
            request.GetInputStringify);
        await File.WriteAllTextAsync(
            Path.Combine(dtosFolderPath, "Delete" + request.EntityName + "Input.cs"),
            request.DeleteInputStringify);

        var managerFolderPath = Path.Combine(projectFolderPath,
            $"aspnet-core\\src\\{request.ProjectName}.Application\\Manager");
        if (!Directory.Exists(managerFolderPath))
        {
            Directory.CreateDirectory(managerFolderPath);
            await File.WriteAllTextAsync(
                Path.Combine(managerFolderPath, "MapperManager.cs"),
                MapperManagerHelper.GetMapperManagerBaseStringify(request.ProjectName));

            await MapperManagerHelper.AddMapperMethodsToApplicationModule(Path.Combine(projectFolderPath,
                    $"aspnet-core\\src\\{request.ProjectName}.Application\\{request.ProjectName}ApplicationModule.cs"),
                request.ProjectName);
        }

        await MapperManagerHelper.WriteAutoMapperConfigurations(Path.Combine(managerFolderPath, "MapperManager.cs"),
            request);


        return new ProjectReply()
        {
            Id = request.Id
        };
    }

    public override async Task<ProjectReply> AddAppServiceToExistingProject(AddAppServiceRequest request,
        ServerCallContext context)
    {
        var projectFolderPath = Path.Combine(_configuration["ProjectsFolderPath"], request.Id);
        var dtosFolderPath = Path.Combine(projectFolderPath,
            $"aspnet-core\\src\\{request.ProjectName}.Application\\Domain\\{request.EntityName}");

        if (!Directory.Exists(dtosFolderPath))
        {
            Directory.CreateDirectory(dtosFolderPath);
        }

        await WritePermissionNames(projectFolderPath, request);
        await WriteAuthorizationProvider(projectFolderPath, request);

        await File.WriteAllTextAsync(
            Path.Combine(dtosFolderPath, $"I{request.EntityName}AppService.cs"),
            request.AppServiceInterfaceStringify);

        await File.WriteAllTextAsync(
            Path.Combine(dtosFolderPath, $"{request.EntityName}AppService.cs"),
            request.AppServiceStringify);
        return new ProjectReply()
        {
            Id = request.Id
        };
    }

    public override async Task<ProjectReply> AddConfigurationToExistingProject(AddEntityRequest request,
        ServerCallContext context)
    {
        var projectFolderPath = Path.Combine(_configuration["ProjectsFolderPath"], request.Id);
        var configurationsFolderPath = Path.Combine(projectFolderPath,
            $"aspnet-core\\src\\{request.ProjectName}.Core\\Domain\\Configurations");

        if (!Directory.Exists(configurationsFolderPath))
        {
            Directory.CreateDirectory(configurationsFolderPath);
        }

        await File.WriteAllTextAsync(
            Path.Combine(configurationsFolderPath, $"{request.EntityName}Configuration.cs"),
            request.Stringified);

        var dbContextFilePath = Path.Combine(projectFolderPath,
            $"aspnet-core\\src\\{request.ProjectName}.EntityFrameworkCore\\EntityFrameworkCore\\{request.ProjectName}DbContext.cs");

        var dbContext =
            await HelperClass.HelperClass.AddConfigurationNamespaceToDbContext(dbContextFilePath, request.ProjectName);

        dbContext = await HelperClass.HelperClass.AddConfigurationToDbContext(dbContext, request.ProjectName,
            request.EntityName);
        await File.WriteAllTextAsync(dbContextFilePath, dbContext.ToString());


        return new ProjectReply()
        {
            Id = request.Id
        };
    }

    public override async Task<ProjectReply> AddTypeScriptDtosToExistingProject(AddDtosRequest request,
        ServerCallContext context)
    {
        var projectFolderPath = Path.Combine(_configuration["ProjectsFolderPath"], request.Id);
        var serviceFolderPath = Path.Combine(projectFolderPath,
            $"angular\\src\\shared\\services\\{request.EntityName}\\dtos");

        if (!Directory.Exists(serviceFolderPath))
        {
            Directory.CreateDirectory(serviceFolderPath);
        }

        await File.WriteAllTextAsync(Path.Combine(serviceFolderPath, $"{request.EntityName}FullOutput.ts"),
            request.FullOutputStringify);

        await File.WriteAllTextAsync(Path.Combine(serviceFolderPath, $"{request.EntityName}PartOutput.ts"),
            request.PartOutputStringify);

        await File.WriteAllTextAsync(Path.Combine(serviceFolderPath, $"Create{request.EntityName}Input.ts"),
            request.CreateInputStringify);

        await File.WriteAllTextAsync(Path.Combine(serviceFolderPath, $"Update{request.EntityName}Input.ts"),
            request.UpdateInputStringify);

        await File.WriteAllTextAsync(Path.Combine(serviceFolderPath, $"Get{request.EntityName}Input.ts"),
            request.GetInputStringify);

        await File.WriteAllTextAsync(Path.Combine(serviceFolderPath, $"Delete{request.EntityName}Input.ts"),
            request.DeleteInputStringify);

        return new ProjectReply()
        {
            Id = request.Id
        };
    }

    public override async Task<ProjectReply> AddTypeScriptServiceToExistingProject(AddTypeScriptServiceRequest request,
        ServerCallContext context)
    {
        var projectFolderPath = Path.Combine(_configuration["ProjectsFolderPath"], request.Id);
        var serviceFolderPath = Path.Combine(projectFolderPath,
            $"angular\\src\\shared\\services\\{request.EntityName}");

        if (!Directory.Exists(serviceFolderPath))
        {
            Directory.CreateDirectory(serviceFolderPath);
        }

        await File.WriteAllTextAsync(Path.Combine(serviceFolderPath, $"{request.EntityName.ToCamelCase()}.service.ts"),
            request.ServiceStringify);

        return new ProjectReply()
        {
            Id = request.Id
        };
    }

    public override async Task FileDownLoad(FileInfo request, IServerStreamWriter<BytesContent> responseStream,
        ServerCallContext context)
    {
        var projectFolderPath = Path.Combine(_configuration["ProjectsFolderPath"], request.ProjectId.ToString());
        var zipPath = Path.Combine(_configuration["ProjectsFolderPath"],
            request.ProjectId + request.FileName + ".zip");

        if (File.Exists(zipPath))
        {
            File.Delete(zipPath);
        }

        ZipFile.CreateFromDirectory(projectFolderPath, zipPath);

        using FileStream fileStream = new FileStream(zipPath, FileMode.Open, FileAccess.Read);

        byte[] buffer = new byte[2048];

        BytesContent content = new BytesContent
        {
            FileSize = fileStream.Length,
            Info = new FileInfo {FileName = request.ProjectId + "-" + request.FileName, FileExtension = ".zip"},
            ReadedByte = 0
        };

        while ((content.ReadedByte = fileStream.Read(buffer, 0, buffer.Length)) > 0)
        {
            content.Buffer = ByteString.CopyFrom(buffer);
            await responseStream.WriteAsync(content);
        }

        fileStream.Close();

        File.Delete(zipPath);
    }

    public override async Task<ProjectReply> AddTypeScriptEnumToExistingProject(AddEnumRequest request,
        ServerCallContext context)
    {
        var projectFolderPath = Path.Combine(_configuration["ProjectsFolderPath"], request.Id);
        var enumFolderPath = Path.Combine(projectFolderPath,
            $"angular\\src\\shared\\services\\enums");

        if (!Directory.Exists(enumFolderPath))
        {
            Directory.CreateDirectory(enumFolderPath);
        }

        await File.WriteAllTextAsync(
            Path.Combine(enumFolderPath, request.EnumName + ".ts"),
            request.Stringified);

        return new ProjectReply()
        {
            Id = request.Id
        };
    }

    public override async Task<ProjectReply> AddTypeScriptComponentsToExistingProject(ComponentResult request,
        ServerCallContext context)
    {
        var projectFolderPath = Path.Combine(_configuration["ProjectsFolderPath"], request.ProjectId.ToString());
        var listComponentFolderPath = Path.Combine(projectFolderPath,
            $"angular\\src\\app\\components\\{request.EntityName}");

        if (!Directory.Exists(listComponentFolderPath))
        {
            Directory.CreateDirectory(listComponentFolderPath);
        }

        await HelperClass.HelperClass.CreateRouting(projectFolderPath, request.EntityName);
        await HelperClass.HelperClass.AddComponentToModule(projectFolderPath, request.EntityName);
        await HelperClass.HelperClass.AddCreateComponentToModule(projectFolderPath, request.EntityName);

        #region ListComponent

        await File.WriteAllTextAsync(
            Path.Combine(listComponentFolderPath, request.EntityName.ToCamelCase() + ".component.ts"),
            request.ListComponent.ComponentTsStringify);

        await File.WriteAllTextAsync(
            Path.Combine(listComponentFolderPath, request.EntityName.ToCamelCase() + ".component.html"),
            request.ListComponent.ComponentHtmlStringify);

        await File.WriteAllTextAsync(
            Path.Combine(listComponentFolderPath, request.EntityName.ToCamelCase() + ".component.css"),
            request.ListComponent.ComponentCssStringify);

        #endregion

        #region CreateComponent

        var createComponentFolderPath =
            Path.Combine(listComponentFolderPath, $"create-{request.EntityName.ToCamelCase()}");

        if (!Directory.Exists(createComponentFolderPath))
        {
            Directory.CreateDirectory(createComponentFolderPath);
        }

        await File.WriteAllTextAsync(
            Path.Combine(createComponentFolderPath, "create-" + request.EntityName.ToCamelCase() + ".component.ts"),
            request.CreateComponent.ComponentTsStringify);

        await File.WriteAllTextAsync(
            Path.Combine(createComponentFolderPath, "create-" + request.EntityName.ToCamelCase() + ".component.html"),
            request.CreateComponent.ComponentHtmlStringify);

        await File.WriteAllTextAsync(
            Path.Combine(createComponentFolderPath, "create-" + request.EntityName.ToCamelCase() + ".component.css"),
            request.CreateComponent.ComponentCssStringify);

        #endregion

        #region EditComponent

        var editComponentFolderPath =
            Path.Combine(listComponentFolderPath, $"edit-{request.EntityName.ToCamelCase()}");

        if (!Directory.Exists(editComponentFolderPath))
        {
            Directory.CreateDirectory(editComponentFolderPath);
        }

        await File.WriteAllTextAsync(
            Path.Combine(editComponentFolderPath, "edit-" + request.EntityName.ToCamelCase() + ".component.ts"),
            request.EditComponent.ComponentTsStringify);

        await File.WriteAllTextAsync(
            Path.Combine(editComponentFolderPath, "edit-" + request.EntityName.ToCamelCase() + ".component.html"),
            request.EditComponent.ComponentHtmlStringify);

        await File.WriteAllTextAsync(
            Path.Combine(editComponentFolderPath, "edit-" + request.EntityName.ToCamelCase() + ".component.css"),
            request.EditComponent.ComponentCssStringify);

        #endregion

        return new ProjectReply()
        {
            Id = request.ProjectId.ToString()
        };
    }

    public override async Task<ProjectReply> AddNavigationToExistingProject(AddStringToExistingProject request,
        ServerCallContext context)
    {
        var result = new ProjectReply();

        var projectFolderPath = Path.Combine(_configuration["ProjectsFolderPath"], request.ProjectId.ToString());
        var sideBarMenuPath =
            Path.Combine(projectFolderPath, "angular\\src\\app\\layout\\sidebar-menu.component.ts");

        var sideBarMenuContent = await File.ReadAllTextAsync(sideBarMenuPath);

        var getMenuItemsIndex =
            sideBarMenuContent.IndexOf("getMenuItems(): MenuItem[] {", StringComparison.Ordinal) + 1;

        var insertableIndex =
            sideBarMenuContent.IndexOf(@"];", getMenuItemsIndex, StringComparison.Ordinal) - 1;

        var stringBuilder = new StringBuilder(sideBarMenuContent);

        var formattedStringify = new StringBuilder();
        foreach (var stringifyLine in request.Stringify.Split(Environment.NewLine))
        {
            formattedStringify.InsertTab(3).Append(stringifyLine).NewLine();
        }

        stringBuilder.Insert(insertableIndex, Environment.NewLine + formattedStringify);

        await File.WriteAllTextAsync(
            sideBarMenuPath,
            stringBuilder.ToString());

        return new ProjectReply()
        {
            Id = request.ProjectId.ToString()
        };
    }

    #region Helper Methods

    private async Task WritePermissionNames(string projectFolderPath, AddAppServiceRequest request)
    {
        var permissionNamesFilePath = Path.Combine(projectFolderPath,
            $"aspnet-core\\src\\{request.ProjectName}.Core\\Authorization\\PermissionNames.cs");
        var permissionNameFileContent = await File.ReadAllTextAsync(permissionNamesFilePath);
        var permissionNameStringBuilder = new StringBuilder(permissionNameFileContent);

        foreach (var permissionName in request.PermissionNames.Split(Environment.NewLine))
        {
            var tempStringBuilder = new StringBuilder().InsertTab(2).Append(permissionName).NewLine();

            var permissionNameInsertableIndex =
                permissionNameStringBuilder.ToString().IndexOf("}", StringComparison.Ordinal) - 4;
            permissionNameStringBuilder.Insert(permissionNameInsertableIndex, tempStringBuilder.ToString());
        }

        permissionNameStringBuilder.NewLine(2);
        await File.WriteAllTextAsync(
            permissionNamesFilePath,
            permissionNameStringBuilder.ToString());
    }

    private async Task WriteAuthorizationProvider(string projectFolderPath, AddAppServiceRequest request)
    {
        var authorizationProviderFilePath = Path.Combine(projectFolderPath,
            $"aspnet-core\\src\\{request.ProjectName}.Core\\Authorization\\{request.ProjectName}AuthorizationProvider.cs");
        var authorizationProviderFileContent = await File.ReadAllTextAsync(authorizationProviderFilePath);
        var authorizationProviderStringBuilder = new StringBuilder(authorizationProviderFileContent);

        var setPermissionStartIndex = authorizationProviderStringBuilder.ToString()
            .IndexOf("public override void SetPermissions(IPermissionDefinitionContext context)",
                StringComparison.Ordinal);

        foreach (var authorizationProvider in request.AuthorizationProviders.Split(Environment.NewLine))
        {
            var tempStringBuilder = new StringBuilder().InsertTab(3).Append(authorizationProvider).NewLine();

            var authorizationProviderInsertableIndex =
                authorizationProviderStringBuilder.ToString()
                    .IndexOf("}", setPermissionStartIndex, StringComparison.Ordinal) - 8;
            authorizationProviderStringBuilder.Insert(authorizationProviderInsertableIndex,
                tempStringBuilder.ToString());
        }

        authorizationProviderStringBuilder.NewLine(2);
        await File.WriteAllTextAsync(
            authorizationProviderFilePath,
            authorizationProviderStringBuilder.ToString());
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

    private void RecursiveDelete(DirectoryInfo baseDir)
    {
        if (!baseDir.Exists)
            return;

        foreach (var dir in baseDir.EnumerateDirectories())
        {
            RecursiveDelete(dir);
        }

        var files = baseDir.GetFiles();
        foreach (var file in files)
        {
            file.IsReadOnly = false;
            file.Delete();
        }

        baseDir.Delete();
    }

    #endregion
}