namespace ApiCodeGenerator.AsyncApi.Tests.SerializationV3;

public class CorrelationIdTests : TestBase
{
    public const string CorrelationIdName = "cid";

    private const string CorrelationIdDefinition = $"""
        components:
          correlationIds:
            {CorrelationIdName}:
        """;

    [Test]
    public void RequiredProperties()
    {
        var yaml = $"""
            {YamlHeader}
            {CorrelationIdDefinition}
                  description: ''
            """;

        RequiredPropertiesTest(yaml);
    }

    [Test]
    public async Task ReadExtensions()
    {
        const string value = "66229402-0446-4d81-8609-54311653f4c4";

        var yaml = $$"""
            {{YamlHeader}}
            {{CorrelationIdDefinition}}
                  {0}: '{{value}}'
                  location: ''
            """;
        await ReadExtensionsTest(yaml, value, d => d.Components.CorrelationIds.GetValueOrNull(CorrelationIdName));
    }

    [Test]
    public async Task ReadProperties()
    {
        var expected = new
        {
            Location = "b643f3c8-ba0b-4be2-8f31-140375cabbc3",
            Description = "04fae75c-1c24-4daa-ba4c-6851e821a4f1",
        };

        var yaml = $"""
            {YamlHeader}
            {CorrelationIdDefinition}
                  {expected.ToYaml(indent: 6)}
            """;

        await ReadPropertiesTest(yaml, expected, d => d.Components.CorrelationIds.GetValueOrNull(CorrelationIdName));
    }
}
