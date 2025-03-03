using Newtonsoft.Json;

namespace ApiCodeGenerator.AsyncApi.DOM.Security;

public class ImplicitOAuthFlow : OAuthFlow
{
    [JsonProperty("authorizationUrl", Required = Required.Always)]
    public required string AuthorizationUrl { get; set; }
}
