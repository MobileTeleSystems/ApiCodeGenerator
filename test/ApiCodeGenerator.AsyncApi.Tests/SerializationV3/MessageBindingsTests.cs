using ApiCodeGenerator.AsyncApi.DOM.Bindings.Amqp;

namespace ApiCodeGenerator.AsyncApi.Tests.SerializationV3;

public class MessageBindingsTests : TestBase
{
    private const string MessageBindingName = "cb";
    private const string MessageBindingDefinition = $"""
        components:
          messageBindings:
            {MessageBindingName}:
        """;

    [Test]
    public async Task ReadProperties()
    {
        var expected = new
        {
            Amqp = new Message(),
        };

        var yaml = $"""
        {YamlHeader}
        {MessageBindingDefinition}
              {expected.ToYaml(indent: 6)}
        """;

        await ReadPropertiesTest(yaml, expected, d => d.Components.MessageBindings.GetValueOrNull(MessageBindingName));
    }

    [Test]
    public async Task ReadExtensions()
    {
        const string value = "a3442910-6806-4de9-93f8-b3ab5f22cfe6";
        var yaml =
            $$"""
            {{YamlHeader}}
            {{MessageBindingDefinition}}
                  {0}: '{{value}}'
            """;

        await ReadExtensionsTest(yaml, value, d => d.Components.MessageBindings.GetValueOrNull(MessageBindingName));
    }
}
