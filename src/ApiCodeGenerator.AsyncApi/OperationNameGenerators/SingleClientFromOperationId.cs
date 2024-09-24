using ApiCodeGenerator.AsyncApi.DOM;

namespace ApiCodeGenerator.AsyncApi.OperationNameGenerators;

public class SingleClientFromOperationId : IOperationNameGenerator
{
    public string GetClientName(AsyncApiDocument document, string path, bool subscribe, Operation operation)
        => string.Empty;

    public string GetOperationName(AsyncApiDocument document, string path, bool subscribe, Operation operation)
        => operation.OperationId
            ?? throw new InvalidOperationException($"Get operation name failed. OperationId in channel '{path}' not set.");
}
