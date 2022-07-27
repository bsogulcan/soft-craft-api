using System.Text;
using Extensions;

namespace ProjectManager.HelperClass;

public class MapperManagerHelper
{
    public static string GetMapperManagerBaseStringify(string projectName)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append("using AutoMapper;").NewLine().Append($"using {projectName}.Domain.Entities;").NewLine(2);
        stringBuilder.Append($"namespace {projectName}.Manager")
            .NewLine()
            .Append('{')
            .NewLine()
            .InsertTab();

        stringBuilder.Append("public class MapperManager")
            .NewLine()
            .InsertTab()
            .Append('{')
            .NewLine();

        stringBuilder.InsertTab(2)
            .Append("public static void DtosToDomain(IMapperConfigurationExpression cfg)")
            .NewLine()
            .InsertTab(2)
            .Append('{')
            .NewLine(1)
            .InsertTab(2)
            .Append('}')
            .NewLine();

        stringBuilder.NewLine();
        stringBuilder.InsertTab(2)
            .Append("public static void DomainToDtos(IMapperConfigurationExpression cfg)")
            .NewLine()
            .InsertTab(2)
            .Append('{')
            .NewLine(1)
            .InsertTab(2)
            .Append('}');

        stringBuilder.NewLine().InsertTab().Append('}');
        stringBuilder.NewLine().Append('}');

        return stringBuilder.ToString();
    }

    public static async Task AddMapperMethodsToApplicationModule(string filePath, string projectName)
    {
        var applicationModuleContent = await File.ReadAllTextAsync(filePath);
        var namespaceBlockIndex =
            applicationModuleContent.IndexOf($"namespace {projectName}", StringComparison.Ordinal) - 1;

        var stringBuilder = new StringBuilder(applicationModuleContent);
        var usingManagerString = $"using {projectName}.Manager;";
        stringBuilder.Insert(namespaceBlockIndex, usingManagerString + Environment.NewLine);


        // cfg => cfg.AddMaps(thisAssembly)

        // cfg =>
        // {
        //     MapperManager.DtosToDomain(cfg);
        //     MapperManager.DomainToDtos(cfg);
        //     cfg.AddMaps(thisAssembly);
        // });

        stringBuilder = stringBuilder.Replace("cfg => cfg.AddMaps(thisAssembly)", @"
                cfg =>
                {
                    MapperManager.DtosToDomain(cfg);
                    MapperManager.DomainToDtos(cfg);
                    cfg.AddMaps(thisAssembly);
                }
");

        await File.WriteAllTextAsync(filePath, stringBuilder.ToString());
    }

    public static async Task WriteAutoMapperConfigurations(string filePath, AddDtosRequest request)
    {
        var mapperManagerContent = await File.ReadAllTextAsync(filePath);
        var namespaceBlockIndex =
            mapperManagerContent.IndexOf($"namespace {request.ProjectName}", StringComparison.Ordinal) - 2;

        var dtosToDomainStartIndex =
            mapperManagerContent.IndexOf("public static void DtosToDomain(IMapperConfigurationExpression cfg)",
                StringComparison.Ordinal);

        var dtosToDomainLastLineIndex =
            mapperManagerContent.IndexOf("}", dtosToDomainStartIndex, StringComparison.Ordinal) - 2;

        var stringBuilder = new StringBuilder(mapperManagerContent);
        var sbDtosToDomain = new StringBuilder().InsertTab(3).Append($"//{request.EntityName}").NewLine();
        var dtosToDomainLines = request.DtosToDomainStringify.Split(Environment.NewLine);
        foreach (var line in dtosToDomainLines)
        {
            sbDtosToDomain.InsertTab(3).Append(line).NewLine();
        }

        stringBuilder.Insert(dtosToDomainLastLineIndex,
            sbDtosToDomain + Environment.NewLine
        );
        await File.WriteAllTextAsync(filePath, stringBuilder.ToString());

        ////
        mapperManagerContent = await File.ReadAllTextAsync(filePath);
        var domainToDtosStartIndex =
            mapperManagerContent.IndexOf("public static void DomainToDtos(IMapperConfigurationExpression cfg)",
                StringComparison.Ordinal);

        var domainToDtosLastLineIndex =
            mapperManagerContent.IndexOf("}", domainToDtosStartIndex, StringComparison.Ordinal) - 2;
        var sbDomainToDtos = new StringBuilder().InsertTab(3).Append($"//{request.EntityName}").NewLine();
        var domainToDtosLines = request.DomainToDtosStringify.Split(Environment.NewLine);
        foreach (var line in domainToDtosLines)
        {
            sbDomainToDtos.InsertTab(3).Append(line).NewLine();
        }

        stringBuilder.Insert(domainToDtosLastLineIndex,
            sbDomainToDtos + Environment.NewLine
        );

        stringBuilder.Insert(namespaceBlockIndex,
            $"using {request.ProjectName}.Domain.{request.EntityName}.Dtos;" + Environment.NewLine);

        await File.WriteAllTextAsync(filePath, stringBuilder.ToString());
    }
}