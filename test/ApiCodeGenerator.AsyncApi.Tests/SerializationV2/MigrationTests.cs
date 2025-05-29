using System.Threading.Tasks;
using ApiCodeGenerator.AsyncApi.DOM;
using ApiCodeGenerator.AsyncApi.DOM.Traits;
using DeepEqual;

namespace ApiCodeGenerator.AsyncApi.Tests.SerializationV2;

public class MigrationTests : SerializationV3.TestBase
{
    private const string YamlHeaderV2 = """
        asyncapi: '2.6.0'
        info:
          title: title
          version: '1.0'
        channels:
          t:
        """;

    // https://www.asyncapi.com/docs/migration/migrating-to-v3#moved-metadata
    [Test]
    public async Task MoveExternalDocsIntoInfo()
    {
        var expected = new
        {
            Url = "bc7f6fa0-3b78-43e5-b316-ab7042418373",
            Description = "94403503-b006-43f1-9630-ee2393e01295",
        };
        var yaml = $"""
            {YamlHeaderV2}
            externalDocs:
              {expected.ToYaml(indent: 2)}
            """;

        await ReadPropertiesTest(yaml, expected, d => d.Info.ExternalDocs);
    }

    // https://www.asyncapi.com/docs/migration/migrating-to-v3#moved-metadata
    [Test]
    public async Task MoveTagsIntoInfo()
    {
        var expected = new[]
        {
            new
            {
                ActualObject = new
                {
                    Name = "658ddd81-da20-42be-a1f8-c117e6ffd2cc",
                    Description = "e52bcd9d-2179-49e4-ad76-cfacc6f83310",
                },
            },
        };
        var yaml = $"""
            {YamlHeaderV2}
            tags:
              {expected.Select(i => i.ActualObject).ToYaml(indent: 2)}
            """;

        await ReadPropertiesTest(yaml, expected, d => d.Info.Tags);
    }

    // https://www.asyncapi.com/docs/migration/migrating-to-v3#server-url-splitting-up
    [TestCaseSource(nameof(GetServerUrlSplitingCases))]
    public async Task ServerUrlSpliting(string yaml, object expected, Func<AsyncApiDocument, Server> getter)
    {
        await ReadPropertiesTest(yaml, expected, getter);
    }

    [Test]
    public void ServerUrlSpliting_UrlRequired()
    {
        var yaml = $"""
            {YamlHeaderV2}
            servers:
              s:
                protocol: amqp
            """;
        RequiredPropertiesTest(yaml, "url");
    }

    [Test]
    public async Task ReadChannels()
    {
        const string ChannelId = "user/signedup";
        const string expectedChannelId = "userSignedup";
        var expected = new Channel
        {
            Address = ChannelId,
            Bindings = new ChannelBindings
            {
                Amqp = new() { Is = DOM.Bindings.Amqp.ChannelType.Queue },
            },
            Description = "302d93cb-42b0-4ba8-b8ca-bf94585934a0",
            Messages =
            {
                ["msg1"] = new NamedReference<Message> { ReferencePath = "#/components/messages/msg1" },
            },
            Parameters =
            {
                ["p1"] = new Parameter() { Default = "257b6392-b4a3-436c-908f-d20949b8d351" },
            },
            Servers = [new() { ReferencePath = "#/servers/dev" }],
            ExtensionData = null,
        };

        var yaml = $"""
            {YamlHeaderV2}
              {ChannelId}:
                bindings:
                  amqp:
                    is: {expected.Bindings.ActualObject.Amqp!.Is}
                description: '{expected.Description}'
                parameters:
                  {expected.Parameters.Single().Key}:
                    {expected.Parameters.Single().Value.ActualObject.ToYaml(indent: 8)}
                servers:
                  - dev
                publish:
                  message:
                    $ref: '{expected.Messages.Single().Value.ReferencePath}'
            components:
              messages:
                msg1:
                    payload:
                      type: object
                      properties:
                        displayName:
                          type: string
                          description: Name of the user
            servers:
              dev:
                url: amqp://host
                protocol: amqp
            """;
        var comparison = new ComparisonBuilder()
            .IgnoreUnmatchedProperties()
            .IgnoreProperty<Reference<Server>>(r => r.ActualObject) // ignore prop for expected.Servers
            .IgnoreProperty<NamedReference<Message>>(r => r.ActualObject) // ignore prop for expected.Messages
            .Create();

        await ReadPropertiesTest(
            yaml,
            expected,
            d =>
            {
                Assert.That(d.Channels, Is.Not.Null.And.ContainKey(expectedChannelId));
                var channel1 = d.Channels[expectedChannelId];
                Assert.NotNull(channel1);
                return channel1;
            },
            comparison);
    }

