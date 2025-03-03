using ApiCodeGenerator.AsyncApi.DOM.Bindings.Amqp;

namespace ApiCodeGenerator.AsyncApi.Tests.SerializationV3;

public class ChannelBindingsTests : TestBase
{
    private const string ChannelBindingName = "cb";
    private const string ChannelBindingDefinition = $"""
        components:
          channelBindings:
            {ChannelBindingName}:
        """;

    [Test]
    public async Task ReadProperties()
    {
        var expected = new
        {
            Amqp = new { Is = ChannelType.RoutingKey },
        };

        var yaml = $"""
        {YamlHeader}
        {ChannelBindingDefinition}
              {expected.ToYaml(indent: 6)}
        """;

        await ReadPropertiesTest(yaml, expected, d => d.Components.ChannelBindings.GetValueOrNull(ChannelBindingName));
    }

    [Test]
    public async Task ReadExtensions()
    {
        const string value = "9925bcaf-c31d-4213-be34-aca6cc928b16";
        var yaml =
            $$"""
            {{YamlHeader}}
            {{ChannelBindingDefinition}}
                  {0}: '{{value}}'
            """;

        await ReadExtensionsTest(yaml, value, d => d.Components.ChannelBindings.GetValueOrNull(ChannelBindingName));
    }
}
