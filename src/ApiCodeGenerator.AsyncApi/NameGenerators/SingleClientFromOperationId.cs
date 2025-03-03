using ApiCodeGenerator.AsyncApi.DOM;

namespace ApiCodeGenerator.AsyncApi.NameGenerators;

public class SingleClientFromOperationId : IOperationNameGenerator
{
    public string GetClientName(AsyncApiDocument document, NamedReference<Operation> operation)
        => string.Empty;

    public string GetOperationName(AsyncApiDocument document, NamedReference<Operation> operation)
        => operation.ObjectId!;
}
