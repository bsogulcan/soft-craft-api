using System.Text;
using DotNetCodeGenerator.Extensions;
using Grpc.Core;
using Humanizer;

namespace DotNetCodeGenerator.Services;

public class DotNetCodeGeneratorService : DotNetCodeGenerator.DotNetCodeGeneratorBase
{
    private readonly ILogger<DotNetCodeGeneratorService> _logger;

    public DotNetCodeGeneratorService(ILogger<DotNetCodeGeneratorService> logger)
    {
        _logger = logger;
    }

    public override async Task<EntityResult> CreateEntity(Entity request, ServerCallContext context)
    {
        var entityResult = new EntityResult
        {
            Entity = request
        };

        var stringBuilder = new StringBuilder();
        for (var i = 0; i < request.Usings.Count; i++)
        {
            stringBuilder.Append("using " + request.Usings[i] + Environment.NewLine);
        }

        stringBuilder.NewLine()
            .Append($"namespace {request.Namespace}")
            .NewLine();

        stringBuilder.Append('{')
            .NewLine();

        stringBuilder.InsertTab()
            .Append($"public class {request.Name} : ");

        if (request.FullAudited)
        {
            stringBuilder.Append($"FullAuditedEntity<{GetPrimaryKey(request.PrimaryKeyType)}>");
        }
        else
        {
            stringBuilder.Append($"Entity<{GetPrimaryKey(request.PrimaryKeyType)}>");
        }

        switch (request.TenantType)
        {
            case TenantType.May:
                stringBuilder.Append(", IMayHaveTenant");
                break;
            case TenantType.Must:
                stringBuilder.Append(", IMustHaveTenant");
                break;
        }

        stringBuilder.NewLine()
            .InsertTab()
            .Append("{")
            .NewLine();

        stringBuilder.InsertTab(2)
            .Append($"public {request.Name}()")
            .NewLine();

        stringBuilder.InsertTab(2)
            .Append("{")
            .NewLine();

        foreach (var relationalProperty in request.Properties.Where(x =>
                     x.IsRelationalProperty && x.RelationType != RelationType.OneToOne))
        {
            stringBuilder.InsertTab(3)
                .Append(
                    $"{relationalProperty.Name.Pluralize()} = new HashSet<{relationalProperty.RelationalEntityName}>();")
                .NewLine();
        }

        stringBuilder.InsertTab(2)
            .Append("}")
            .NewLine();

        for (var i = 0; i < request.Properties.Count; i++)
        {
            var property = request.Properties[i];

            stringBuilder.InsertTab(2);

            if (!property.IsRelationalProperty)
            {
                stringBuilder.Append($"public {property.Type + (property.Nullable ? "? " : " ")}" +
                                     property.Name + " { get; set; }" + Environment.NewLine);
            }
            else
            {
                //TODO: Write Relational properties
                if (property.RelationType == RelationType.OneToOne)
                {
                    // public int LineId { get; set; }
                    // public virtual Line Line { get; set; }
                    stringBuilder.Append(
                        $"public {GetPrimaryKey(property.RelationalEntityPrimaryKeyType) + (property.Nullable ? "? " : " ")}" +
                        property.RelationalEntityName + "Id { get; set; }" + Environment.NewLine);

                    stringBuilder.InsertTab(2);

                    stringBuilder.Append(
                        $"public virtual {property.RelationalEntityName + (property.Nullable ? "? " : " ")}" +
                        property.Name + " { get; set; }" + Environment.NewLine);
                }
                else // OneToMany
                {
                    stringBuilder.Append(
                        $"public virtual ICollection<{property.RelationalEntityName}>" +
                        property.Name.Pluralize() + " { get; set; }" + Environment.NewLine);
                }
            }
        }

        stringBuilder.InsertTab()
            .Append("}")
            .NewLine()
            .Append("}");

        entityResult.Stringified = stringBuilder.ToString();
        return entityResult;
    }

