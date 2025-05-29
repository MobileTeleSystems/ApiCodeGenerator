namespace ApiCodeGenerator.AsyncApi.Tests.SerializationV3;

public class OperationReplyAddressTests : TestBase
{
    private const string OperationReplyAddressName = "oa";

    private const string OperationReplyAddressDefinition = $"""
        components:
          replyAddresses:
            {OperationReplyAddressName}:
        """;

    [Test]
    public void RequiredProperties()
    {
        const string yaml = $$"""
            {{YamlHeader}}
            {{OperationReplyAddressDefinition}} {}
            """;

        RequiredPropertiesTest(yaml, "location");
    }

    [Test]
    public async Task ReadProperties()
    {
        var expected = new
        {
            Description = "7de3980c-e8d4-40ba-801b-3de6702cbaf0",
            Location = "1cede9bc-81cf-4149-a5ca-212dfe36c50c",
        };

        var yaml = $"""
            {YamlHeader}
            {OperationReplyAddressDefinition}
                  {expected.ToYaml(indent: 6)}
            """;

        await ReadPropertiesTest(yaml, expected, d =>
        {
            Assert.That(d.Components.ReplyAddresses, Is.Not.Null);
            return d.Components.ReplyAddresses.GetValueOrNull(OperationReplyAddressName)?.ActualObject;
        });
    }

    [Test]
    public async Task ReadExtensions()
    {
        const string value = "18763153-b2db-4bab-acea-6b26311d561f";
        var yaml =
            $$"""
            {{YamlHeader}}
            {{OperationReplyAddressDefinition}}
                  location: loc
                  {0}: '{{value}}'
            """;

        await ReadExtensionsTest(yaml, value, d => d.Components.ReplyAddresses.GetValueOrNull(OperationReplyAddressName));
    }
}
