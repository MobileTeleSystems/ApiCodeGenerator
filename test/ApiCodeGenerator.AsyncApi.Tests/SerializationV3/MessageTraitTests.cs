using ApiCodeGenerator.AsyncApi.DOM;
using ApiCodeGenerator.AsyncApi.DOM.Traits;
using NJsonSchema.References;

namespace ApiCodeGenerator.AsyncApi.Tests.SerializationV3;

public class MessageTraitTests : TestBase
{
    private const string MessageTraitsName = "mt";
    private const string MessageTraitsDefinition = $"""
      components:
        messageTraits:
          {MessageTraitsName}:
      """;

    [Test]
    public async Task ReadProperties()
    {
        var expected = new
        {
            Headers = new { SchemaFormat = "application/vnd.aai.asyncapi;version=3.0.0", Schema = new object() },
            CorrelationId = new { Location = "2dd758cc-325b-4c1c-ba69-203ecd84f18e" },
            ContentType = "4b25742d-b1b7-491e-90ab-89eb0e14ddd8",
            Name = "c616dbd9-5b60-4252-8b94-93598dc44c07",
            Title = "0ab88636-d582-477d-9c59-8c4fdae57794",
            Summary = "57c99c48-1d60-4340-b8ab-0cf92e7a2fa4",
            Description = "206d1eff-46d4-4617-ab28-f8d0cfd63ac2",
            Tags = new[] { new { Name = "tag" } },
            ExternalDocs = new { Url = "http://tempuri.org" },
            Bindings = new { Amqp = new { BindingVersion = "latest" } },
            Examples = new[] { new { Name = "abf70bc1-b535-451e-b18c-ea1dbdb8abeb" } },
        };

        var yaml = $"""
            {YamlHeader}
            {MessageTraitsDefinition}
                  {expected.ToYaml(indent: 6)}
            """;

        Reference<MessageTraits>? traitsRef = null;
        await ReadPropertiesTest(yaml, expected, d => traitsRef = d.Components.MessageTraits.GetValueOrNull(MessageTraitsName));

        Assert.NotNull(((IDocumentAware?)traitsRef?.ActualObject)?.Document);

    }

    [Test]
    public async Task ReadExtensions()
    {
        const string value = "3be8a059-ecba-4ecd-98ee-01f22b2a725f";
        var yaml =
            $$"""
            {{YamlHeader}}
            {{MessageTraitsDefinition}}
                  {0}: '{{value}}'
            """;

        await ReadExtensionsTest(yaml, value, d => d.Components.MessageTraits.GetValueOrNull(MessageTraitsName));
    }

    [Test]
    public async Task HeadersRef()
    {
        const string refPath = "#/components/schemas/h";
        var expected = new { Title = "1d69d02c-4aae-4235-8cdb-414929f8132b" };

        var yaml = $"""
        {YamlHeader}
        {MessageTraitsDefinition}
              headers:
                $ref: '{refPath}'
          schemas:
            h:
              {expected.ToYaml(indent: 6)}
        """;

        await MessageTraitsResolveReferenceTest(yaml, refPath, expected, o => o.Headers);
    }

    [Test]
    public async Task CorrelationIdRef()
    {
        const string refPath = "#/components/correlationIds/cid";
        var expected = new { Location = "5994d55b-efe8-4587-b28a-d98d5c859971" };

        var yaml = $"""
        {YamlHeader}
        {MessageTraitsDefinition}
              correlationId:
                $ref: '{refPath}'
          correlationIds:
            cid:
              {expected.ToYaml(indent: 6)}
        """;

        await MessageTraitsResolveReferenceTest(yaml, refPath, expected, o => o.CorrelationId);
    }

    [Test]
    public async Task TagsRef()
    {
        const string refPath = "#/components/tags/tag";
        var expected = new { Name = "5d9e4ea2-1fac-45fd-b37c-dfdfb62a9a74" };

        var yaml = $"""
        {YamlHeader}
        {MessageTraitsDefinition}
              tags:
                - $ref: '{refPath}'
          tags:
            tag:
              {expected.ToYaml(indent: 6)}
        """;

        await MessageTraitsResolveReferenceTest(yaml, refPath, expected, o =>
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
        {MessageTraitsDefinition}
              externalDocs:
                $ref: '{refPath}'
          externalDocs:
            d:
              {expected.ToYaml(indent: 6)}
        """;

        await MessageTraitsResolveReferenceTest(yaml, refPath, expected, o => o.ExternalDocs);
    }

    [Test]
    public async Task BindingsRef()
    {
        const string refPath = "#/components/messageBindings/b";
        var expected = new { Amqp = new { BindingVersion = "latest" } };

        var yaml = $"""
        {YamlHeader}
        {MessageTraitsDefinition}
              bindings:
                $ref: '{refPath}'
          messageBindings:
            b:
              {expected.ToYaml(indent: 6)}
        """;

        await MessageTraitsResolveReferenceTest(yaml, refPath, expected, o => o.Bindings);
    }

    private Task MessageTraitsResolveReferenceTest<T>(string yaml, string refPath, object expected, Func<MessageTraits, T?> getRef)
        where T : IJsonReference
    {
        return ResolveReferernceTest(yaml, refPath, expected, d =>
        {
            Assert.That(d.Components.MessageTraits, Is.Not.Null.And.ContainKey(MessageTraitsName));
            var op = d.Components.MessageTraits[MessageTraitsName];
            Assert.That(op, Is.Not.Null);
            return getRef(op.ActualObject);
        });
    }
}
