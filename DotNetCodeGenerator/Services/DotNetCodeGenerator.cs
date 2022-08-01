using System.Text;
using Extensions;
using Grpc.Core;
using Humanizer;
using Volo.Abp.Tracing;

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
        stringBuilder.Append("using System;" + Environment.NewLine);
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

        switch (request.TenantType)
        {
            case TenantType.May:
            {
                //public int? TenantId { get; set; }
                stringBuilder.InsertTab(2).Append("public int? TenantId { get; set; }").NewLine();
            }
                break;
            case TenantType.Must:
            {
                //public int TenantId { get; set; }
                stringBuilder.InsertTab(2).Append("public int TenantId { get; set; }").NewLine();
            }
                break;
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
            .NewLine()
            .Append("using System.Threading.Tasks;")
            .NewLine()
            .Append("using Abp.Application.Services.Dto;")
            .NewLine(2);

        appServiceInterfaceStringBuilder.Append($"namespace {request.ProjectName}.Domain.{request.EntityName}")
            .NewLine()
            .Append('{')
            .NewLine()
            .InsertTab();

        appServiceInterfaceStringBuilder
            .Append(
                $"public interface I{request.EntityName}AppService:IAsyncCrudAppService<{request.EntityName}FullOutput,{request.EntityType},Get{request.EntityName}Input,Create{request.EntityName}Input,Update{request.EntityName}Input,Get{request.EntityName}Input,Delete{request.EntityName}Input>")
            .NewLine()
            .InsertTab()
            .Append('{')
            .NewLine();


        foreach (var relationalProperty in request.Properties.Where(x =>
                     x.IsRelationalProperty && x.RelationType == RelationType.OneToOne))
        {
            appServiceInterfaceStringBuilder.InsertTab(2)
                .Append(
                    $"Task<PagedResultDto<{request.EntityName}FullOutput>> Get{request.EntityName.Pluralize()}By{relationalProperty.Name}Id({relationalProperty.Type} {relationalProperty.Name.ToCamelCase()}Id);")
                .NewLine();
        }

        var releatedEntities = request.Properties.Where(x =>
            x.IsRelationalProperty && x.RelationType == RelationType.OneToOne);

        if (releatedEntities.Count() > 1)
        {
            appServiceInterfaceStringBuilder.InsertTab(2)
                .Append(
                    $"Task<PagedResultDto<{request.EntityName}FullOutput>> GetAll{request.EntityName.Pluralize()}Filtered(");

            bool isFirst = true;
            foreach (var relationalProperty in releatedEntities)
            {
                if (isFirst)
                {
                    appServiceInterfaceStringBuilder.Append(
                        $"{relationalProperty.Type}? {relationalProperty.Name.ToCamelCase()}Id");
                    isFirst = false;
                }
                else
                {
                    appServiceInterfaceStringBuilder.Append(
                        $", {relationalProperty.Type}? {relationalProperty.Name.ToCamelCase()}Id");
                }
            }

            appServiceInterfaceStringBuilder.Append(");").NewLine();
        }

        appServiceInterfaceStringBuilder
            .NewLine()
            .InsertTab()
            .Append('}');
        appServiceInterfaceStringBuilder
            .NewLine()
            .Append('}');

        result.AppServiceInterfaceStringify = appServiceInterfaceStringBuilder.ToString();


        var permissionNameStringBuilder = new StringBuilder();
        permissionNameStringBuilder.Append($"public const string {request.EntityName} = \"{request.EntityName}\";")
            .NewLine()
            .Append(
                $"public const string {request.EntityName}_Create = \"{request.EntityName}.Create\";")
            .NewLine()
            .Append($"public const string {request.EntityName}_Update = \"{request.EntityName}.Update\";")
            .NewLine()
            .Append($"public const string {request.EntityName}_GetList = \"{request.EntityName}.GetList\";")
            .NewLine()
            .Append($"public const string {request.EntityName}_Get = \"{request.EntityName}.Get\";")
            .NewLine()
            .Append($"public const string {request.EntityName}_Delete = \"{request.EntityName}.Delete\";")
            .NewLine()
            .Append($"public const string {request.EntityName}_Navigation = \"{request.EntityName}.Navigation\";");

        result.PermissionNames = permissionNameStringBuilder.ToString();

        var authorizationProviderStringBuilder = new StringBuilder();
        authorizationProviderStringBuilder.Append(
                $"context.CreatePermission(PermissionNames.{request.EntityName}, L(PermissionNames.{request.EntityName}));")
            .NewLine()
            .Append(
                $"context.CreatePermission(PermissionNames.{request.EntityName}_Create, L(PermissionNames.{request.EntityName}_Create));")
            .NewLine()
            .Append(
                $"context.CreatePermission(PermissionNames.{request.EntityName}_Update, L(PermissionNames.{request.EntityName}_Update));")
            .NewLine()
            .Append(
                $"context.CreatePermission(PermissionNames.{request.EntityName}_GetList, L(PermissionNames.{request.EntityName}_GetList));")
            .NewLine()
            .Append(
                $"context.CreatePermission(PermissionNames.{request.EntityName}_Get, L(PermissionNames.{request.EntityName}_Get));")
            .NewLine()
            .Append(
                $"context.CreatePermission(PermissionNames.{request.EntityName}_Delete, L(PermissionNames.{request.EntityName}_Delete));")
            .NewLine()
            .Append(
                $"context.CreatePermission(PermissionNames.{request.EntityName}_Navigation, L(PermissionNames.{request.EntityName}_Navigation));");

        result.AuthorizationProviders = authorizationProviderStringBuilder.ToString();
        var appServiceStringBuilder = new StringBuilder();
        appServiceStringBuilder.Append("using Abp.Application.Services;")
            .NewLine()
            .Append($"using Abp.Application.Services.Dto;")
            .NewLine()
            .Append("using Abp.Authorization;")
            .NewLine()
            .Append("using System.Collections.Generic;")
            .NewLine()
            .Append("using System.Linq;")
            .NewLine()
            .Append("using System.Linq.Dynamic.Core;")
            .NewLine()
            .Append("using System.Threading.Tasks;")
            .NewLine()
            .Append($"using {request.ProjectName}.Authorization;")
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

        appServiceStringBuilder.Append($"[AbpAuthorize(PermissionNames.{request.EntityName})]")
            .NewLine()
            .InsertTab();

        appServiceStringBuilder.Append(
                $"public class {request.EntityName}AppService : AsyncCrudAppService<Entities.{request.EntityName}, {request.EntityName}FullOutput, {request.EntityType}, Get{request.EntityName}Input, Create{request.EntityName}Input, Update{request.EntityName}Input, Get{request.EntityName}Input, Delete{request.EntityName}Input>, I{request.EntityName}AppService")
            .NewLine()
            .InsertTab()
            .Append('{')
            .NewLine();

        appServiceStringBuilder.InsertTab(2)
            .Append(
                $"public {request.EntityName}AppService(I{request.EntityName}Repository {request.EntityName.ToCamelCase()}Repository) : base({request.EntityName.ToCamelCase()}Repository)")
            .NewLine()
            .InsertTab(2)
            .Append('{')
            .NewLine();

        appServiceStringBuilder.InsertTab(3)
            .Append($"CreatePermissionName = PermissionNames.{request.EntityName}_Create;")
            .NewLine();
        appServiceStringBuilder.InsertTab(3)
            .Append($"UpdatePermissionName = PermissionNames.{request.EntityName}_Update;")
            .NewLine();
        appServiceStringBuilder.InsertTab(3)
            .Append($"DeletePermissionName = PermissionNames.{request.EntityName}_Delete;")
            .NewLine();
        appServiceStringBuilder.InsertTab(3)
            .Append($"GetPermissionName = PermissionNames.{request.EntityName}_Get;")
            .NewLine();
        appServiceStringBuilder.InsertTab(3)
            .Append($"GetAllPermissionName = PermissionNames.{request.EntityName}_GetList;")
            .NewLine();

        appServiceStringBuilder.InsertTab(2)
            .Append('}')
            .NewLine();

        foreach (var relationalProperty in request.Properties.Where(x =>
                     x.IsRelationalProperty && x.RelationType == RelationType.OneToOne))
        {
            appServiceStringBuilder.NewLine().InsertTab(2)
                .Append($"[AbpAuthorize(PermissionNames.{request.EntityName}_GetList)]")
                .NewLine().InsertTab(2)
                .Append(
                    $"public async Task<PagedResultDto<{request.EntityName}FullOutput>> Get{request.EntityName.Pluralize()}By{relationalProperty.Name}Id({relationalProperty.Type} {relationalProperty.Name.ToCamelCase()}Id)")
                .NewLine().InsertTab(2).Append("{")
                .NewLine().InsertTab(3)
                .Append(
                    $"var items = await Repository.GetAllListAsync(x => x.{relationalProperty.Name}Id == {relationalProperty.Name.ToCamelCase()}Id);")
                .NewLine().InsertTab(3).Append($"return new PagedResultDto<{request.EntityName}FullOutput>")
                .NewLine().InsertTab(3).Append("{")
                .NewLine().InsertTab(4).Append("TotalCount = items.Count(),")
                .NewLine().InsertTab(4).Append($"Items = ObjectMapper.Map<List<{request.EntityName}FullOutput>>(items)")
                .NewLine().InsertTab(3).Append("};")
                .NewLine().InsertTab(2).Append("}")
                .NewLine();
        }

        if (releatedEntities.Count() > 1)
        {
            appServiceStringBuilder.NewLine().InsertTab(2)
                .Append($"[AbpAuthorize(PermissionNames.{request.EntityName}_GetList)]")
                .NewLine().InsertTab(2)
                .Append(
                    $"public async Task<PagedResultDto<{request.EntityName}FullOutput>> GetAll{request.EntityName.Pluralize()}Filtered(");


            bool isFirst = true;
            foreach (var relationalProperty in releatedEntities)
            {
                if (isFirst)
                {
                    appServiceStringBuilder.Append(
                        $"{relationalProperty.Type}? {relationalProperty.Name.ToCamelCase()}Id");
                    isFirst = false;
                }
                else
                {
                    appServiceStringBuilder.Append(
                        $", {relationalProperty.Type}? {relationalProperty.Name.ToCamelCase()}Id");
                }
            }

            appServiceStringBuilder.Append(")")
                .NewLine()
                .InsertTab(2)
                .Append("{")
                .NewLine()
                .InsertTab(3)
                .Append("var items = Repository.GetAll();");

            foreach (var relationalProperty in releatedEntities)
            {
                appServiceStringBuilder
                    .NewLine(2)
                    .InsertTab(3)
                    .Append($"if ({relationalProperty.Name.ToCamelCase()}Id.HasValue)")
                    .NewLine()
                    .InsertTab(4)
                    .Append(
                        $"items = items.Where(x => x.{relationalProperty.Name}Id == {relationalProperty.Name.ToCamelCase()}Id.Value);");
            }

            appServiceStringBuilder
                .NewLine(2).InsertTab(3).Append($"return new PagedResultDto<{request.EntityName}FullOutput>")
                .NewLine().InsertTab(3).Append("{")
                .NewLine().InsertTab(4).Append("TotalCount = items.Count(),")
                .NewLine().InsertTab(4).Append($"Items = ObjectMapper.Map<List<{request.EntityName}FullOutput>>(items)")
                .NewLine().InsertTab(3).Append("};")
                .NewLine().InsertTab(2).Append("}")
                .NewLine();
        }

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

        foreach (var property in request.Properties.Where(x =>
                     x.IsRelationalProperty && x.RelationType == RelationType.OneToOne && x.OneToOne))
        {
            stringBuilder.NewLine(2)
                .InsertTab(3)
                .Append($"builder.HasOne(x => x.{property.Name})")
                .NewLine().InsertTab(4)
                .Append($".WithOne(y => y.{property.RelationalPropertyName})")
                .NewLine().InsertTab(4)
                .Append($".HasForeignKey<{property.RelationalEntityName}>(y => y.{request.Name}Id);")
                .NewLine();
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
        stringBuilder.Append("using System;" + Environment.NewLine);
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
        stringBuilder.Append("using System;" + Environment.NewLine);
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
        stringBuilder.Append("using System;" + Environment.NewLine);
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
            if (!property.IsRelationalProperty)
            {
                stringBuilder.InsertTab(2);
                stringBuilder.Append($"public {property.Type + (property.Nullable ? "? " : " ")} " +
                                     property.Name + " { get; set; }" + Environment.NewLine);
            }
            else
            {
                if (property.RelationType == RelationType.OneToOne)
                {
                    stringBuilder.InsertTab(2);
                    stringBuilder.Append(
                        $"public {GetPrimaryKey(property.RelationalEntityPrimaryKeyType) + (property.Nullable ? "? " : " ")}" +
                        property.RelationalEntityName + "Id { get; set; }" + Environment.NewLine);

                    stringBuilder.InsertTab(2).Append(
                        $"public {property.RelationalEntityName + "PartOutput" + (property.Nullable ? "? " : " ")} " +
                        property.Name + " { get; set; }" + Environment.NewLine);
                }
                else // OneToMany
                {
                }
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
        stringBuilder.Append("using System;" + Environment.NewLine);
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

            stringBuilder.InsertTab(2);

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
        stringBuilder.Append("using System;" + Environment.NewLine);
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

            if (!property.IsRelationalProperty)
            {
                stringBuilder.InsertTab(2);
                stringBuilder.Append($"public {property.Type + (property.Nullable ? "? " : " ")}" +
                                     property.Name + " { get; set; }" + Environment.NewLine);
            }
            else
            {
                if (property.RelationType == RelationType.OneToOne)
                {
                    stringBuilder.InsertTab(2);
                    // public int LineId { get; set; }
                    stringBuilder.Append(
                        $"public {GetPrimaryKey(property.RelationalEntityPrimaryKeyType) + (property.Nullable ? "? " : " ")} " +
                        property.RelationalEntityName + "Id { get; set; }" + Environment.NewLine);
                }
                else // OneToMany
                {
                    //Note: Many relations types cannot insert now. Check this later
                    // stringBuilder.Append(
                    //     $"public List<{GetPrimaryKey(property.RelationalEntityPrimaryKeyType)}>{(property.Nullable ? "? " : " ")}" +
                    //     property.Name.Pluralize() + " { get; set; }" + Environment.NewLine);
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
        stringBuilder.Append("using System;" + Environment.NewLine);
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

            if (!property.IsRelationalProperty)
            {
                stringBuilder.InsertTab(2);
                stringBuilder.Append($"public {property.Type + (property.Nullable ? "? " : " ")}" +
                                     property.Name + " { get; set; }" + Environment.NewLine);
            }
            else
            {
                if (property.RelationType == RelationType.OneToOne)
                {
                    // public int LineId { get; set; }
                    stringBuilder.InsertTab(2);
                    stringBuilder.Append(
                        $"public {GetPrimaryKey(property.RelationalEntityPrimaryKeyType) + (property.Nullable ? "? " : " ")} " +
                        property.RelationalEntityName + "Id { get; set; }" + Environment.NewLine);
                }
                else // OneToMany
                {
                    // stringBuilder.Append(
                    //     $"public List<{GetPrimaryKey(property.RelationalEntityPrimaryKeyType)}>{(property.Nullable ? "? " : " ")}" +
                    //     property.Name.Pluralize() + " { get; set; }" + Environment.NewLine);
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