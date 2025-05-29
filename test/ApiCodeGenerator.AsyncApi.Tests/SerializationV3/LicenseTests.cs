namespace ApiCodeGenerator.AsyncApi.Tests.SerializationV3;

public class LicenseTests : TestBase
{
    [TestCase($$"""
        {{YamlHeader}}
          license: {}
        """,
        "name",
        TestName = $"{nameof(RequiredProperties)}(name)")]
    public void RequiredProperties(string yaml, string propName)
        => RequiredPropertiesTest(yaml, propName);

    [Test]
    public async Task ReadProperties()
    {
        var expected = new
        {
            Name = "ecbe7a6c-0823-4f57-ac8c-b088aed19684",
            Url = "7045c6cf-d4e7-42ee-b53d-a9d8f538c4bb",
        };

        var yaml = $"""
        {YamlHeader}
          license:
            {expected.ToYaml(indent: 4)}
        """;

        await ReadPropertiesTest(yaml, expected, d => d.Info.License);
    }

    [Test]
    public async Task ReadExtensions()
    {
        const string value = "b0bd3938-5582-41f4-8833-9e30a9f28984";

        var yaml = $$"""
        {{YamlHeader}}
          license:
            name: MIT
            {0}: '{{value}}'
        """;

        await ReadExtensionsTest(yaml, value, d => d.Info.License);
    }
}
