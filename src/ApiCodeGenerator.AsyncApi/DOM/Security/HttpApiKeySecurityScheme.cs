using Newtonsoft.Json;

namespace ApiCodeGenerator.AsyncApi.DOM.Security;

public class HttpApiKeySecurityScheme : SecurityScheme
{
    internal const string SchemaType = "httpApiKey";

    internal HttpApiKeySecurityScheme()
        : base(SchemaType)
    {
    }

    [JsonProperty("name", Required = Required.Always)]
    public required string Name { get; set; }

    [JsonProperty("in", Required = Required.Always)]
    public HttpApiKeySecuritySchemaLocations In { get; set; }
}
