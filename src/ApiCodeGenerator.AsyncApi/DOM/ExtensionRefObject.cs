using Newtonsoft.Json;
using NJsonSchema;
using NJsonSchema.References;

namespace ApiCodeGenerator.AsyncApi.DOM;

/// <summary>
/// Base object for referenced objects and may be extended.
/// </summary>
public class ExtensionRefObject : JsonExtensionObject, IJsonReference
{
    [JsonIgnore]
    IJsonReference IJsonReference.ActualObject => this;

    [JsonIgnore]
    object? IJsonReference.PossibleRoot { get; }

    [JsonIgnore]
    IJsonReference? IJsonReferenceBase.Reference { get; set; }

    string? IJsonReferenceBase.ReferencePath { get; set; }

    [JsonIgnore]
    string? IDocumentPathProvider.DocumentPath { get; set; }
}
