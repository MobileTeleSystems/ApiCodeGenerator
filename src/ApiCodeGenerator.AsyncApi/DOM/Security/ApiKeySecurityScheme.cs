using Newtonsoft.Json;

namespace ApiCodeGenerator.AsyncApi.DOM.Security;

public class ApiKeySecurityScheme : SecurityScheme
{
    internal const string SchemaType = "apiKey";

    internal ApiKeySecurityScheme()
        : base(SchemaType)
    {
    }

    [JsonProperty("in", Required = Required.Always)]
    public ApiKeySecuritySchemaLocations In { get; set; }
}
