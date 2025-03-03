using Newtonsoft.Json;
using NJsonSchema;

namespace ApiCodeGenerator.AsyncApi.DOM.Security;

public class OAuthFlows : JsonExtensionObject
{
    [JsonProperty("implicit")]
    public ImplicitOAuthFlow? Implicit { get; set; }

    [JsonProperty("password")]
    public PasswordOAuthFlow? Password { get; set; }

    [JsonProperty("clientCredentials")]
    public ClientCredentialsOAuthFlow? ClientCredentials { get; set; }

    [JsonProperty("authorizationCode")]
    public AuthorizationCodeOAuthFlow? AuthorizationCode { get; set; }
}
