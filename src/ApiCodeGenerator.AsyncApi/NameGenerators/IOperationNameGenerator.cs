using ApiCodeGenerator.AsyncApi.DOM;

namespace ApiCodeGenerator.AsyncApi.NameGenerators;

/// <summary>Generates the client and operation name for a given operation.</summary>
public interface IOperationNameGenerator
{
    /// <summary>Gets the client name for a given operation (may be empty).</summary>
    /// <param name="document">The Swagger document.</param>
    /// <param name="operation">The operation.</param>
    /// <returns>The client name.</returns>
    public string GetClientName(AsyncApiDocument document, NamedReference<Operation> operation);

    /// <summary>Gets the operation name for a given operation.</summary>
    /// <param name="document">The Swagger document.</param>
    /// <param name="operation">The operation.</param>
    /// <returns>The operation name.</returns>
    public string GetOperationName(AsyncApiDocument document, NamedReference<Operation> operation);
}