    public override async Task<EntityResult> CreateRepositoryInterface(EntityForRepository request,
        ServerCallContext context)
    {
        var entityResult = new EntityResult();
        var stringBuilder = new StringBuilder();
        stringBuilder.Append("using Abp.Domain.Repositories;" + Environment.NewLine);

        for (var i = 0; i < request.Usings.Count; i++)
        {
            stringBuilder.Append("using " + request.Usings[i] + Environment.NewLine);
        }

        stringBuilder.Append(Environment.NewLine);

        stringBuilder.Append($"namespace {request.Namespace}");
        stringBuilder.Append(Environment.NewLine);

        stringBuilder.Append('{' + Environment.NewLine);

        stringBuilder.Append('\t' +
                             $"public interface I{request.Name}Repository : IRepository<{request.Name}, {GetPrimaryKey(request.PrimaryKeyType)}>");
        stringBuilder.Append(Environment.NewLine);
        stringBuilder.Append('\t');
        stringBuilder.Append("{");
        stringBuilder.Append(Environment.NewLine);
        stringBuilder.Append('\t');
        stringBuilder.Append("}");
        stringBuilder.Append(Environment.NewLine);
        stringBuilder.Append("}");

        entityResult.Stringified = stringBuilder.ToString();
        return entityResult;
    }

    public override async Task<EntityResult> CreateRepository(EntityForRepository request,
        ServerCallContext context)
    {
        var entityResult = new EntityResult();

        var stringBuilder = new StringBuilder();
        stringBuilder.Append("using Abp.EntityFrameworkCore;" + Environment.NewLine);

        for (var i = 0; i < request.Usings.Count; i++)
        {
            stringBuilder.Append("using " + request.Usings[i] + Environment.NewLine);
        }

        stringBuilder.Append(Environment.NewLine);

        stringBuilder.Append($"namespace {request.Namespace}");
        stringBuilder.Append(Environment.NewLine);

        stringBuilder.Append('{' + Environment.NewLine);

        //TODO: Create Repository
        stringBuilder.Append('\t' +
                             $"public class {request.Name}Repository : {request.ProjectName}RepositoryBase<{request.Name},{GetPrimaryKey(request.PrimaryKeyType)}>,I{request.Name}Repository");

        stringBuilder.Append(Environment.NewLine);
        stringBuilder.Append('\t');
        stringBuilder.Append("{");
        stringBuilder.Append(Environment.NewLine);
        stringBuilder.Append('\t');
        stringBuilder.Append('\t');
        stringBuilder.Append(
            $"public {request.Name}Repository(IDbContextProvider<{request.ProjectName}DbContext> dbContextProvider) : base(dbContextProvider)");
        stringBuilder.Append(Environment.NewLine);
        stringBuilder.Append('\t');
        stringBuilder.Append('\t');
        stringBuilder.Append("{");
        stringBuilder.Append(Environment.NewLine);
        stringBuilder.Append('\t');
        stringBuilder.Append('\t');
        stringBuilder.Append("}");
        stringBuilder.Append(Environment.NewLine);
        stringBuilder.Append('\t');
        stringBuilder.Append("}");
        stringBuilder.Append(Environment.NewLine);
        stringBuilder.Append("}");


        entityResult.Stringified = stringBuilder.ToString();
        return entityResult;
    }

    public override async Task<EntityResult> CreateEnum(Enum request, ServerCallContext context)
    {
        var entityResult = new EntityResult();
        var stringBuilder = new StringBuilder();

        stringBuilder.Append($"namespace {request.Namespace}");
        stringBuilder.Append(Environment.NewLine);

        stringBuilder.Append('{' + Environment.NewLine);
        stringBuilder.Append('\t' + $"public enum {request.Name}");
        stringBuilder.Append(Environment.NewLine);
        stringBuilder.Append('\t');
        stringBuilder.Append("{");
        stringBuilder.Append(Environment.NewLine);

        foreach (var value in request.Values)
        {
            stringBuilder.Append('\t');
            stringBuilder.Append('\t');
            stringBuilder.Append(value.Name + " = " + value.Value + ",");
            stringBuilder.Append(Environment.NewLine);
        }

        stringBuilder.Append('\t');
        stringBuilder.Append("}");
        stringBuilder.Append(Environment.NewLine);
        stringBuilder.Append("}");

        entityResult.Stringified = stringBuilder.ToString();
        return entityResult;
    }

