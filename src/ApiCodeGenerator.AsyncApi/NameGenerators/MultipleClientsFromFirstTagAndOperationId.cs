using ApiCodeGenerator.AsyncApi.DOM;

namespace ApiCodeGenerator.AsyncApi.NameGenerators;

public class MultipleClientsFromFirstTagAndOperationId : IOperationNameGenerator
{
    public string GetClientName(AsyncApiDocument document, NamedReference<Operation> operation)
        => operation.ActualObject.Tags?.FirstOrDefault()?.ActualObject.Name ?? string.Empty;

    public string GetOperationName(AsyncApiDocument document, NamedReference<Operation> operation)
        => operation.ObjectId!;
}
