namespace ApiCodeGenerator.AsyncApi.Tests.SerializationV3;

public class ParameterTests : TestBase
{
    private const string ParameterName = "p";

    private const string ParameterDefinition = $"""
        components:
          parameters:
            {ParameterName}:
        """;

    [Test]
    public async Task ReadProperties()
    {
        var expected = new
        {
            Enum = new[] { "bbcd2c7d-e07b-4b87-8a97-6d4691768c8a" },
            Default = "d1874489-f30d-42e5-9194-34a85dac4066",
            Description = "71b2d238-bb46-4efb-bf78-d748239a8b0e",
            Examples = new[] { "a24945c0-4f6c-448c-892f-f10d1b77cf64" },
            Location = "60270d0a-e624-4afe-a847-2624a87a6fb3",
        };

        var yaml = $"""
            {YamlHeader}
            {ParameterDefinition}
                  {expected.ToYaml(indent: 6)}
            """;

        await ReadPropertiesTest(yaml, expected, d => d.Components.Parameters.GetValueOrNull(ParameterName));
    }

    [Test]
    public async Task ReadExtensions()
    {
        const string value = "18763153-b2db-4bab-acea-6b26311d561f";
        var yaml =
            $$"""
            {{YamlHeader}}
            {{ParameterDefinition}}
                  {0}: '{{value}}'
            """;

        await ReadExtensionsTest(yaml, value, d => d.Components.Parameters.GetValueOrNull(ParameterName));
    }
}
