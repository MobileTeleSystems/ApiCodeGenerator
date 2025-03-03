using NJsonSchema;
using NJsonSchema.References;

namespace ApiCodeGenerator.AsyncApi.DOM;

/// <summary>
/// Base object for referenced objects.
/// </summary>
public abstract class RefObject : IJsonReference
{
    IJsonReference IJsonReference.ActualObject => this;

    object? IJsonReference.PossibleRoot => null;

    string? IJsonReferenceBase.ReferencePath { get; set; }

    IJsonReference? IJsonReferenceBase.Reference { get; set; }

    string? IDocumentPathProvider.DocumentPath { get; set; }
}
