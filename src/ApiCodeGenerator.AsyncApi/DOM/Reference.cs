using Newtonsoft.Json;
using NJsonSchema;
using NJsonSchema.References;

namespace ApiCodeGenerator.AsyncApi.DOM;

/// <summary>
/// Reference to object.
/// </summary>
/// <remarks>If property allows reference to an object, use this class as the property type.</remarks>
/// <typeparam name="T">Target object type.</typeparam>
[JsonConverter(typeof(Serialization.RefObjectConverter))]
public class Reference<T> : IJsonReference
    where T : IJsonReference
{
    public Reference()
    {
    }

    protected Reference(T actualObj)
    {
        ((IJsonReferenceBase)this).Reference = actualObj;
    }

    [JsonIgnore]
    public T ActualObject => (T)((IJsonReferenceBase)this).Reference!;

    public string? ReferencePath { get; set; }

    [JsonIgnore]
    IJsonReference IJsonReference.ActualObject => ActualObject;

    [JsonIgnore]
    object? IJsonReference.PossibleRoot { get; }

    [JsonIgnore]
    IJsonReference? IJsonReferenceBase.Reference { get; set; }

    [JsonIgnore]
    string? IDocumentPathProvider.DocumentPath { get; set; }

    public static implicit operator T(Reference<T> reference)
    {
        return reference.ActualObject;
    }

    public static implicit operator Reference<T>(T actualObj) => new Reference<T>(actualObj);
}
