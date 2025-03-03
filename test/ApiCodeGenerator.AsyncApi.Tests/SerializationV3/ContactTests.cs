namespace ApiCodeGenerator.AsyncApi.Tests.SerializationV3;

public class ContactTests : TestBase
{
    [Test]
    public async Task ReadProperties()
    {
        var expected = new
        {
            Name = "e4d497d5-7bc8-4e4b-bcfc-e8ae344b8ed7",
            Url = "c2152991-4910-4c8a-873a-dab78489cbb8",
            Email = "0a147392-90bf-4592-981a-28b0223c9750",
        };

        var yaml = $"""
        {YamlHeader}
          contact:
            {expected.ToYaml(indent: 4)}
        """;

        await ReadPropertiesTest(yaml, expected, d => d.Info?.Contact);
    }

    [Test]
    public async Task ReadExtensions()
    {
        const string value = "c6801f22-76b8-4385-bfb5-9619bc8c456c";

        var yaml = $$"""
        {{YamlHeader}}
          contact:
            {0}: '{{value}}'
        """;

        await ReadExtensionsTest(yaml, value, d => d.Info.Contact);
    }
}
