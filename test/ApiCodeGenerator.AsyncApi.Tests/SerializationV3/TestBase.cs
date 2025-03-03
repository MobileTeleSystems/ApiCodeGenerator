using ApiCodeGenerator.AsyncApi.DOM;
using NJsonSchema;
using NJsonSchema.References;

namespace ApiCodeGenerator.AsyncApi.Tests.SerializationV3;

public abstract class TestBase
{
    protected const string YamlHeader = """
    asyncapi: '3.0.0'
    info:
      title: t
      version: '1.0'

    """;

    protected void RequiredPropertiesTest(string yaml)
    {
        var ex = Assert.ThrowsAsync<JsonSerializationException>(() => AsyncApiSerializer.FromYamlAsync(yaml));
        Assert.That(ex, Is.Not.Null);
        Assert.That(ex.Message, Does.StartWith("Required property "));
    }

    protected async Task ReadExtensionsTest(string yaml, string value, Func<AsyncApiDocument, object?> getter)
    {
        const string propName = "x-test";

        var document = await AsyncApiSerializer.FromYamlAsync(string.Format(yaml, propName));

        var obj = getter(document);
        Assert.That(obj, Is.Not.Null);
        if (obj is IJsonReference jr)
        {
            obj = jr.ActualObject;
        }

        Assert.That(obj, Is.Not.Null.And.InstanceOf<JsonExtensionObject>());
        var extObj = (JsonExtensionObject)obj!;
        Assert.That(extObj.ExtensionData, Is.Not.Null.And.ContainKey(propName));
        Assert.That(extObj.ExtensionData[propName], Is.EqualTo(value));
    }

    protected async Task ReadPropertiesTest(string yaml, object expected, Func<AsyncApiDocument, object?> getter)
    {
        var document = await AsyncApiSerializer.FromYamlAsync(yaml);

        var obj = getter(document);

        Assert.That(obj, Is.Not.Null);
        if (obj is IJsonReference jr)
        {
            obj = jr.ActualObject;
        }

        obj.ShouldDeepEqual(expected, IgnoreUnmatchedProperties);
    }

    protected async Task ResolveReferernceTest<T>(string yaml, string refPath, object expected, Func<AsyncApiDocument, T?> getRef)
        where T : IJsonReference
    {
        var document = await AsyncApiSerializer.FromYamlAsync(yaml);

        var objRef = getRef(document);
        Assert.That(objRef, Is.Not.Null);
        Assert.That(objRef.ReferencePath, Is.EqualTo(refPath));
        objRef.ActualObject.ShouldDeepEqual(expected, IgnoreUnmatchedProperties);
    }
}
