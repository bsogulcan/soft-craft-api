using Grpc.Core;

namespace TypeScriptCodeGenerator.Services;

public class TypeScriptCodeGeneratorService : TypeScriptCodeGenerator.TypeScriptCodeGeneratorBase
{
    private readonly ILogger<TypeScriptCodeGeneratorService> _logger;

    public TypeScriptCodeGeneratorService(ILogger<TypeScriptCodeGeneratorService> logger)
    {
        _logger = logger;
    }

    public override Task<DtoResult> CreateDtos(Entity request, ServerCallContext context)
    {
        return base.CreateDtos(request, context);
    }
}