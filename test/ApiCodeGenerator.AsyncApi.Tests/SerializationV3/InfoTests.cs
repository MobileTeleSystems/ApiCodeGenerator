namespace ApiCodeGenerator.AsyncApi.Tests.SerializationV3;

public class InfoTests : TestBase
{
    [TestCase("""
        asyncapi: '3.0.0'
        info:
          title: test
        """,
        "version",
        TestName = $"{nameof(RequiredProperties)}(version)")]
    [TestCase("""
        asyncapi: '3.0.0'
        info:
          version: test
        """,
        "title",
        TestName = $"{nameof(RequiredProperties)}(title)")]
    public void RequiredProperties(string yaml, string propName)
        => RequiredPropertiesTest(yaml, propName);

    [Test]
    public async Task ReadProperties()
    {
        var expected = new
        {
            Title = "test",
            Version = "1.0",
            Description = "d5e4f0fb-b463-42a9-91ed-adf139ef62a7",
            TermsOfService = "1650e42e-d753-41d4-8811-3a6df9c576e4",
            Contact = new { },
            License = new { Name = "MIT" },
            Tags = new[] { new { Name = "TAG" } },
            ExternalDocs = new { Url = "url" },
        };

        var yaml = $"""
        {YamlHeader}
          {expected.ToYaml(indent: 2)}
        """;

        await ReadPropertiesTest(yaml, expected, d => d.Info);
    }

    [Test]
    public async Task ReadExtensions()
    {
        const string value = "d923ab53-46c1-4407-8334-e65301ae85f8";

        var yaml = $$"""
        {{YamlHeader}}
          {0}: '{{value}}'
        """;

        await ReadExtensionsTest(yaml, value, d => d.Info);
    }

    [Test]
    public async Task TagsRef()
    {
        const string refPath = "#/components/tags/tag";
        var expected = new { Name = "9a51d619-07aa-46fe-a800-3b75729ccc6f" };

        var yaml = $"""
        {YamlHeader}
          tags:
            - $ref: '{refPath}'
        components:
          tags:
            tag:
              {expected.ToYaml(indent: 6)}
        """;

        await ResolveReferernceTest(yaml, refPath, expected, d =>
        {
            Assert.That(d.Info.Tags, Is.Not.Null.And.Count.EqualTo(1));
            return d.Info.Tags.First();
        });
    }

    [Test]
    public async Task ExternalDocsRef()
    {
        const string refPath = "#/components/externalDocs/d";
        var expected = new { Url = "http://some.url" };

        var yaml = $$"""
        {{YamlHeader}}
          externalDocs:
            $ref: '{{refPath}}'
        components:
          externalDocs:
            d:
              {{expected.ToYaml(indent: 6)}}
        """;

        await ResolveReferernceTest(yaml, refPath, expected, d => d.Info.ExternalDocs);
    }
}
