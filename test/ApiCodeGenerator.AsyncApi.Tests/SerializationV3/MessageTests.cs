using ApiCodeGenerator.AsyncApi.DOM;
using NJsonSchema.References;

namespace ApiCodeGenerator.AsyncApi.Tests.SerializationV3;

public class MessageTests : TestBase
{
    private const string MessageName = "msg";

    private const string MessageDefinition = $"""
        components:
          messages:
            {MessageName}:
        """;

    [Test]
    public async Task ReadProperties()
    {
        var expected = new
        {
            Headers = new { SchemaFormat = "application/vnd.aai.asyncapi;version=3.0.0", Schema = new object() },
            Payload = new { SchemaFormat = "application/vnd.aai.asyncapi;version=3.0.0", Schema = new object() },
            CorrelationId = new { Location = "53e2420a-b0f2-4401-825b-b7bdd3a3df41" },
            ContentType = "7e0c3c05-4437-42b9-b52a-a1e85ec98a95",
            Name = "fff3e3a5-b266-43be-908a-04a47d626890",
            Title = "33629d50-2a44-409e-9687-700da06606de",
            Summary = "17e58801-223a-4b0d-abe2-2c95c492ab7b",
            Description = "7dc15aa6-4eb7-427b-9b64-d2bbf7c380b3",
            Tags = new[] { new { Name = "62b89c8d-cb2d-457e-9553-c7be93de7e71" } },
            ExternalDocs = new { Url = "039a707a-85c5-474e-bfe8-be156f813e8f" },
            Bindings = new { Amqp = new object() },
            Examples = new[] { new { Name = "59d53925-c20f-4cda-b819-ea9b28af10fc" } },
            Traits = new { Title = "11690732-96c8-498c-9f9f-b54d5278f118" },
        };

        var yaml = $"""
            {YamlHeader}
            {MessageDefinition}
                  {expected.ToYaml(indent: 6)}
            """;

        await ReadPropertiesTest(yaml, expected, d => d.Components.Messages.GetValueOrNull(MessageName));
    }

    [Test]
    public async Task HeadersRef()
    {
        const string refPath = "#/components/schemas/h";
        var expected = new { Title = "80313f49-cfa9-4e1c-aaf8-631c4144cf5c" };
        var yaml = $"""
            {YamlHeader}
            {MessageDefinition}
                  headers:
                    $ref: '{refPath}'
              schemas:
                h:
                  {expected.ToYaml(indent: 6)}
            """;

        await MessageResolveReferernceTest(yaml, refPath, expected, m => m.Headers);
    }

    [Test]
    public async Task PayloadRef()
    {
        const string refPath = "#/components/schemas/p";
        var expected = new { Title = "d1301309-a6b6-4fde-988c-e1503ffd60fe" };
        var yaml = $"""
            {YamlHeader}
            {MessageDefinition}
                  payload:
                    $ref: '{refPath}'
              schemas:
                p:
                  {expected.ToYaml(indent: 6)}
            """;

        await MessageResolveReferernceTest(yaml, refPath, expected, m => m.Payload);
    }

    [Test]
    public async Task CorrelationIdRef()
    {
        const string refPath = "#/components/correlationIds/cid";
        var expected = new { Location = "d1301309-a6b6-4fde-988c-e1503ffd60fe" };
        var yaml = $"""
            {YamlHeader}
            {MessageDefinition}
                  correlationId:
                    $ref: '{refPath}'
              correlationIds:
                cid:
                  {expected.ToYaml(indent: 6)}
            """;

        await MessageResolveReferernceTest(yaml, refPath, expected, m => m.CorrelationId);
    }

    [Test]
    public async Task TagsRef()
    {
        const string refPath = "#/components/tags/tag";
        var expected = new { Name = "70664197-f27f-4352-b3e8-a0a1672e3163" };

        var yaml = $"""
        {YamlHeader}
        {MessageDefinition}
              tags:
                - $ref: '{refPath}'
          tags:
            tag:
              {expected.ToYaml(indent: 6)}
        """;

        await MessageResolveReferernceTest(yaml, refPath, expected, o =>
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
        {MessageDefinition}
              externalDocs:
                $ref: '{refPath}'
          externalDocs:
            d:
              {expected.ToYaml(indent: 6)}
        """;

        await MessageResolveReferernceTest(yaml, refPath, expected, o => o.ExternalDocs);
    }

    [Test]
    public async Task BindingsRef()
    {
        const string refPath = "#/components/messageBindings/b";
        var expected = new { Amqp = new { BindingVersion = "latest" } };

        var yaml = $"""
        {YamlHeader}
        {MessageDefinition}
              bindings:
                $ref: '{refPath}'
          messageBindings:
            b:
              {expected.ToYaml(indent: 6)}
        """;

        await MessageResolveReferernceTest(yaml, refPath, expected, o => o.Bindings);
    }

    [Test]
    public async Task TraitsRef()
    {
        const string refPath = "#/components/messageTraits/t";
        var expected = new { Title = "9f7b58c5-63f5-4209-910a-b786c571eee4" };

        var yaml = $"""
        {YamlHeader}
        {MessageDefinition}
              traits:
               $ref: '{refPath}'
          messageTraits:
            t:
              {expected.ToYaml(indent: 6)}
        """;

        await MessageResolveReferernceTest(yaml, refPath, expected, o => o.Traits);
    }

    private Task MessageResolveReferernceTest<T>(string yaml, string refPath, object expected, Func<Message, T?> getRef)
        where T : IJsonReference
        => ResolveReferernceTest(yaml, refPath, expected, d =>
        {
            Assert.That(d.Components.Messages, Is.Not.Null.And.ContainKey(MessageName));
            var msg = d.Components.Messages[MessageName];
            Assert.That(msg, Is.Not.Null);
            return getRef(msg);
        });
}
