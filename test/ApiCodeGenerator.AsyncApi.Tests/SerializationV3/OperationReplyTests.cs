using ApiCodeGenerator.AsyncApi.DOM;
using NJsonSchema.References;

namespace ApiCodeGenerator.AsyncApi.Tests.SerializationV3;

public class OperationReplyTests : TestBase
{
    private const string OperationReplyName = "or";

    private const string OperationReplyDefinition = $"""
        components:
          replies:
            {OperationReplyName}:
        """;

    [Test]
    public async Task ReadProperties()
    {
        var expected = new
        {
            Address = new
            {
                ActualObject = new { Location = "bad10799-02b2-4f3c-b3a9-fb3315806e94" },
            },
        };

        var yaml = $"""
            {YamlHeader}
            {OperationReplyDefinition}
                  address:
                    {expected.Address.ActualObject.ToYaml(indent: 8)}
            """;

        await ReadPropertiesTest(yaml, expected, d => d.Components.Replies.GetValueOrNull(OperationReplyName));
    }

    [Test]
    public async Task ReadExtensions()
    {
        const string value = "18763153-b2db-4bab-acea-6b26311d561f";
        var yaml =
            $$"""
            {{YamlHeader}}
            {{OperationReplyDefinition}}
                  {0}: '{{value}}'
            """;

        await ReadExtensionsTest(yaml, value, d => d.Components.Replies.GetValueOrNull(OperationReplyName));
    }

    [Test]
    public async Task AddressRef()
    {
        const string refPath = "#/components/replyAddresses/ra";
        var expected = new
        {
            Location = "a244ba5d-70aa-4f91-ada0-f2e8b1a99e69",
        };
        var yaml = $"""
            {YamlHeader}
            {OperationReplyDefinition}
                  address:
                    $ref: '{refPath}'
              replyAddresses:
                ra:
                  {expected.ToYaml(indent: 6)}
            """;

        await OperationReplyResolveReferenceTest(yaml, refPath, expected, r => r.Address);
    }

    [Test]
    public async Task ChannelRef()
    {
        const string refPath = "#/components/channels/c";
        var expected = new
        {
            Address = "c51efc7d-aff5-44be-9336-918a26e3897f",
        };
        var yaml = $"""
            {YamlHeader}
            {OperationReplyDefinition}
                  channel:
                    $ref: '{refPath}'
              channels:
                c:
                  {expected.ToYaml(indent: 6)}
            """;

        await OperationReplyResolveReferenceTest(yaml, refPath, expected, r => r.Channel);
    }

    [Test]
    public async Task MessagesRef()
    {
        const string refPath = "#/components/messages/m";
        var expected = new
        {
            ContentType = "36e871ad-75b3-45ba-b1bf-56e74733cb20",
        };
        var yaml = $"""
            {YamlHeader}
            {OperationReplyDefinition}
                  messages:
                    - $ref: '{refPath}'
              messages:
                m:
                  {expected.ToYaml(indent: 6)}
            """;

        await OperationReplyResolveReferenceTest(yaml, refPath, expected, r =>
        {
            Assert.That(r.Messages, Is.Not.Null.And.Count.EqualTo(1));
            return r.Messages.Single();
        });
    }

    private Task OperationReplyResolveReferenceTest<T>(string yaml, string refPath, object expected, Func<OperationReply, Reference<T>?> getRef)
        where T : IJsonReference
    {
        return ResolveReferernceTest(
            yaml,
            refPath,
            expected,
            d =>
            {
                Assert.That(d.Components.Replies, Is.Not.Null.And.ContainKey(OperationReplyName));
                var reply = d.Components.Replies[OperationReplyName];
                Assert.That(reply, Is.Not.Null);
                return getRef(reply.ActualObject);
            });
    }
}