    [Test]
    public async Task ReadChannelsRef()
    {
        const string ChannelId = "user/signedup";
        const string componentChannelId = "userSignedup";
        const string refPath = $"#/components/channels/{componentChannelId}";
        var expected = new Channel
        {
            Address = ChannelId,
            Bindings = new ChannelBindings
            {
                Amqp = new() { Is = DOM.Bindings.Amqp.ChannelType.Queue },
            },
            Description = "302d93cb-42b0-4ba8-b8ca-bf94585934a0",
            Messages =
            {
                ["msg1"] = new NamedReference<Message> { ReferencePath = "#/components/messages/msg1" },
            },
            Parameters =
            {
                ["p1"] = new Parameter() { Default = "257b6392-b4a3-436c-908f-d20949b8d351" },
            },
            Servers = [new() { ReferencePath = "#/servers/dev" }],
            ExtensionData = null,
        };

        var yaml = $"""
            {YamlHeaderV2}
              {ChannelId}:
                $ref: '{refPath}'

            components:
              channels:
                {componentChannelId}:
                  bindings:
                    amqp:
                      is: {expected.Bindings.ActualObject.Amqp!.Is}
                  description: '{expected.Description}'
                  parameters:
                    {expected.Parameters.Single().Key}:
                      {expected.Parameters.Single().Value.ActualObject.ToYaml(indent: 8)}
                  servers:
                    - dev
                  publish:
                    message:
                      $ref: '{expected.Messages.Single().Value.ReferencePath}'
              messages:
                msg1:
                    payload:
                      type: object
                      properties:
                        displayName:
                          type: string
                          description: Name of the user
            servers:
              dev:
                url: amqp://host
                protocol: amqp
            """;
        var comparison = new ComparisonBuilder()
            .IgnoreUnmatchedProperties()
            .IgnoreProperty<Reference<Server>>(r => r.ActualObject) // ignore prop for expected.Servers
            .IgnoreProperty<NamedReference<Message>>(r => r.ActualObject) // ignore prop for expected.Messages
            .Create();

        await ReadPropertiesTest(
            yaml,
            expected,
            d =>
            {
                Assert.That(d.Components.Channels, Is.Not.Null.And.ContainKey(componentChannelId));
                var channel1 = d.Components.Channels[componentChannelId];
                Assert.NotNull(channel1);

                Assert.That(d.Channels, Is.Not.Null.And.ContainKey(componentChannelId));
                var channelRef = d.Channels[componentChannelId];
                Assert.That(channelRef.ReferencePath, Is.EqualTo(refPath));
                Assert.That(channelRef.ActualObject, Is.SameAs(channel1.ActualObject));
                return channel1;
            },
            comparison);
    }

    [TestCaseSource(nameof(GetReadOperationCases))]
    public Task ReadOperations(string yaml, object expectedOperation, Func<AsyncApiDocument, Reference<Operation>> getOperation)
    {
        var comparison = new ComparisonBuilder()
            .IgnoreUnmatchedProperties()
            .IgnoreProperty<Reference<Channel>>(r => r.ActualObject)
            .IgnoreProperty<Reference<Message>>(r => r.ActualObject)
            .Create();
        return ReadPropertiesTest(yaml, expectedOperation, getOperation, comparison);
    }

