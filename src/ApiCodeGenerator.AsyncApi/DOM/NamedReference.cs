using NJsonSchema.References;

namespace ApiCodeGenerator.AsyncApi.DOM;

public sealed class NamedReference<T> : Reference<T>
    where T : IJsonReference
{
    public NamedReference()
    {
    }

    private NamedReference(T actualObject)
        : base(actualObject)
    {
    }

    public string? ObjectId { get; internal set; }

    public static implicit operator NamedReference<T>(T actualObj) => new NamedReference<T>(actualObj);
}
