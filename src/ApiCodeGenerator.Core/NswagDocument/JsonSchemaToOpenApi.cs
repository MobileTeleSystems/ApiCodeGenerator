using Newtonsoft.Json;

namespace ApiCodeGenerator.Core.NswagDocument
{
    public class JsonSchemaToOpenApi
    {
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("schema")]
        public string Schema { get; set; } = string.Empty;
    }
}
