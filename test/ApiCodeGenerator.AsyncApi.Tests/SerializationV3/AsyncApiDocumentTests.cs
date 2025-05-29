using ApiCodeGenerator.AsyncApi.DOM;

namespace ApiCodeGenerator.AsyncApi.Tests.SerializationV3;

public class AsyncApiDocumentTests : TestBase
{
    [TestCase("asyncapi: '3.0.0'", "info")]
    [TestCase("info: { title: 'test', version: '1.0' }", "asyncapi")]
    public void RequiredProperties(string yaml, string propName)
        => RequiredPropertiesTest(yaml, propName);

    [Test]
    public async Task ReadExtensions()
    {
        const string propName = "x-test";
        const string value = "d9c4b3d4-dc0c-44d7-ac89-aebf9c09cc0b";

        var yaml = $$"""
        {{YamlHeader}}
        {{propName}}: '{{value}}'
        """;

        var document = await AsyncApiSerializer.FromYamlAsync(yaml);
        Assert.That(document.ExtensionData, Is.Not.Null
                                            .And.ContainKey(propName));
        Assert.That(document.ExtensionData[propName], Is.EqualTo(value));
    }

    [Test]
    public async Task ReadProperties()
    {
        var expected = new
        {
            AsyncApi = "3.0.0.",
            Id = "http://test.uri",
            Info = new { Title = "title", Version = "1.0" },
            Servers = new Dictionary<string, object> { ["s"] = new { Host = "host", Protocol = "http" } },
            DefaultContentType = "application/json",
            Channels = new Dictionary<string, object> { ["c"] = new { Description = "e7450db0-8194-48e6-89e2-0078e4e4cf90" } },
            Operations = new Dictionary<string, object> { ["o"] = new { Action = OperationAction.Receive, Channel = new Dictionary<string, string> { ["$ref"] = "#/channels/c" } } },
            Components = new { },
        };

        var yaml = $"""
            {YamlHeader}
            {expected.ToYaml(indent: 0)}
            """;
        await ReadPropertiesTest(yaml, expected, d => d);
    }

    [Test]
    public async Task ServersRef()
    {
        const string refPath = "#/components/servers/amqp";
        var expected = new { Host = "host", Protocol = "http" };
        var yaml = $"""
            {YamlHeader}
            servers:
              s:
                $ref: '{refPath}'
            components:
              servers:
                amqp:
                  {expected.ToYaml(indent: 6)}
            """;
        await ResolveReferernceTest(yaml, refPath, expected, d => d.Servers.GetValueOrNull("s"));
    }

    [Test]
    public async Task ChannelsRef()
    {
        const string refPath = "#/components/channels/ch1";
        const string chName = "c";
        var expected = new { Description = "356b0029-1630-4fdb-9306-76f0462dd177" };
        var yaml = $"""
            {YamlHeader}
            channels:
              {chName}:
                $ref: '{refPath}'
            components:
              channels:
                ch1:
                  {expected.ToYaml(indent: 6)}
            """;

        NamedReference<Channel>? channelRef = null;
        await ResolveReferernceTest(yaml, refPath, expected, d => channelRef = d.Channels.GetValueOrNull(chName));

        Assert.NotNull(channelRef);
        Assert.That(channelRef.ObjectId, Is.EqualTo(chName));
    }

    [Test]
    public async Task OperationsRef()
    {
        const string refPath = "#/components/operations/op1";
        const string opName = "op";
        var expected = new
        {
            Action = OperationAction.Receive,
            Description = "a6974e21-8326-4f36-a893-a05a00e91d2f",
            Channel = new Dictionary<string, string> { ["$ref"] = "#/components/channels/ch1" },
        };

        var yaml = $"""
            {YamlHeader}
            operations:
              {opName}:
                $ref: '{refPath}'
            components:
              channels:
                ch1:
                  address: queue
              operations:
                op1:
                  {expected.ToYaml(indent: 6)}
            """;

        NamedReference<Operation>? operationRef = null;
        await ResolveReferernceTest(yaml, refPath, expected, d => operationRef = d.Operations.GetValueOrNull(opName));

        Assert.NotNull(operationRef);
        Assert.That(operationRef.ObjectId, Is.EqualTo(opName));
    }
}
