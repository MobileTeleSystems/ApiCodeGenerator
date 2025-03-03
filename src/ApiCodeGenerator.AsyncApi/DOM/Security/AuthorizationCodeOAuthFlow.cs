using Newtonsoft.Json;

namespace ApiCodeGenerator.AsyncApi.DOM.Security;

public class AuthorizationCodeOAuthFlow : OAuthFlow
{
    [JsonProperty("authorizationUrl", Required = Required.Always)]
    public required string AuthorizationUrl { get; set; }

    [JsonProperty("tokenUrl", Required = Required.Always)]
    public required string TokenUrl { get; set; }
}
