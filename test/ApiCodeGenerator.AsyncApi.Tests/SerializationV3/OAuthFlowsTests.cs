using ApiCodeGenerator.AsyncApi.DOM;
using ApiCodeGenerator.AsyncApi.DOM.Security;

namespace ApiCodeGenerator.AsyncApi.Tests.SerializationV3;

public class OAuthFlowsTests : TestBase
{
    private const string FlowsDefintition = """
        components:
          securitySchemes:
            s:
              type: oauth2
              flows:
        """;

    [Test]
    public async Task ReadExtensions()
    {
        const string value = "57e5afe1-d77d-483e-a8cf-e0337f7e23b0";
        var yaml = $$"""
            {{YamlHeader}}
            {{FlowsDefintition}}
                    {0}: '{{value}}'
            """;
        await ReadExtensionsTest(yaml, value, GetOAuthFlows);
    }

    [Test]
    public async Task ReadProperties()
    {
#pragma warning disable SA1312 // Variable names should begin with lower-case letter
        Dictionary<string, string> AvailableScopes = new() { ["dbe82a7c-ef56-41fa-8aa7-042414c2f8fd"] = "bf230c6e-c872-4957-b1fc-8db5009cdee3" };
#pragma warning restore SA1312 // Variable names should begin with lower-case letter

        var expected = new
        {
            Implicit = new
            {
                AuthorizationUrl = "992741d0-0e5b-4e48-b8f9-f05b7732e431",
                RefreshUrl = "69e52c5e-8b38-4b3b-b720-a884d330f13e",
                AvailableScopes,
            },
            Password = new
            {
                TokenUrl = "eda8952e-42f4-4c6c-9131-72615a62a6f9",
                RefreshUrl = "d39266f4-59bb-46ee-b45f-f0b40520d057",
                AvailableScopes,
            },
            ClientCredentials = new
            {
                TokenUrl = "ea510232-8fad-4e27-9ad0-6840fdbe2d50",
                RefreshUrl = "845c5f38-4b54-45b6-9810-32377673c541",
                AvailableScopes,
            },
            AuthorizationCode = new
            {
                AuthorizationUrl = "061605f9-c061-4e9d-a4f4-e302f2fcea65",
                TokenUrl = "93e9eacc-f575-4ffc-9d73-8a1521384bac",
                RefreshUrl = "4d40c092-b3c8-47fe-9209-db3ba6eff7d5",
                AvailableScopes,
            },
        };

        var yaml = $"""
            {YamlHeader}
            {FlowsDefintition}
                    {expected.ToYaml(indent: 8)}
            """;

        await ReadPropertiesTest(yaml, expected, GetOAuthFlows);
    }

    [TestCaseSource(nameof(GetRequiredPropertiesCases))]
    public void RequiredProperties(object expected, string propName)
    {
        var yaml = $"""
            {YamlHeader}
            {FlowsDefintition}
                    {expected.ToYaml(indent: 8)}
            """;

        RequiredPropertiesTest(yaml, propName);
    }

    private static IEnumerable<TestCaseData> GetRequiredPropertiesCases()
    {
#pragma warning disable SA1312 // Variable names should begin with lower-case letter
        Dictionary<string, string> AvailableScopes = new() { ["dbe82a7c-ef56-41fa-8aa7-042414c2f8fd"] = "bf230c6e-c872-4957-b1fc-8db5009cdee3" };
#pragma warning restore SA1312 // Variable names should begin with lower-case letter

        yield return new TestCaseData(
            new
            {
                Implicit = new
                {
                    AuthorizationUrl = "992741d0-0e5b-4e48-b8f9-f05b7732e431",
                },
            },
            "availableScopes")
            .SetArgDisplayNames("type: implicit", "availableScopes");

        yield return new TestCaseData(
            new
            {
                Implicit = new
                {
                    AvailableScopes,
                },
            },
            "authorizationUrl")
            .SetArgDisplayNames("type: implicit", "authorizationUrl");

        yield return new TestCaseData(
            new
            {
                Password = new
                {
                    TokenUrl = "ea73d2a8-968e-4de8-a5c4-c50d1b27343f",
                },
            },
            "availableScopes")
            .SetArgDisplayNames("type: password", "availableScopes");

        yield return new TestCaseData(
            new
            {
                Password = new
                {
                    AvailableScopes,
                },
            },
            "tokenUrl")
            .SetArgDisplayNames("type: password", "tokenUrl");

        yield return new TestCaseData(
            new
            {
                ClientCredentials = new
                {
                    TokenUrl = "ea73d2a8-968e-4de8-a5c4-c50d1b27343f",
                },
            },
            "availableScopes")
            .SetArgDisplayNames("type: clientCredentials");

        yield return new TestCaseData(
            new
            {
                ClientCredentials = new
                {
                    AvailableScopes,
                },
            },
            "tokenUrl")
            .SetArgDisplayNames("type: clientCredentials", "tokenUrl");

        yield return new TestCaseData(
            new
            {
                AuthorizationCode = new
                {
                    TokenUrl = "69071823-16c5-476a-b9bb-1bcb4926ad38",
                    AuthorizationUrl = "cd147b08-42d3-480b-a9ea-9167e9dc8aef",
                },
            },
            "availableScopes")
            .SetArgDisplayNames("type: authorizationCode", "availableScopes");

        yield return new TestCaseData(
            new
            {
                AuthorizationCode = new
                {
                    AvailableScopes,
                    AuthorizationUrl = "88cfe0a6-8ed3-4d59-a405-76a708899cb1",
                },
            },
            "tokenUrl")
            .SetArgDisplayNames("type: authorizationCode", "tokenUrl");

        yield return new TestCaseData(
            new
            {
                AuthorizationCode = new
                {
                    TokenUrl = "d88c1bd8-ea04-40d6-9add-b1a345a4d6ae",
                    AvailableScopes,
                },
            },
            "authorizationUrl")
            .SetArgDisplayNames("type: authorizationCode", "authorizationUrl");
    }

    private static OAuthFlows GetOAuthFlows(AsyncApiDocument document)
    {
        var scheme = document.Components.SecuritySchemes.GetValueOrNull("s");
        Assert.That(scheme?.ActualObject, Is.Not.Null.And.TypeOf<OAuth2SecurityScheme>());
        var oa2scheme = (OAuth2SecurityScheme)scheme;
        return oa2scheme.Flows;
    }
}
