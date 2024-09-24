using ApiCodeGenerator.AsyncApi.DOM;

namespace ApiCodeGenerator.AsyncApi;

/// <summary>Generates the client and operation name for a given operation.</summary>
public interface IOperationNameGenerator
{
    /// <summary>Gets the client name for a given operation (may be empty).</summary>
    /// <param name="document">The Swagger document.</param>
    /// <param name="path">The Channel path.</param>
    /// <param name="subscribe">True if subscribe operation.</param>
    /// <param name="operation">The operation.</param>
    /// <returns>The client name.</returns>
    string GetClientName(AsyncApiDocument document, string path, bool subscribe, Operation operation);

    /// <summary>Gets the operation name for a given operation.</summary>
    /// <param name="document">The Swagger document.</param>
    /// <param name="path">The Channel path.</param>
    /// <param name="subscribe">True if subscribe operation.</param>
    /// <param name="operation">The operation.</param>
    /// <returns>The operation name.</returns>
    string GetOperationName(AsyncApiDocument document, string path, bool subscribe, Operation operation);
}
