using ApiCodeGenerator.AsyncApi.DOM;
using NJsonSchema.References;

namespace ApiCodeGenerator.AsyncApi.Tests.SerializationV3;

public class OpertationTests : TestBase
{
    private const string OperationName = "op1";
    private const string OperationDefinition = $"""
        components:
          channels:
           ch:
             title: ch
          operations:
            {OperationName}:
              channel:
                $ref: '#/components/channels/ch'
        """ + "\r\n";

    [TestCase(YamlHeader + """
        components:
          operations:
            op1:
              action: send
        """,
        "channel",
        TestName = $"{nameof(RequiredProperties)}(channel)")]
    [TestCase(YamlHeader +
        OperationDefinition,
        "action",
        TestName = $"{nameof(RequiredProperties)}(action)")]
    public void RequiredProperties(string yaml, string propName)
    {
        RequiredPropertiesTest(yaml, propName);
    }

    [Test]
    public async Task ReadExtensions()
    {
        const string value = "18763153-b2db-4bab-acea-6b26311d561f";
        var yaml =
            $$"""
            {{YamlHeader}}
            {{OperationDefinition}}
                  action: send
                  {0}: '{{value}}'
            """;

        await ReadExtensionsTest(yaml, value, d => d.Components.Operations.GetValueOrNull(OperationName));
    }

    [Test]
    public async Task ReadProperties()
    {
        var expected = new
        {
            Action = "Send",
            Title = "1aeb7ca8-5b11-4404-a7ff-766f129e2a4b",
            Summary = "656021a0-69f7-4322-8703-50bbd74a6ac4",
            Description = "12b13814-4919-4e20-8b4f-373c4e983ca3",
            SecurtityScheme = new { Type = "test" },
            Tags = new[] { new { Name = "tag" } },
            ExternalDocs = new { Url = "url" },
            Bindings = new { Amqp = new object() },
            Traits = new[] { new { Title = "abf76138-4a3b-43d6-b4b6-488a4e7a69e6" } },
            Messages = new object[0],
            Reply = new { Address = new { Location = "e6c1c3d4-691e-476d-ba69-65be200066f9" } },
        };

        var yaml = $"""
            {YamlHeader}
            {OperationDefinition}
                  {expected.ToYaml(indent: 6)}
            """;

        Reference<Operation>? operation = null;
        await ReadPropertiesTest(yaml, expected, d => operation = d.Components.Operations.GetValueOrNull(OperationName));
        Assert.That(operation!.ActualObject.Channel, Is.Not.Null);
        Assert.That(operation!.ActualObject.Channel.ActualObject.Title, Is.EqualTo("ch"));
    }

    [Test]
    public async Task SecurityRef()
    {
        const string refPath = "#/components/securitySchemes/plain";
        var expected = new { Type = "plain" };

        var yaml = $"""
        {YamlHeader}
        {OperationDefinition}
              action: send
              security:
                - $ref: '{refPath}'
          securitySchemes:
            plain:
              {expected.ToYaml(indent: 6)}
        """;

        await OperationResolveReferenceTest(yaml, refPath, expected, o =>
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
        {OperationDefinition}
              action: send
              tags:
                - $ref: '{refPath}'
          tags:
            tag:
              {expected.ToYaml(indent: 6)}
        """;

        await OperationResolveReferenceTest(yaml, refPath, expected, o =>
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
        {OperationDefinition}
              action: send
              externalDocs:
                $ref: '{refPath}'
          externalDocs:
            d:
              {expected.ToYaml(indent: 6)}
        """;

        await OperationResolveReferenceTest(yaml, refPath, expected, o => o.ExternalDocs);
    }

    [Test]
    public async Task BindingsRef()
    {
        const string refPath = "#/components/operationBindings/b";
        var expected = new { Amqp = new { Ack = true } };

        var yaml = $"""
        {YamlHeader}
        {OperationDefinition}
              action: send
              bindings:
                $ref: '{refPath}'
          operationBindings:
            b:
              {expected.ToYaml(indent: 6)}
        """;

        await OperationResolveReferenceTest(yaml, refPath, expected, o => o.Bindings);
    }

    [Test]
    public async Task TraitsRef()
    {
        const string refPath = "#/components/operationTraits/t";
        var expected = new { Title = "ea361810-a63f-434e-91f1-a343f23d97c4" };

        var yaml = $"""
        {YamlHeader}
        {OperationDefinition}
              action: send
              traits:
              - $ref: '{refPath}'
          operationTraits:
            t:
              {expected.ToYaml(indent: 6)}
        """;

        await OperationResolveReferenceTest(yaml, refPath, expected, o => o.Traits?.Single());
    }

    [Test]
    public async Task MessageRef()
    {
        const string refPath = "#/components/messages/m";
        var expected = new { Name = "61dbf59b-25fb-4004-a4a8-fef224e284c1" };

        var yaml = $"""
        {YamlHeader}
        {OperationDefinition}
              action: send
              messages:
               - $ref: '{refPath}'
          messages:
            m:
              {expected.ToYaml(indent: 6)}
        """;

        await OperationResolveReferenceTest(yaml, refPath, expected, o =>
        {
            Assert.That(o.Messages, Is.Not.Null.And.Count.EqualTo(1));
            return o.Messages.Single();
        });
    }

    [Test]
    public async Task ReplyRef()
    {
        const string refPath = "#/components/replies/r";
        var expected = new { Address = new { ActualObject = new { Location = "a504b027-8b98-40ad-991c-ad6dfd37839e" } } };

        var yaml = $"""
        {YamlHeader}
        {OperationDefinition}
              action: send
              reply:
                $ref: '{refPath}'
          replies:
            r:
              address:
                location: {expected.Address.ActualObject.Location}
        """;

        await OperationResolveReferenceTest(yaml, refPath, expected, s => s.Reply);
    }

    private Task OperationResolveReferenceTest<T>(string yaml, string refPath, object expected, Func<Operation, Reference<T>?> getRef)
        where T : IJsonReference
    {
        return ResolveReferernceTest(yaml, refPath, expected, d =>
        {
            Assert.That(d.Components.Operations, Is.Not.Null.And.ContainKey(OperationName));
            var op = d.Components.Operations[OperationName];
            Assert.That(op, Is.Not.Null);
            return getRef(op.ActualObject);
        });
    }
}
