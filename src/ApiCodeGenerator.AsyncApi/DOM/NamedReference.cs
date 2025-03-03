using NJsonSchema.References;

namespace ApiCodeGenerator.AsyncApi.DOM;

public sealed class NamedReference<T> : Reference<T>
    where T : IJsonReference
{
    public NamedReference()
    {
    }

    public string? ObjectId { get; internal set; }
}
