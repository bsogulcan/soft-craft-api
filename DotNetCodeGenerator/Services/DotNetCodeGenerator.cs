using System.Text;
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

        stringBuilder.Append(Environment.NewLine);

        stringBuilder.Append($"namespace {request.Namespace}");
        stringBuilder.Append(Environment.NewLine);

        stringBuilder.Append('{' + Environment.NewLine);

        stringBuilder.Append('\t' + $"public class {request.Name} : ");

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

        stringBuilder.Append(Environment.NewLine);

        stringBuilder.Append('\t' + "{");
        stringBuilder.Append(Environment.NewLine);

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
                //TODO: Write Relational properties
                if (property.RelationType == RelationType.OneToOne)
                {
                    // public int LineId { get; set; }
                    // public virtual Line Line { get; set; }
                    stringBuilder.Append(
                        $"public {GetPrimaryKey(property.RelationalEntityPrimaryKeyType) + (property.Nullable ? "? " : " ")}" +
                        property.RelationalEntityName + "Id { get; set; }" + Environment.NewLine);

                    stringBuilder.Append('\t');
                    stringBuilder.Append('\t');

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

        stringBuilder.Append('\t');
        stringBuilder.Append("}");
        stringBuilder.Append(Environment.NewLine);
        stringBuilder.Append("}");

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
}