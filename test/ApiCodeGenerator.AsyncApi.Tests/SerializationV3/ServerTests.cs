using ApiCodeGenerator.AsyncApi.DOM;
using NJsonSchema.References;

namespace ApiCodeGenerator.AsyncApi.Tests.SerializationV3;

public class ServerTests : TestBase
{
    private const string ServerName = "test";

    private const string ServerDefinition = """
        components:
          servers:
            test:
        """;

    private const string ServerDefinitionWithReq = $"""
        {ServerDefinition}
              protocol: 'http'
              host: 'host'
        """;

    [TestCase($"""
        {YamlHeader}
        components:
          servers:
              test:
                host: ''
        """,
        "protocol",
        TestName = $"{nameof(RequiredProperties)}(protocol)")]
    [TestCase($"""
        {YamlHeader}
        components:
          servers:
              test:
                protocol: ''
        """,
        "host",
        TestName = $"{nameof(RequiredProperties)}(host)")]
    public void RequiredProperties(string yaml, string propName)
        => RequiredPropertiesTest(yaml, propName);

    [Test]
    public async Task ReadProperties()
    {
        var expected = new
        {
            Host = "tempuri.org",
            Protocol = "https",
            ProtocolVersion = "10.0",
            Pathname = "242f423a-22c6-431e-8f9f-bcda007aa246",
            Description = "8c3f9bff-fbb6-45cb-849f-273268b3d36a",
            Title = "8d76741a-3edd-48ff-ac91-f7f13c226eb1",
            Summary = "64ab03dc-17a2-4447-8951-ba343f7458cf",
            Variables = new Dictionary<string, object>() { ["v"] = new { Default = "3cd855b0-170f-49a9-8c3a-4b957b1ec591" } },
            Security = new[] { new { Type = "plain" } },
            Tags = new object[0],
            ExternalDocs = new { Url = "7bb3a3d6-64d8-421a-852f-534ff54580e2" },
            Bindings = new object(),
        };

        var yaml = $"""
        {YamlHeader}
        {ServerDefinition}
              {expected.ToYaml(indent: 6)}
        """;

        await ReadPropertiesTest(yaml, expected, d => d.Components.Servers.GetValueOrNull(ServerName));
    }

    [Test]
    public async Task VariableRef()
    {
        const string NAME = "varRef";
        const string refPath = "#/components/serverVariables/v";
        var expected = new { Default = "1055e5fe-1737-4ba6-9320-2cfa22df54d2" };

        var yaml = $"""
        {YamlHeader}
        {ServerDefinitionWithReq}
              variables:
                {NAME}:
                  $ref: '#/components/serverVariables/v'
          serverVariables:
            v:
              {expected.ToYaml(indent: 6)}
        """;

        await ServerResolveReferenceTest(yaml, refPath, expected, s =>
        {
            Assert.That(s.Variables, Is.Not.Null.And.ContainKey(NAME));
            return s.Variables[NAME];
        });
    }

    [Test]
    public async Task SecurityRef()
    {
        const string refPath = "#/components/securitySchemes/plain";
        var expected = new { Type = "plain" };

        var yaml = $"""
        {YamlHeader}
        {ServerDefinitionWithReq}
              security:
                - $ref: '{refPath}'
          securitySchemes:
            plain:
              {expected.ToYaml(indent: 6)}
        """;

        await ServerResolveReferenceTest(yaml, refPath, expected, s =>
        {
            Assert.That(s.Security, Is.Not.Null.And.Count.EqualTo(1));
            return s.Security.Single();
        });
    }

    [Test]
    public async Task TagsRef()
    {
        const string refPath = "#/components/tags/tag";
        var expected = new { Name = "2afe15f5-0aab-4ad7-af7a-28734bb34cef" };

        var yaml = $"""
        {YamlHeader}
        {ServerDefinitionWithReq}
              tags:
                - $ref: '{refPath}'
          tags:
            tag:
              {expected.ToYaml(indent: 6)}
        """;

        await ServerResolveReferenceTest(yaml, refPath, expected, s =>
        {
            Assert.That(s.Tags, Is.Not.Null.And.Count.EqualTo(1));
            return s.Tags.Single();
        });
    }

    [Test]
    public async Task ExternalDocsRef()
    {
        const string refPath = "#/components/externalDocs/d";
        var expected = new { Url = "http://some.url" };

        var yaml = $"""
        {YamlHeader}
        {ServerDefinitionWithReq}
              externalDocs:
                $ref: '{refPath}'
          externalDocs:
            d:
              {expected.ToYaml(indent: 6)}
        """;

        await ServerResolveReferenceTest(yaml, refPath, expected, s => s.ExternalDocs);
    }

    [Test]
    public async Task BindingsRef()
    {
        const string refPath = "#/components/serverBindings/b";
        var expected = new { Amqp = new object() };

        var yaml = $"""
        {YamlHeader}
        {ServerDefinitionWithReq}
              bindings:
                $ref: '{refPath}'
          serverBindings:
            b:
              {expected.ToYaml(indent: 6)}
        """;

        await ServerResolveReferenceTest(yaml, refPath, expected, s => s.Bindings);
    }

    private Task ServerResolveReferenceTest<T>(string yaml, string refPath, object expected, Func<Server, Reference<T>?> getRef)
        where T : IJsonReference
      => ResolveReferernceTest(yaml, refPath, expected, d =>
      {
          Assert.That(d.Components.Servers, Is.Not.Null.And.ContainKey(ServerName));
          var server = d.Components.Servers.GetValueOrNull(ServerName);
          Assert.That(server, Is.Not.Null);
          return getRef(server.ActualObject);
      });
}
