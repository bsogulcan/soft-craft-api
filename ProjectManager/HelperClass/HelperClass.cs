using System.Text;
using Extensions;
using Humanizer;

namespace ProjectManager.HelperClass;

public static class HelperClass
{
    public static async Task<StringBuilder> AddEntitiesNamespace(string dbContextFilePath, string projectName)
    {
        var dbContext = await File.ReadAllTextAsync(dbContextFilePath);
        var entitiesUsingLine = $"using {projectName}.Domain.Entities;";
        var existingEntitiesUsingBlock = dbContext.Contains(entitiesUsingLine);

        var modifiedDbContext = new StringBuilder(dbContext);

        if (existingEntitiesUsingBlock)
        {
            return modifiedDbContext;
        }

        var lastUsingLineNumber = dbContext.LastIndexOf($"namespace {projectName}.EntityFrameworkCore",
            StringComparison.Ordinal) - 1;

        modifiedDbContext.Insert(lastUsingLineNumber,
            entitiesUsingLine
        );
        modifiedDbContext.Insert(lastUsingLineNumber + entitiesUsingLine.Length, Environment.NewLine);
        modifiedDbContext.Append(Environment.NewLine);

        return modifiedDbContext;
    }

    public static async Task<StringBuilder> AddEntityToDbContext(string dbContextFilePath, string projectName,
        string entityName)
    {
        var dbContext = await File.ReadAllTextAsync(dbContextFilePath);
        var existingField = dbContext.Contains($"DbSet<{entityName}>");

        var modifiedDbContext = new StringBuilder(dbContext);

        if (existingField)
        {
            return modifiedDbContext;
        }

        var dbSetText = "public virtual DbSet<" + entityName + "> " + entityName.Pluralize() + " { get; set; }";
        var constructorIndex = dbContext.LastIndexOf(
            $"public {projectName}DbContext(DbContextOptions<{projectName}DbContext> options)",
            StringComparison.Ordinal);

        modifiedDbContext.Insert(constructorIndex,
            dbSetText
        );
        modifiedDbContext.Insert(constructorIndex + dbSetText.Length, Environment.NewLine + "\t \t");

        return modifiedDbContext;
    }

    public static string GetElasticSearchConfiguration(string projectName)
    {
        return
            $"<?xml version=\"1.0\" encoding=\"utf-8\"?>\n\n<log4net>\n    <appender name=\"ElasticSearchAppender\" type=\"log4net.ElasticSearch.ElasticSearchAppender, log4net.ElasticSearch\">\n        <connectionString value=\"Server=localhost;Index={projectName};Port=9200;\" />\n        <bufferSize value=\"0\" />\n    </appender>\n    <root>\n        <level value=\"ALL\" />\n        <appender-ref ref=\"ElasticSearchAppender\" />\n    </root>\n</log4net>";
    }

    public static async Task<StringBuilder> AddConfigurationNamespaceToDbContext(string dbContextFilePath,
        string projectName)
    {
        var dbContext = await File.ReadAllTextAsync(dbContextFilePath);
        var entitiesUsingLine = $"using {projectName}.Domain.Configurations;";
        var existingEntitiesUsingBlock = dbContext.Contains(entitiesUsingLine);

        var modifiedDbContext = new StringBuilder(dbContext);

        if (existingEntitiesUsingBlock)
        {
            return modifiedDbContext;
        }

        var lastUsingLineNumber = dbContext.LastIndexOf($"namespace {projectName}.EntityFrameworkCore",
            StringComparison.Ordinal) - 1;

        modifiedDbContext.Insert(lastUsingLineNumber,
            entitiesUsingLine
        );
        modifiedDbContext.Insert(lastUsingLineNumber + entitiesUsingLine.Length, Environment.NewLine);
        modifiedDbContext.Append(Environment.NewLine);

        return modifiedDbContext;
    }