    [Test]
    public async Task ReadMessages()
    {
        const string MessageId = "11b8fbfa";
        var expected = new
        {
            // payloads and headers are checked separately
            CorrelationId = new { Location = "46e5b57a-4326-4f01-98e8-87bcf9d19804" },
            ContentType = "25d353d7-8e43-496f-a8a0-8295ed7de850",
            Name = "526231ea-da38-43e1-8bbb-6e33ce896a39",
            Title = "bab568de-0467-42c1-bf94-a4e48212c93e",
            Summary = "4d5eee97-88d3-402b-b24f-054e69d8ffa9",
            Description = "d9d51365-0ef3-464d-b60a-28e7ac4aa5cd",
            Tags = new[] { new { Name = "3b009642-5a68-41dd-a685-ed16c9c1d32f" } },
            ExternalDocs = new { Url = "a86e2dc6-ab10-4fbd-81d8-b08c898c0748" },
            Bindings = new { Amqp = new object() },
            Examples = new[] { new { Name = "9189f33b-3900-40d4-aae1-da322d5b32d8" } },
            Traits = new[] { new { Title = "29d601e6-d6cb-4532-a905-8cf4d3035b3e" } },
        };

        var yaml = $"""
            {YamlHeaderV2}
                publish:
                  message:
                    messageId: '{MessageId}'
                    headers:
                      type: object
                    payload:
                      type: object
                    schemaFormat: {AsyncApiSchema.OpenApi}
                    {expected.ToYaml(indent: 8)}
            """;

        await ReadPropertiesTest(yaml, expected, d =>
        {
            var channel = d.Channels.GetValueOrNull("t");
            Assert.NotNull(channel, "Channel 't' not found");
            var msg = channel.ActualObject.Messages.GetValueOrNull(MessageId)?.ActualObject;
            Assert.NotNull(msg, "Message '{0}' not found", MessageId);
            Assert.That(msg.Headers?.ActualObject, Is.Not.Null.And.Property("SchemaFormat").EqualTo(AsyncApiSchema.AsyncApi3));
            Assert.That(msg.Payload?.ActualObject, Is.Not.Null.And.Property("SchemaFormat").EqualTo(AsyncApiSchema.OpenApi));
            return msg;
        });
    }

    [Test]
    public async Task ReadMessagesRef()
    {
        const string MessageId = "aaeb8fd4";
        var expected = new
        {
            // payloads and headers are checked separately
            CorrelationId = new { Location = "744840e3-c593-41c9-a6e1-05e08fb6d059" },
            ContentType = "9092dabc-f628-4123-944c-c77525f99c47",
            Name = "08c29430-58b2-4e7c-b091-f62022088725",
            Title = "bd6e8c55-9791-426c-b51c-599217c7f849",
            Summary = "0aa73c1c-b1d7-493b-88eb-917f11b83730",
            Description = "c03de12d-1297-4e84-a481-ae2c39cae951",
            Tags = new[] { new { Name = "4e2a254c-1a47-4bd3-a75f-aab6c69d13f3" } },
            ExternalDocs = new { Url = "e9d9d011-30a4-4300-8172-e0d368462c19" },
            Bindings = new { Amqp = new object() },
            Examples = new[] { new { Name = "4da980d2-3060-4bfd-90fc-b8031f2fda80" } },
            Traits = new[] { new { Title = "b9f09244-1cd3-446e-ae06-80614d94368a" } },
        };

        var yaml = $"""
            {YamlHeaderV2}
            components:
              messages:
                {MessageId}:
                    messageId: '{MessageId}'
                    headers:
                      type: object
                    payload:
                      type: object
                    schemaFormat: {AsyncApiSchema.OpenApi}
                    {expected.ToYaml(indent: 8)}
            """;

        await ReadPropertiesTest(yaml, expected, d =>
        {
            var msg = d.Components.Messages.GetValueOrNull(MessageId)?.ActualObject;
            Assert.NotNull(msg, "Message '{0}' not found", MessageId);
            Assert.That(msg.Headers?.ActualObject, Is.Not.Null.And.Property("SchemaFormat").EqualTo(AsyncApiSchema.AsyncApi3));
            Assert.That(msg.Payload?.ActualObject, Is.Not.Null.And.Property("SchemaFormat").EqualTo(AsyncApiSchema.OpenApi));
            return msg;
        });
    }

