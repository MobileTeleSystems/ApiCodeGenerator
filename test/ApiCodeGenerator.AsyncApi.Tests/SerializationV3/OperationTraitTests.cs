using ApiCodeGenerator.AsyncApi.DOM;
using ApiCodeGenerator.AsyncApi.DOM.Traits;
using NJsonSchema.References;

namespace ApiCodeGenerator.AsyncApi.Tests.SerializationV3;

public class OperationTraitTests : TestBase
{
    private const string OperationTraitsName = "ot";
    private const string OperationTraitsDefinition = $"""
      components:
        operationTraits:
          {OperationTraitsName}:
      """;

    [Test]
    public async Task ReadProperties()
    {
        var expected = new
        {
            Title = "0796b4fe-a20b-4614-ba5e-e204252316e3",
            Summary = "9ced3caa-39a9-4ab1-b382-9164fdbbb634",
            Description = "fd67aa43-f1a9-4cb7-9c7f-748706b00d30",
            Security = new[] { new { Type = "plain" } },
            Tags = new[] { new { Name = "tag" } },
            ExternalDocs = new { Url = "http://tempuri.org" },
            Bindings = new { Amqp = new { Expiration = 2 } },
        };

        var yaml = $"""
            {YamlHeader}
            {OperationTraitsDefinition}
                  {expected.ToYaml(indent: 6)}
            """;

        Reference<OperationTraits>? traitsRef = null;
        await ReadPropertiesTest(yaml, expected, d => traitsRef = d.Components.OperationTraits.GetValueOrNull(OperationTraitsName));

        Assert.NotNull(((IDocumentAware?)traitsRef?.ActualObject)?.Document);
    }

    [Test]
    public async Task ReadExtensions()
    {
        const string value = "6a85de5c-84f2-4b9e-9e37-498953703255";
        var yaml =
            $$"""
            {{YamlHeader}}
            {{OperationTraitsDefinition}}
                  {0}: '{{value}}'
            """;

        await ReadExtensionsTest(yaml, value, d => d.Components.OperationTraits.GetValueOrNull(OperationTraitsName));
    }

    [Test]
    public async Task SecurityRef()
    {
        const string refPath = "#/components/securitySchemes/plain";
        var expected = new { Type = "plain" };

        var yaml = $"""
        {YamlHeader}
        {OperationTraitsDefinition}
              security:
                - $ref: '{refPath}'
          securitySchemes:
            plain:
              {expected.ToYaml(indent: 6)}
        """;

        await OperationTraitsResolveReferenceTest(yaml, refPath, expected, o =>
        {
            Assert.That(o.Security, Is.Not.Null.And.Count.EqualTo(1));
            return o.Security.Single();
        });
    }

    [Test]
    public async Task TagsRef()
    {
        const string refPath = "#/components/tags/tag";
        var expected = new { Name = "2afe15f5-0aab-4ad7-af7a-28734bb34cef" };

        var yaml = $"""
        {YamlHeader}
        {OperationTraitsDefinition}
              tags:
                - $ref: '{refPath}'
          tags:
            tag:
              {expected.ToYaml(indent: 6)}
        """;

        await OperationTraitsResolveReferenceTest(yaml, refPath, expected, o =>
        {
            Assert.That(o.Tags, Is.Not.Null.And.Count.EqualTo(1));
            return o.Tags.Single();
        });
    }

    [Test]
    public async Task ExternalDocsRef()
    {
        const string refPath = "#/components/externalDocs/d";
        var expected = new { Url = "http://some.url" };

        var yaml = $"""
        {YamlHeader}
        {OperationTraitsDefinition}
              externalDocs:
                $ref: '{refPath}'
          externalDocs:
            d:
              {expected.ToYaml(indent: 6)}
        """;

        await OperationTraitsResolveReferenceTest(yaml, refPath, expected, o => o.ExternalDocs);
    }

    [Test]
    public async Task BindingsRef()
    {
        const string refPath = "#/components/operationBindings/b";
        var expected = new { Amqp = new { Ack = true } };

        var yaml = $"""
        {YamlHeader}
        {OperationTraitsDefinition}
              bindings:
                $ref: '{refPath}'
          operationBindings:
            b:
              {expected.ToYaml(indent: 6)}
        """;

        await OperationTraitsResolveReferenceTest(yaml, refPath, expected, o => o.Bindings);
    }

    private Task OperationTraitsResolveReferenceTest<T>(string yaml, string refPath, object expected, Func<OperationTraits, Reference<T>?> getRef)
        where T : IJsonReference
    {
        return ResolveReferernceTest(yaml, refPath, expected, d =>
        {
            Assert.That(d.Components.OperationTraits, Is.Not.Null.And.ContainKey(OperationTraitsName));
            var op = d.Components.OperationTraits[OperationTraitsName];
            Assert.That(op, Is.Not.Null);
            return getRef(op.ActualObject);
        });
    }
}