    public static async Task<StringBuilder> AddConfigurationToDbContext(StringBuilder stringBuilder,
        string projectName, string entityName)
    {
        var existingOnModelCreatingMethod =
            stringBuilder.ToString().Contains("protected override void OnModelCreating");

        var ctor = $"public {projectName}DbContext(DbContextOptions<{projectName}DbContext> options)";

        if (!existingOnModelCreatingMethod)
        {
            var ctorBlockIndex = stringBuilder.ToString()
                .IndexOf(ctor,
                    StringComparison.Ordinal);

            var closingCtorBlockIndex =
                stringBuilder.ToString().IndexOf("}", ctorBlockIndex, StringComparison.Ordinal) + 1;

            var onModelCreatingStringBuilder = new StringBuilder();
            onModelCreatingStringBuilder.NewLine(2)
                .InsertTab(2)
                .Append("protected override void OnModelCreating(ModelBuilder modelBuilder)")
                .NewLine()
                .InsertTab(2)
                .Append("{")
                .NewLine()
                .InsertTab(3)
                .Append("base.OnModelCreating(modelBuilder);")
                .NewLine()
                .InsertTab(2)
                .Append("}");

            stringBuilder.Insert(closingCtorBlockIndex, onModelCreatingStringBuilder.ToString());
        }

        var baseOnModelCreatingIndex = stringBuilder.ToString()
            .IndexOf("base.OnModelCreating(modelBuilder);", StringComparison.Ordinal);

        var insertableIndex = stringBuilder.ToString()
            .IndexOf(Environment.NewLine, baseOnModelCreatingIndex, StringComparison.Ordinal);

        var applyConfigurationStringBuilder = new StringBuilder();
        applyConfigurationStringBuilder.NewLine()
            .InsertTab(3)
            .Append($"modelBuilder.ApplyConfiguration(new {entityName}Configuration());");

        stringBuilder.Insert(insertableIndex, applyConfigurationStringBuilder.ToString());

        return stringBuilder;
    }

    public static async Task CreateRouting(string basePath, string entityName)
    {
        var appRoutingModuleFilePath =
            Path.Combine(basePath, "angular", "src", "app", "app-routing.module.ts");
        var appRoutingModuleContent = await File.ReadAllTextAsync(appRoutingModuleFilePath);

        var stringBuilder = new StringBuilder(appRoutingModuleContent);

        var ngModuleIndex = appRoutingModuleContent.IndexOf("@NgModule({", StringComparison.Ordinal) - 1;

        var import = "import {" + entityName + "Component} from './components/" + entityName + "/" +
                     entityName.ToCamelCase() + ".component';" + Environment.NewLine;
        stringBuilder.Insert(ngModuleIndex, import);

        var childrenIndex = stringBuilder.ToString().IndexOf("children", StringComparison.Ordinal);
        var childrenArrayEndIndex = stringBuilder.ToString()
            .IndexOf(",\n                ]", childrenIndex, StringComparison.Ordinal) + 1;
        var route =
            Environment.NewLine + "                    {path: '" + entityName.ToCamelCase().Pluralize() +
            "', component: " + entityName +
            "Component, data: { permission: '" + entityName + "' }, canActivate: [AppRouteGuard]},";


        stringBuilder.Insert(childrenArrayEndIndex, route);

        await File.WriteAllTextAsync(appRoutingModuleFilePath, stringBuilder.ToString());
    }

    public static async Task AddComponentToModule(string basePath, string entityName)
    {
        var appModuleFilePath =
            Path.Combine(basePath, "angular", "src", "app", "app.module.ts");
        var appModuleContent = await File.ReadAllTextAsync(appModuleFilePath);

        var stringBuilder = new StringBuilder(appModuleContent);

        var ngModuleIndex = appModuleContent.IndexOf("@NgModule({", StringComparison.Ordinal) - 1;

        var import = "import {" + entityName + "Component} from './components/" + entityName + "/" +
                     entityName.ToCamelCase() + ".component';" + Environment.NewLine;
        stringBuilder.Insert(ngModuleIndex, import);

        var declarationIndex = stringBuilder.ToString().IndexOf("declarations", StringComparison.Ordinal);
        var declarationArrayEndIndex = stringBuilder.ToString()
            .IndexOf(",\n    ]", declarationIndex, StringComparison.Ordinal) + 1;
        var component =
            Environment.NewLine + $"        {entityName}Component,";


        stringBuilder.Insert(declarationArrayEndIndex, component);

        await File.WriteAllTextAsync(appModuleFilePath, stringBuilder.ToString());
    }
}