    [Test]
    public async Task ReadServerSecurityRef()
    {
        const string refPath = "#/components/securitySchemes/oauth";
        var expected = new DOM.Security.OAuth2SecurityScheme
        {
            Flows = new()
            {
                Implicit = new()
                {
                    AuthorizationUrl = "http://example.com",
                    AvailableScopes = new Dictionary<string, string>(),
                },
            },
            Scopes = ["test"],
        };

        var yaml = $"""
            {YamlHeaderV2}
            servers:
              test:
                url: test.mykafkacluster.org:28092
                protocol: kafka-secure
                security:
                - oauth: ["test"]
            components:
              securitySchemes:
                oauth:
                  {expected.ToYaml(indent: 6)}
            """;

        await ResolveReferernceTest(yaml, refPath, expected, d =>
        {
            var server = d.Servers.GetValueOrNull("test")?.ActualObject;
            Assert.NotNull(server, "Server 'test' not found.");
            Assert.That(server.Security, Is.Not.Null.And.Count.EqualTo(1));
            return server.Security.Single();
        });
    }

    [Test]
    public async Task ReadOperationSecurityRef()
    {
        const string refPath = "#/components/securitySchemes/oauth";
        var expected = new DOM.Security.OAuth2SecurityScheme
        {
            Flows = new()
            {
                Implicit = new()
                {
                    AuthorizationUrl = "http://example.com",
                    AvailableScopes = new Dictionary<string, string>(),
                },
            },
            Scopes = ["test"],
        };

        var yaml = $"""
            {YamlHeaderV2}
                publish:
                  operationId: op1
                  security:
                  - oauth: ["test"]
            components:
              securitySchemes:
                oauth:
                  {expected.ToYaml(indent: 6)}
            """;

        await ResolveReferernceTest(yaml, refPath, expected, d =>
        {
            var operation = d.Operations.GetValueOrNull("op1")?.ActualObject;
            Assert.NotNull(operation, "Operation 'op1' not found.");
            Assert.That(operation.Security, Is.Not.Null.And.Count.EqualTo(1));
            return operation.Security.Single();
        });
    }

    private static IEnumerable<TestCaseData> GetServerUrlSplitingCases()
    {
        var expected = new
        {
            Host = "host",
            PathName = "/path",
            Protocol = "amqp",
        };

        var url = $"{expected.Protocol}://{expected.Host}{expected.PathName}";

        yield return
            new TestCaseData($"""
                {YamlHeaderV2}
                servers:
                  s:
                    url: '{url}'
                    protocol: amqp
                """,
                expected,
                (AsyncApiDocument d) => d.Servers.GetValueOrNull("s")?.ActualObject)
            .SetArgDisplayNames("servers");

        yield return
            new TestCaseData($"""
                {YamlHeaderV2}
                components:
                  servers:
                    s:
                      url: '{url}'
                      protocol: amqp
                """,
                expected,
                (AsyncApiDocument d) => d.Components.Servers.GetValueOrNull("s")?.ActualObject)
            .SetArgDisplayNames("components.servers");

        yield return
            new TestCaseData($"""
                {YamlHeaderV2}
                servers:
                  s:
                    $ref: '#/components/servers/s'
                components:
                  servers:
                    s:
                      url: '{url}'
                      protocol: amqp
                """,
                expected,
                (AsyncApiDocument d) => d.Servers.GetValueOrNull("s")?.ActualObject)
            .SetArgDisplayNames("servers.s.$ref");
    }

