using ApiCodeGenerator.AsyncApi.DOM.Bindings.Amqp;

namespace ApiCodeGenerator.AsyncApi.Tests.SerializationV3;

public class ServerBindingsTests : TestBase
{
    private const string ServerBindingName = "sb";
    private const string ServerBindingDefinition = $"""
        components:
          serverBindings:
            {ServerBindingName}:
        """;

    [Test]
    public async Task ReadProperties()
    {
        var expected = new
        {
            Amqp = new Server(),
        };

        var yaml = $"""
        {YamlHeader}
        {ServerBindingDefinition}
              {expected.ToYaml(indent: 6)}
        """;

        await ReadPropertiesTest(yaml, expected, d => d.Components.ServerBindings.GetValueOrNull(ServerBindingName));
    }

    [Test]
    public async Task ReadExtensions()
    {
        const string value = "1294d95f-6662-4c0b-a0d9-12eb319ac4c7";
        var yaml =
            $$"""
            {{YamlHeader}}
            {{ServerBindingDefinition}}
                  {0}: '{{value}}'
            """;

        await ReadExtensionsTest(yaml, value, d => d.Components.ServerBindings.GetValueOrNull(ServerBindingName));
    }
}
