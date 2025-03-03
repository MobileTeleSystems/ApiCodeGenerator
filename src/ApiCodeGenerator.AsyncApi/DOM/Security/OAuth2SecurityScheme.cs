using Newtonsoft.Json;

namespace ApiCodeGenerator.AsyncApi.DOM.Security;

public class OAuth2SecurityScheme : SecurityScheme
{
    internal const string SchemaType = "oauth2";

    internal OAuth2SecurityScheme()
        : base(SchemaType)
    {
    }

    [JsonProperty("flows", Required = Required.Always)]
    public required OAuthFlows Flows { get; set; }

    [JsonProperty("scopes")]
    public ICollection<string>? Scopes { get; set; }
}
