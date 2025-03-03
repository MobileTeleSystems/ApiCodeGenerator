using ApiCodeGenerator.AsyncApi.DOM;
using NJsonSchema.References;

namespace ApiCodeGenerator.AsyncApi.Tests.SerializationV3;

public class ChannelTests : TestBase
{
    private const string ChannelId = "c";
    private const string ChannelDefinition = $"""
        components:
          channels:
            {ChannelId}:
        """;

    [Test]
    public async Task ReadProperties()
    {
        var expected = new
        {
            Address = "5f8fccfd-b175-43ec-a0c4-6cc2ab821282",
            Messages = new Dictionary<string, object> { ["m"] = new { Name = "98b3a608-5115-47fd-8cb4-84e063eacd50" } },
            Title = "315cbf60-5ebd-4275-b022-30c508145a00",
            Summary = "8274d7f9-41d0-4586-a53a-fa7ece4a5f74",
            Desciption = "d2914978-d650-41ba-ac03-d542e33670dd",
            Servers = new object[0], // only refs
            Parameters = new Dictionary<string, object>(),
            Tags = new[] { new { Name = "Tag" } },
            ExternalDocs = new { Url = "9c13a80d-f7c0-4682-95bb-ec1d2a55a169" },
            Bindings = new object(),
        };

        var yaml = $"""
        {YamlHeader}
        {ChannelDefinition}
              {expected.ToYaml(indent: 6)}
        """;

        await ReadPropertiesTest(yaml, expected, d => d.Components?.Channels?.GetValueOrNull(ChannelId));
    }

    [Test]
    public async Task RedExtensions()
    {
        const string value = "078cc5b9-34d9-4a7d-ae24-ef66b235750a";

        var yaml = $$"""
        {{YamlHeader}}
        {{ChannelDefinition}}
              {0}: '{{value}}'
        """;

        await ReadExtensionsTest(yaml, value, d => d.Components.Channels.GetValueOrNull(ChannelId));
    }

    [Test]
    public async Task MessagesRef()
    {
        const string refPath = "#/components/messages/m";
        const string msgName = "msg";
        var expected = new { Name = "dcea8ef8-fa0e-4594-8246-5947dc7fea67" };

        var yaml = $"""
        {YamlHeader}
        {ChannelDefinition}
              messages:
               {msgName}:
                $ref: '{refPath}'
          messages:
            m:
              {expected.ToYaml(indent: 6)}
        """;

        NamedReference<Message>? messageRef = null;
        await ChResolveReferenceTest(yaml, refPath, expected, c => messageRef = c.Messages?.GetValueOrNull(msgName));

        Assert.NotNull(messageRef);
        Assert.AreEqual(msgName, messageRef.ObjectId);
    }

    [Test]
    public async Task ServersRef()
    {
        const string refPath = "#/components/servers/s";
        var expected = new { Host = "host", Protocol = "amqp" };

        var yaml = $"""
        {YamlHeader}
        {ChannelDefinition}
              servers:
                - $ref: '{refPath}'
          servers:
            s:
              {expected.ToYaml(indent: 6)}
        """;

        await ChResolveReferenceTest(yaml, refPath, expected, c =>
        {
            Assert.That(c.Servers, Is.Not.Null.And.Count.EqualTo(1));
            return c.Servers.Single();
        });
    }

    [Test]
    public async Task ParametersRef()
    {
        const string refPath = "#/components/parameters/p";
        const string paramName = "param";
        var expected = new { Description = "dcea8ef8-fa0e-4594-8246-5947dc7fea67" };

        var yaml = $"""
        {YamlHeader}
        {ChannelDefinition}
              parameters:
                {paramName}:
                  $ref: '{refPath}'
          parameters:
            p:
              {expected.ToYaml(indent: 6)}
        """;

        NamedReference<Parameter>? messageRef = null;
        await ChResolveReferenceTest(yaml, refPath, expected, c => messageRef = c.Parameters?.GetValueOrNull(paramName));

        Assert.NotNull(messageRef);
        Assert.AreEqual(paramName, messageRef.ObjectId);
    }

    [Test]
    public async Task TagsRef()
    {
        const string refPath = "#/components/tags/t";
        var expected = new { Name = "ce5ac997-52ff-471a-b183-f880eb5cd219" };

        var yaml = $"""
        {YamlHeader}
        {ChannelDefinition}
              tags:
                - $ref: '{refPath}'
          tags:
            t:
              {expected.ToYaml(indent: 6)}
        """;

        await ChResolveReferenceTest(yaml, refPath, expected, c =>
        {
            Assert.That(c.Tags, Is.Not.Null.And.Count.EqualTo(1));
            return c.Tags.Single();
        });
    }

    [Test]
    public async Task ExternalDocsRef()
    {
        const string refPath = "#/components/externalDocs/d";
        var expected = new { Url = "http://tempuri.org/628248e3-25e9-487b-a8ca-4a5d05689ce1" };

        var yaml = $"""
        {YamlHeader}
        {ChannelDefinition}
              externalDocs:
                $ref: '{refPath}'
          externalDocs:
            d:
              {expected.ToYaml(indent: 6)}
        """;

        await ChResolveReferenceTest(yaml, refPath, expected, c => c.ExternalDocs);
    }

    [Test]
    public async Task BindingsRef()
    {
        const string refPath = "#/components/channelBindings/b";
        var expected = new { Amqp = new { Exchange = new { Name = "ex" } } };
        var yaml = $"""
        {YamlHeader}
        {ChannelDefinition}
              bindings:
                $ref: '{refPath}'
          channelBindings:
            b:
              {expected.ToYaml(indent: 6)}
        """;

        await ChResolveReferenceTest(yaml, refPath, expected, c => c.Bindings);
    }

    protected Task ChResolveReferenceTest<T>(string yaml, string refPath, object expected, Func<Channel, Reference<T>?> getRef)
        where T : IJsonReference
    {
        return ResolveReferernceTest(
            yaml,
            refPath,
            expected,
            d =>
            {
                var channel = d.Components.Channels.GetValueOrNull(ChannelId);
                Assert.That(channel, Is.Not.Null);
                return getRef(channel.ActualObject);
            });
    }
}