    private static IEnumerable<TestCaseData> GetReadOperationCases()
    {
        // channels.subscribe
        {
            var expected = CreateOperation(OperationAction.Send);

            yield return new TestCaseData(
                $"""
            {YamlHeaderV2}
                subscribe:
                  operationId: op1
                  bindings:
                    {expected.Bindings!.ActualObject.ToYaml(indent: 8)}
                  description: '{expected.Description}'
                  externalDocs:
                    url: '{expected.ExternalDocs!.ActualObject.Url}'
                  message:
                    messageId: msg1
                  summary: '{expected.Summary}'
                  tags:
                  - {expected.Tags!.Single().ActualObject.ToYaml(indent: 8)}
                  traits:
                  - {expected.Traits!.Single().ActualObject.ToYaml(indent: 8)}
            """,
                expected,
                (AsyncApiDocument d) => d.Operations.GetValueOrNull("op1"))
            .SetArgDisplayNames("channels.subscribe");
        }

        // channels.publish
        {
            var expected = CreateOperation(OperationAction.Receive);
            yield return new TestCaseData(
                $"""
            {YamlHeaderV2}
                publish:
                  operationId: op1
                  bindings:
                    {expected.Bindings!.ActualObject.ToYaml(indent: 8)}
                  description: '{expected.Description}'
                  externalDocs:
                    url: '{expected.ExternalDocs!.ActualObject.Url}'
                  message:
                    messageId: msg1
                  summary: '{expected.Summary}'
                  tags:
                  - {expected.Tags!.Single().ActualObject.ToYaml(indent: 8)}
                  traits:
                  - {expected.Traits!.Single().ActualObject.ToYaml(indent: 8)}
            """,
                expected,
                (AsyncApiDocument d) => d.Operations.GetValueOrNull("op1"))
            .SetArgDisplayNames("channels.publish");
        }

        //channelsRef.publish
        {
            var expected = CreateOperation(OperationAction.Receive, "#/components/");
            expected.Channel.ReferencePath = "#/components/channels/t";
            const string refPath = "#/components/operations/op1";

            yield return new TestCaseData(
                $"""
            {YamlHeaderV2}
                $ref: '#/components/channels/t'
            components:
              channels:
                t:
                  publish:
                    operationId: op1
                    bindings:
                      {expected.Bindings!.ActualObject.ToYaml(indent: 10)}
                    description: '{expected.Description}'
                    externalDocs:
                      url: '{expected.ExternalDocs!.ActualObject.Url}'
                    message:
                      messageId: msg1
                    summary: '{expected.Summary}'
                    tags:
                    - {expected.Tags!.Single().ActualObject.ToYaml(indent: 10)}
                    traits:
                    - {expected.Traits!.Single().ActualObject.ToYaml(indent: 10)}
            """,
                expected,
                (AsyncApiDocument d) =>
                {
                    var op = d.Components.Operations.GetValueOrNull("op1");
                    Assert.That(op, Is.Not.Null);
                    var opRef = d.Operations.GetValueOrNull("op1");
                    Assert.That(opRef, Is.Not.Null);
                    Assert.That(opRef.ReferencePath, Is.EqualTo(refPath));
                    Assert.That(opRef.ActualObject, Is.SameAs(op.ActualObject));

                    return op;
                })
            .SetArgDisplayNames("channelsRef.publish");
        }

        Operation CreateOperation(OperationAction action, string refRoot = "#/")
        {
            return new Operation
            {
                Action = action,
                Bindings = new OperationBindings
                {
                    Amqp = new DOM.Bindings.Amqp.OperationV0_3
                    {
                        Ack = true,
                    },
                },
                Channel = new Reference<Channel> { ReferencePath = refRoot + "channels/t" },
                Description = "40750c12-74b3-471d-83a1-c4c802362784",
                ExternalDocs = new ExternalDocumentation { Url = new Uri("http://example.com/") },
                Messages = [new Reference<Message> { ReferencePath = refRoot + "channels/t/messages/msg1" }],
                /* The Security property will be tested in other tests */
                Summary = "ee1c906e-0fc7-437e-8abb-4b9530bc51f8",
                Tags = [new Tag { Name = "2a372cb5-c2ed-4d05-95a1-b2d481d3d1f5" }],
                Traits = [new OperationTraits { Summary = "bb1719cc-6fc5-45a9-a1ff-08a292be1144" }],
            };
        }
    }
}
