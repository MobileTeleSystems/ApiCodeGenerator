using Newtonsoft.Json;

namespace ApiCodeGenerator.AsyncApi.DOM.Security;

public class OpenIdConnectSecurityScheme : SecurityScheme
{
    internal const string SchemaType = "openIdConnect";

    internal OpenIdConnectSecurityScheme()
        : base(SchemaType)
    {
    }

    [JsonProperty("openIdConnectUrl", Required = Required.Always)]
    public required string OpenIdConnectUrl { get; set; }

    [JsonProperty("scopes")]
    public ICollection<string>? Scopes { get; set; }
}