    public override async Task<DtoResult> CreateDtos(Entity request, ServerCallContext context)
    {
        var dtoResult = new DtoResult();
        dtoResult.FullOutputStringify = GenerateFullOutputDto(request);
        dtoResult.PartOutputStringify = GeneratePartOutputDto(request);
        dtoResult.CreateInputStringify = GenerateCreateInputDto(request);
        dtoResult.UpdateInputStringify = GenerateUpdateInputDto(request);
        dtoResult.DeleteInputStringify = GenerateDeleteInputDto(request);
        dtoResult.GetInputStringify = GenerateGetInputDto(request);

        dtoResult.DtosToDomainStringify = GenerateDtosToDomainStringify(request);
        dtoResult.DomainToDtosStringify = GenerateDomainToDtosStringify(request);
        return dtoResult;
    }

    public override async Task<AppServiceResult> CreateAppService(AppServiceRequest request, ServerCallContext context)
    {
        var result = new AppServiceResult();
        var appServiceInterfaceStringBuilder = new StringBuilder();
        appServiceInterfaceStringBuilder.Append("using Abp.Application.Services;")
            .NewLine()
            .Append($"using {request.ProjectName}.Domain.{request.EntityName}.Dtos;")
            .NewLine(2);

        appServiceInterfaceStringBuilder.Append($"namespace {request.ProjectName}.Domain.{request.EntityName}")
            .NewLine()
            .Append('{')
            .NewLine()
            .InsertTab();

        appServiceInterfaceStringBuilder
            .Append(
                $"public interface I{request.EntityName}AppService:IAsyncCrudAppService<{request.EntityName}FullOutput,int,Get{request.EntityName}Input,Create{request.EntityName}Input,Update{request.EntityName}Input,Get{request.EntityName}Input,Delete{request.EntityName}Input>")
            .NewLine()
            .InsertTab()
            .Append('{')
            .NewLine();


        appServiceInterfaceStringBuilder
            .NewLine()
            .InsertTab()
            .Append('}');
        appServiceInterfaceStringBuilder
            .NewLine()
            .Append('}');

        result.AppServiceInterfaceStringify = appServiceInterfaceStringBuilder.ToString();

        var appServiceStringBuilder = new StringBuilder();
        appServiceStringBuilder.Append("using Abp.Application.Services;")
            .NewLine()
            .Append($"using {request.ProjectName}.Domain.{request.EntityName}.Dtos;")
            .NewLine()
            .Append($"using {request.ProjectName}.Domain.{request.EntityName};")
            .NewLine()
            .Append($"using {request.ProjectName}.Domain.Entities;")
            .NewLine()
            .Append($"using {request.ProjectName}.EntityFrameworkCore.Repositories;")
            .NewLine(2);

        appServiceStringBuilder.Append($"namespace {request.ProjectName}.Domain.{request.EntityName}")
            .NewLine()
            .Append('{')
            .NewLine()
            .InsertTab();

        appServiceStringBuilder.Append(
                $"public class {request.EntityName}AppService : AsyncCrudAppService<Entities.{request.EntityName}, {request.EntityName}FullOutput, int, Get{request.EntityName}Input, Create{request.EntityName}Input, Update{request.EntityName}Input, Get{request.EntityName}Input, Delete{request.EntityName}Input>, I{request.EntityName}AppService")
            .NewLine()
            .InsertTab()
            .Append('{')
            .NewLine();

        appServiceStringBuilder.InsertTab(2)
            .Append(
                $"public {request.EntityName}AppService(I{request.EntityName}Repository {request.EntityName.ToLower()}Repository) : base({request.EntityName.ToLower()}Repository)")
            .NewLine()
            .InsertTab(2)
            .Append('{')
            .NewLine(1)
            .InsertTab(2)
            .Append('}')
            .NewLine();

        appServiceStringBuilder
            .NewLine()
            .InsertTab()
            .Append('}');
        appServiceStringBuilder
            .NewLine()
            .Append('}');

        result.AppServiceStringify = appServiceStringBuilder.ToString();
        return result;
    }

