namespace ApiCodeGenerator.AsyncApi.Tests.SerializationV3;

public class ExternalDocumentationTests : TestBase
{
    private const string ExternalDocName = "tg";
    private const string ExternalDocDefinition = $"""
        components:
          externalDocs:
            {ExternalDocName}:
        """;

    [Test]
    public void RequiredProperties()
    {
        var yaml = $"""
            {YamlHeader}
            {ExternalDocDefinition}
                  description: ''
            """;
        RequiredPropertiesTest(yaml);
    }

    [Test]
    public async Task ReadProperties()
    {
        var expected = new
        {
            Url = "bc3f36a6-6fd8-427b-b352-4ebcb6b15e29",
            Description = "d2def82f-4d14-46e1-bd76-82f7a21cb383",
        };

        var yaml = $"""
            {YamlHeader}
            {ExternalDocDefinition}
                  {expected.ToYaml(indent: 6)}
            """;

        await ReadPropertiesTest(yaml, expected, d => d.Components.ExternalDocs.GetValueOrNull(ExternalDocName));
    }

    [Test]
    public async Task ReadExtensions()
    {
        const string value = "8131db35-db20-4253-baa7-0ab127376a52";
        var yaml = $$"""
            {{YamlHeader}}
            {{ExternalDocDefinition}}
                  url: 't'
                  {0}: '{{value}}'
            """;
        await ReadExtensionsTest(yaml, value, d => d.Components.ExternalDocs.GetValueOrNull(ExternalDocName));
    }
}
