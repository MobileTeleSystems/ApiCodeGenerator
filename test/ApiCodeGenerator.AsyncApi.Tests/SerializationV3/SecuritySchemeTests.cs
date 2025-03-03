using ApiCodeGenerator.AsyncApi.DOM.Security;
using NUnit.Framework.Internal;

namespace ApiCodeGenerator.AsyncApi.Tests.SerializationV3;

public class SecuritySchemeTests : TestBase
{
    private const string SchemeName = "sec";

    private const string SchemeDefinintion = $"""
        components:
          securitySchemes:
            {SchemeName}:
        """;

    [TestCaseSource(nameof(GetRequiredPropertiesCases))]
    public void RequiredProperties(string yaml)
      => RequiredPropertiesTest(yaml);

    [Test]
    public async Task ReadExtensions()
    {
        const string value = "dce52072-8342-4ad9-a311-b763b70cb645";
        var yaml = $$"""
        {{YamlHeader}}
        {{SchemeDefinintion}}
              {0}: '{{value}}'
              type: plain
        """;
        await ReadExtensionsTest(yaml, value, d => d.Components.SecuritySchemes.GetValueOrNull(SchemeName));
    }

    [TestCaseSource(nameof(GetReadPropertiesCases))]
    public async Task ReadProperties(object expected)
    {
        var yaml = $"""
            {YamlHeader}
            {SchemeDefinintion}
                  {expected.ToYaml(indent: 6)}
            """;
        await ReadPropertiesTest(yaml, expected, d => d.Components.SecuritySchemes.GetValueOrNull(SchemeName));
    }

    private static IEnumerable<TestCaseData> GetRequiredPropertiesCases()
    {
        yield return new TestCaseData($"""
            {YamlHeader}
            {SchemeDefinintion}
                  description: ''
            """)
            .SetArgDisplayNames("Type not set");

        yield return new TestCaseData($"""
            {YamlHeader}
            {SchemeDefinintion}
                  type: apiKey
            """)
            .SetArgDisplayNames("ApiKey without 'in'");

        yield return new TestCaseData($"""
            {YamlHeader}
            {SchemeDefinintion}
                  type: http
            """)
            .SetArgDisplayNames("Http without 'scheme'");

        yield return new TestCaseData($"""
            {YamlHeader}
            {SchemeDefinintion}
                  type: httpApiKey
                  name: '123'
            """)
            .SetArgDisplayNames("HttpApiKey without 'in'");

        yield return new TestCaseData($"""
            {YamlHeader}
            {SchemeDefinintion}
                  type: httpApiKey
                  in: cookie
            """)
            .SetArgDisplayNames("HttpApiKey without 'name'");

        yield return new TestCaseData($"""
            {YamlHeader}
            {SchemeDefinintion}
                  type: oauth2
            """)
            .SetArgDisplayNames("OAuth2 without 'flows'");

        yield return new TestCaseData($"""
            {YamlHeader}
            {SchemeDefinintion}
                  type: openIdConnect
            """)
            .SetArgDisplayNames("OpenIdConnect without 'openIdConnectUrl'");
    }

    private static IEnumerable<TestCaseData> GetReadPropertiesCases()
    {
        yield return new TestCaseData(
            new
            {
                Type = "apiKey",
                Description = "8dfc9713-c472-422f-80fb-ec0e59334ec7",
                In = ApiKeySecuritySchemaLocations.Password,
            })
        .SetArgDisplayNames("ApiKey");

        yield return new TestCaseData(
            new
            {
                Type = "httpApiKey",
                Description = "4acd16d5-5c6c-4c67-bf28-7b8f9244eeba",
                In = HttpApiKeySecuritySchemaLocations.Cookie,
                Name = "e9ae9169-6751-4b95-a90b-9abea3852c07",
            })
        .SetArgDisplayNames("HttpApiKey");

        yield return new TestCaseData(
            new
            {
                Type = "http",
                Description = "38e593e6-2baf-4f67-bafc-c4097f0a52aa",
                Scheme = "Bearer",
                BearerFormat = "7dbccdae-df1f-4b7d-80ac-0fc0c082e157",
            })
        .SetArgDisplayNames("Http");

        yield return new TestCaseData(
            new
            {
                Type = "openIdConnect",
                Description = "d21a4c72-bc8b-4253-8d7a-1e598a3e2183",
                OpenIdConnectUrl = "42d7b73b-d8e0-4476-a514-6845920c7cb9",
                Scopes = new[] { "s1", "s2" },
            })
        .SetArgDisplayNames("OpenIdConnect");

        yield return new TestCaseData(
            new
            {
                Type = "oauth2",
                Description = "9b47bfac-db22-44e2-857f-d2e175b34c5f",
                Flows = new object(),
                Scopes = new[] { "s1", "s2" },
            })
        .SetArgDisplayNames("OAuth2");

        foreach (var t in new[] { "userPassword", "X509", "symmetricEncryption", "asymmetricEncryption", "plain", "scramSha256", "scramSha512", "gssapi" })
        {
            yield return new TestCaseData(
                new
                {
                    Type = t,
                    Description = "8abd50e4-9995-4491-bea5-244149e84e30",
                })
            .SetArgDisplayNames(t);
        }
    }
}