    public override async Task<EntityResult> CreateConfiguration(Entity request, ServerCallContext context)
    {
        var entityResult = new EntityResult
        {
            Entity = request
        };

        var stringBuilder = new StringBuilder();
        stringBuilder.Append($"using {request.ProjectName}.Domain.Entities;").NewLine();
        stringBuilder.Append($"using Microsoft.EntityFrameworkCore;").NewLine();
        stringBuilder.Append($"using Microsoft.EntityFrameworkCore.Metadata.Builders;").NewLine();

        stringBuilder.NewLine()
            .Append($"namespace {request.ProjectName}.Domain.Configurations")
            .NewLine()
            .Append("{");

        stringBuilder.NewLine()
            .InsertTab()
            .Append($"public class {request.Name}Configuration : IEntityTypeConfiguration<{request.Name}>");

        stringBuilder.NewLine()
            .InsertTab()
            .Append("{");

        stringBuilder.NewLine()
            .InsertTab(2)
            .Append($"public void Configure(EntityTypeBuilder<{request.Name}> builder)");

        stringBuilder.NewLine()
            .InsertTab(2)
            .Append("{");

        stringBuilder.NewLine()
            .InsertTab(3)
            .Append($"builder.ToTable(\"{request.Name.Pluralize()}\");");

        stringBuilder.NewLine()
            .InsertTab(3)
            .Append($"builder.HasKey(x => x.Id);");

        foreach (var property in request.Properties.Where(x => x.MaxLength > 0))
        {
            stringBuilder.NewLine()
                .InsertTab(3)
                .Append($"builder.Property(x => x.{property.Name}).HasMaxLength({property.MaxLength});");
        }

        foreach (var property in request.Properties.Where(x =>
                     x.IsRelationalProperty && x.RelationType != RelationType.OneToOne && !x.ManyToMany))
        {
            stringBuilder.NewLine(2)
                .InsertTab(3)
                .Append($"builder.HasMany(x => x.{property.Name.Pluralize()})")
                .NewLine().InsertTab(4)
                .Append($".WithOne(y => y.{request.Name})")
                .NewLine().InsertTab(4)
                .Append($".HasForeignKey(y => y.{request.Name}Id)")
                .NewLine().InsertTab(4)
                .Append($".OnDelete(DeleteBehavior.ClientSetNull);");
        }


        stringBuilder.NewLine()
            .InsertTab(2)
            .Append("}");

        stringBuilder.NewLine()
            .InsertTab()
            .Append("}");

        stringBuilder.NewLine()
            .Append("}");

        entityResult.Stringified = stringBuilder.ToString();
        return entityResult;
    }

    #region HelperMethods

    private string GenerateDtosToDomainStringify(Entity request)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder
            .Append($"cfg.CreateMap<{request.Name}, Create{request.Name}Input>();")
            .NewLine();
        stringBuilder.Append($"cfg.CreateMap<{request.Name}, Update{request.Name}Input>();")
            .NewLine();
        stringBuilder.Append($"cfg.CreateMap<{request.Name}, Delete{request.Name}Input>();")
            .NewLine();
        stringBuilder.Append($"cfg.CreateMap<{request.Name}, Get{request.Name}Input>();")
            .NewLine();
        stringBuilder.Append($"cfg.CreateMap<{request.Name}, {request.Name}FullOutput>();")
            .NewLine();
        stringBuilder.Append($"cfg.CreateMap<{request.Name}, {request.Name}PartOutput>();");


