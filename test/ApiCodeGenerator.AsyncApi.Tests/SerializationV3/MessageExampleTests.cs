namespace ApiCodeGenerator.AsyncApi.Tests.SerializationV3;

public class MessageExampleTests : TestBase
{
    [Test]
    public async Task ReadProperties()
    {
        var expected = new
        {
            Headers = new Dictionary<string, object>
            {
                ["h1"] = "7392eebf-f805-4c6e-847a-fa2faea18470",
            },
            Payload = new Dictionary<string, object>
            {
                ["m1"] = new { Title = "d256e84a-7f91-496e-a73b-9ad884dcf78b" },
            },
            Name = "57a4d75a-b4ec-401a-b804-d4cb9d605e60",
            Summary = "1bf70ff6-c6f9-478f-bfca-2c3dcba1c206",
        };

        var yaml = $"""
            {YamlHeader}
            components:
              messages:
                m1:
                  examples:
                  - {expected.ToYaml(indent: 8)}
            """;

        var document = await AsyncApiSerializer.FromYamlAsync(yaml);

        Assert.That(document, Is.Not.Null);
        var msg = document.Components.Messages.GetValueOrNull("m1");
        Assert.That(msg?.ActualObject, Is.Not.Null);
        Assert.That(msg.ActualObject.Examples, Is.Not.Null.And.Count.EqualTo(1));
        var expl = msg.ActualObject.Examples.Single();
        Assert.That(expl, Is.Not.Null);
        expl.ShouldDeepEqual(expected, IgnoreUnmatchedProperties);
    }

    [Test]
    public async Task ReadExtension()
    {
        const string value = "424f5f4e-9e95-4e59-bc2a-2fb074e75680";
        const string MessageName = "msg1";

        var yaml = $$"""
            {{YamlHeader}}
            components:
              messages:
                {{MessageName}}:
                  examples:
                    - {0}: '{{value}}'
            """;
        await ReadExtensionsTest(yaml, value, d =>
        {
            var msg = d.Components.Messages.GetValueOrNull(MessageName);
            Assert.That(msg, Is.Not.Null);
            Assert.That(msg.ActualObject.Examples, Is.Not.Null.And.Count.EqualTo(1));
            return msg.ActualObject.Examples.Single();
        });
    }
}
