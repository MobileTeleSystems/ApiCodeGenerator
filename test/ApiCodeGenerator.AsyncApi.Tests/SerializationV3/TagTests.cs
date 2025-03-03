namespace ApiCodeGenerator.AsyncApi.Tests.SerializationV3;

public class TagTests : TestBase
{
    private const string TagName = "tg";
    private const string TagDefinition = $"""
        components:
          tags:
            {TagName}:
        """;

    [Test]
    public void RequiredProperties()
    {
        var yaml = $"""
            {YamlHeader}
            {TagDefinition}
                  description: ''
            """;
        RequiredPropertiesTest(yaml);
    }

    [Test]
    public async Task ReadProperties()
    {
        var expected = new
        {
            Name = "070e42d2-dcdb-4f9e-a437-6699abdea7f9",
            Description = "ef001bd0-f0aa-49d9-8e24-991c9904abd5",
            ExternalDocs = new { Url = "dbbd5cc5-acbf-4bff-a547-f34d97b776a4" },
        };

        var yaml = $"""
            {YamlHeader}
            {TagDefinition}
                  {expected.ToYaml(indent: 6)}
            """;

        await ReadPropertiesTest(yaml, expected, d => d.Components.Tags.GetValueOrNull(TagName));
    }

    [Test]
    public async Task ReadExtensions()
    {
        const string value = "528df6af-a938-488b-a177-e3fd0173a90e";
        var yaml = $$"""
            {{YamlHeader}}
            {{TagDefinition}}
                  name: 't'
                  {0}: '{{value}}'
            """;
        await ReadExtensionsTest(yaml, value, d => d.Components.Tags.GetValueOrNull(TagName));
    }

    [Test]
    public async Task ExternalDocsRef()
    {
        const string refPath = "#/components/externalDocs/ed";
        var expected = new { Url = "b537cbb2-c126-43f3-8a29-feb0652b4fc6" };
        var yaml = $"""
            {YamlHeader}
            {TagDefinition}
                  name: 'tag'
                  externalDocs:
                    $ref: '{refPath}'
              externalDocs:
                ed:
                  {expected.ToYaml(indent: 6)}
            """;

        await ResolveReferernceTest(yaml, refPath, expected, d =>
        {
            var tag = d.Components.Tags.GetValueOrNull(TagName);
            Assert.That(tag, Is.Not.Null);
            return tag.ActualObject.ExternalDocs;
        });
    }
}
