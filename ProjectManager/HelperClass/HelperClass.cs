using System.Text;
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
}