        return stringBuilder.ToString();
    }

    private string GenerateDomainToDtosStringify(Entity request)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append($"cfg.CreateMap<Create{request.Name}Input, {request.Name}>();")
            .NewLine();
        stringBuilder.Append($"cfg.CreateMap<Update{request.Name}Input, {request.Name}>();")
            .NewLine();
        stringBuilder.Append($"cfg.CreateMap<Delete{request.Name}Input, {request.Name}>();")
            .NewLine();
        stringBuilder.Append($"cfg.CreateMap<Get{request.Name}Input, {request.Name}>();")
            .NewLine();
        stringBuilder.Append($"cfg.CreateMap<{request.Name}FullOutput, {request.Name}>();")
            .NewLine();
        stringBuilder.Append($"cfg.CreateMap<{request.Name}PartOutput, {request.Name}>();");

        return stringBuilder.ToString();
    }

    private string GenerateGetInputDto(Entity request)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append("using Abp.Application.Services.Dto;" + Environment.NewLine);
        for (var i = 0; i < request.Usings.Count; i++)
        {
            stringBuilder.Append("using " + request.Usings[i] + Environment.NewLine);
        }

        stringBuilder.Append(Environment.NewLine);

        stringBuilder.Append($"namespace {request.Namespace}");
        stringBuilder.Append(Environment.NewLine);

        stringBuilder.Append('{' + Environment.NewLine);

        stringBuilder.Append('\t' +
                             $"public class Get{request.Name}Input : EntityDto<{GetPrimaryKey(request.PrimaryKeyType)}>" +
                             Environment.NewLine);
        stringBuilder.Append('\t');
        stringBuilder.Append("{" + Environment.NewLine);

        stringBuilder.Append('\t');
        stringBuilder.Append("}");
        stringBuilder.Append(Environment.NewLine);
        stringBuilder.Append("}");

        return stringBuilder.ToString();
    }

    private string GenerateDeleteInputDto(Entity request)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append("using Abp.Application.Services.Dto;" + Environment.NewLine);
        for (var i = 0; i < request.Usings.Count; i++)
        {
            stringBuilder.Append("using " + request.Usings[i] + Environment.NewLine);
        }

        stringBuilder.Append(Environment.NewLine);

        stringBuilder.Append($"namespace {request.Namespace}");
        stringBuilder.Append(Environment.NewLine);

        stringBuilder.Append('{' + Environment.NewLine);

        stringBuilder.Append('\t' +
                             $"public class Delete{request.Name}Input : EntityDto<{GetPrimaryKey(request.PrimaryKeyType)}>" +
                             Environment.NewLine);
        stringBuilder.Append('\t');
        stringBuilder.Append("{" + Environment.NewLine);

        stringBuilder.Append('\t');
        stringBuilder.Append("}");
        stringBuilder.Append(Environment.NewLine);
        stringBuilder.Append("}");

        return stringBuilder.ToString();
    }

    private string GeneratePartOutputDto(Entity request)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append("using Abp.Application.Services.Dto;" + Environment.NewLine);
        for (var i = 0; i < request.Usings.Count; i++)
        {
            stringBuilder.Append("using " + request.Usings[i] + Environment.NewLine);
        }

        stringBuilder.Append(Environment.NewLine);

        stringBuilder.Append($"namespace {request.Namespace}");
        stringBuilder.Append(Environment.NewLine);

        stringBuilder.Append('{' + Environment.NewLine);

        stringBuilder.Append('\t' +
                             $"public class {request.Name}PartOutput : EntityDto<{GetPrimaryKey(request.PrimaryKeyType)}>" +
                             Environment.NewLine);
        stringBuilder.Append('\t');
        stringBuilder.Append("{" + Environment.NewLine);
        for (var i = 0; i < request.Properties.Count; i++)
        {
            var property = request.Properties[i];

            stringBuilder.Append('\t');
            stringBuilder.Append('\t');

            if (!property.IsRelationalProperty)
            {
                stringBuilder.Append($"public {property.Type + (property.Nullable ? "? " : " ")} " +
                                     property.Name + " { get; set; }" + Environment.NewLine);
            }
            else
            {
                stringBuilder.Append(
                    $"public {GetPrimaryKey(property.RelationalEntityPrimaryKeyType) + (property.Nullable ? "? " : " ")}" +
                    property.RelationalEntityName + "Id { get; set; }" + Environment.NewLine);
            }
        }

        stringBuilder.Append('\t');
        stringBuilder.Append("}");
        stringBuilder.Append(Environment.NewLine);
        stringBuilder.Append("}");

        return stringBuilder.ToString();
    }

    private string GenerateFullOutputDto(Entity request)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append("using Abp.Application.Services.Dto;" + Environment.NewLine);
        for (var i = 0; i < request.Usings.Count; i++)
        {
            stringBuilder.Append("using " + request.Usings[i] + Environment.NewLine);
        }

        stringBuilder.Append(Environment.NewLine);

        stringBuilder.Append($"namespace {request.Namespace}");
        stringBuilder.Append(Environment.NewLine);

        stringBuilder.Append('{' + Environment.NewLine);

        stringBuilder.Append('\t' +
                             $"public class {request.Name}FullOutput : EntityDto<{GetPrimaryKey(request.PrimaryKeyType)}>" +
                             Environment.NewLine);
        stringBuilder.Append('\t');
        stringBuilder.Append("{" + Environment.NewLine);
        for (var i = 0; i < request.Properties.Count; i++)
        {
            var property = request.Properties[i];

            stringBuilder.Append('\t');
            stringBuilder.Append('\t');

            if (!property.IsRelationalProperty)
            {
                stringBuilder.Append($"public {property.Type + (property.Nullable ? "? " : " ")}" +
                                     property.Name + " { get; set; }" + Environment.NewLine);
            }
            else
            {
                if (property.RelationType == RelationType.OneToOne)
                {
                    // public int LineId { get; set; }
                    // public virtual Line Line { get; set; }
                    stringBuilder.Append(
                        $"public {GetPrimaryKey(property.RelationalEntityPrimaryKeyType) + (property.Nullable ? "? " : " ")} " +
                        property.RelationalEntityName + "Id { get; set; }" + Environment.NewLine);

                    stringBuilder.Append('\t');
                    stringBuilder.Append('\t');

                    stringBuilder.Append(
                        $"public {property.RelationalEntityName + "PartOutput" + (property.Nullable ? "? " : " ")} " +
                        property.Name + " { get; set; }" + Environment.NewLine);
                }
                else // OneToMany
                {
                    stringBuilder.Append(
                        $"public List<{property.RelationalEntityName + "PartOutput"}>" +
                        property.Name.Pluralize() + " { get; set; }" + Environment.NewLine);
                }
            }
        }

        stringBuilder.Append('\t');
        stringBuilder.Append("}");
        stringBuilder.Append(Environment.NewLine);
        stringBuilder.Append("}");

        return stringBuilder.ToString();
    }

    private string GenerateCreateInputDto(Entity request)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append("using Abp.Application.Services.Dto;" + Environment.NewLine);
        for (var i = 0; i < request.Usings.Count; i++)
        {
            stringBuilder.Append("using " + request.Usings[i] + Environment.NewLine);
        }

        stringBuilder.Append(Environment.NewLine);

        stringBuilder.Append($"namespace {request.Namespace}");
        stringBuilder.Append(Environment.NewLine);

        stringBuilder.Append('{' + Environment.NewLine);

        stringBuilder.Append('\t' +
                             $"public class Create{request.Name}Input" +
                             Environment.NewLine);
        stringBuilder.Append('\t');
        stringBuilder.Append("{" + Environment.NewLine);
        for (var i = 0; i < request.Properties.Count; i++)
        {
            var property = request.Properties[i];

            stringBuilder.Append('\t');
            stringBuilder.Append('\t');

            if (!property.IsRelationalProperty)
            {
                stringBuilder.Append($"public {property.Type + (property.Nullable ? "? " : " ")}" +
                                     property.Name + " { get; set; }" + Environment.NewLine);
            }
            else
            {
                if (property.RelationType == RelationType.OneToOne)
                {
                    // public int LineId { get; set; }
                    stringBuilder.Append(
                        $"public {GetPrimaryKey(property.RelationalEntityPrimaryKeyType) + (property.Nullable ? "? " : " ")} " +
                        property.RelationalEntityName + "Id { get; set; }" + Environment.NewLine);
                }
                else // OneToMany
                {
                    stringBuilder.Append(
                        $"public List<{GetPrimaryKey(property.RelationalEntityPrimaryKeyType)}>{(property.Nullable ? "? " : " ")}" +
                        property.Name.Pluralize() + " { get; set; }" + Environment.NewLine);
                }
            }
        }

        stringBuilder.Append('\t');
        stringBuilder.Append("}");
        stringBuilder.Append(Environment.NewLine);
        stringBuilder.Append("}");

        return stringBuilder.ToString();
    }

    private string GenerateUpdateInputDto(Entity request)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append("using Abp.Application.Services.Dto;" + Environment.NewLine);
        for (var i = 0; i < request.Usings.Count; i++)
        {
            stringBuilder.Append("using " + request.Usings[i] + Environment.NewLine);
        }

        stringBuilder.Append(Environment.NewLine);

        stringBuilder.Append($"namespace {request.Namespace}");
        stringBuilder.Append(Environment.NewLine);

        stringBuilder.Append('{' + Environment.NewLine);

        stringBuilder.Append('\t' +
                             $"public class Update{request.Name}Input : EntityDto<{GetPrimaryKey(request.PrimaryKeyType)}>" +
                             Environment.NewLine);
        stringBuilder.Append('\t');
        stringBuilder.Append("{" + Environment.NewLine);
        for (var i = 0; i < request.Properties.Count; i++)
        {
            var property = request.Properties[i];

            stringBuilder.Append('\t');
            stringBuilder.Append('\t');

            if (!property.IsRelationalProperty)
            {
                stringBuilder.Append($"public {property.Type + (property.Nullable ? "? " : " ")}" +
                                     property.Name + " { get; set; }" + Environment.NewLine);
            }
            else
            {
                if (property.RelationType == RelationType.OneToOne)
                {
                    // public int LineId { get; set; }
                    stringBuilder.Append(
                        $"public {GetPrimaryKey(property.RelationalEntityPrimaryKeyType) + (property.Nullable ? "? " : " ")} " +
                        property.RelationalEntityName + "Id { get; set; }" + Environment.NewLine);
                }
                else // OneToMany
                {
                    stringBuilder.Append(
                        $"public List<{GetPrimaryKey(property.RelationalEntityPrimaryKeyType)}>{(property.Nullable ? "? " : " ")}" +
                        property.Name.Pluralize() + " { get; set; }" + Environment.NewLine);
                }
            }
        }

        stringBuilder.Append('\t');
        stringBuilder.Append("}");
        stringBuilder.Append(Environment.NewLine);
        stringBuilder.Append("}");

        return stringBuilder.ToString();
    }

    private string GetPrimaryKey(PrimaryKeyType primaryKeyType)
    {
        return primaryKeyType switch
        {
            PrimaryKeyType.Int => "int",
            PrimaryKeyType.Long => "long",
            PrimaryKeyType.Guid => "Guid",
            _ => throw new Exception("PrimaryKey does not match any enum type!")
        };
    }

    #endregion
}