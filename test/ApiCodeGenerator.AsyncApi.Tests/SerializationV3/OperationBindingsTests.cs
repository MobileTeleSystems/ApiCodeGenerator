using ApiCodeGenerator.AsyncApi.DOM.Bindings.Amqp;

namespace ApiCodeGenerator.AsyncApi.Tests.SerializationV3;

public class OperationBindingsTests : TestBase
{
    private const string OperationBindingName = "cb";
    private const string OperationBindingDefinition = $"""
        components:
          operationBindings:
            {OperationBindingName}:
        """;

    [Test]
    public async Task ReadProperties()
    {
        var expected = new
        {
            Amqp = new OperationV0_3(),
        };

        var yaml = $"""
        {YamlHeader}
        {OperationBindingDefinition}
              {expected.ToYaml(indent: 6)}
        """;

        await ReadPropertiesTest(yaml, expected, d => d.Components.OperationBindings.GetValueOrNull(OperationBindingName));
    }

    [Test]
    public async Task ReadExtensions()
    {
        const string value = "c4a6976e-a5af-4dd1-851c-bcbc73ee89b5";
        var yaml =
            $$"""
            {{YamlHeader}}
            {{OperationBindingDefinition}}
                  {0}: '{{value}}'
            """;

        await ReadExtensionsTest(yaml, value, d => d.Components.OperationBindings.GetValueOrNull(OperationBindingName));
    }
}
