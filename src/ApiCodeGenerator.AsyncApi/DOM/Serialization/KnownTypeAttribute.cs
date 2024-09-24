namespace ApiCodeGenerator.AsyncApi.DOM.Serialization;

[AttributeUsage(System.AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
internal sealed class KnownTypeAttribute : Attribute
{
    public KnownTypeAttribute(string discriminatorValue, Type type)
    {
        DiscriminatorValue = discriminatorValue;
        Type = type;
    }

    public string DiscriminatorValue { get; }

    public Type Type { get; }
}
