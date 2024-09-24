using Newtonsoft.Json;
using NJsonSchema;
using NJsonSchema.References;

namespace ApiCodeGenerator.AsyncApi.DOM;

public class RefObject<T> : IJsonReference
where T : RefObject<T>
{
    [JsonProperty("$ref", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
    public string? ReferencePath { get; set; }

    [JsonIgnore]
    public T? Reference
    {
        get => (T?)((IJsonReference)this).Reference;
        set => ((IJsonReference)this).Reference = value;
    }

    [JsonIgnore]
    public T ActualObject => Reference ?? (T)this;

    [JsonIgnore]
    IJsonReference IJsonReference.ActualObject => ActualObject;

    [JsonIgnore]
    object? IJsonReference.PossibleRoot { get; }

    [JsonIgnore]
    IJsonReference? IJsonReferenceBase.Reference { get; set; }

    [JsonIgnore]
    string? IDocumentPathProvider.DocumentPath { get; set; }
}
