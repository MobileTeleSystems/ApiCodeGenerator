using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ApiCodeGenerator.AsyncApi.DOM
{
    public class Operation : RefObject<Operation>
    {
        [JsonProperty("bindings", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public OperationBindings? Bindings { get; set; }

        [JsonProperty("description")]
        public string? Description { get; set; }

        [JsonProperty("message")]
        public Message Message { get; set; } = default!;

        [JsonProperty("operationId")]
        public string? OperationId { get; set; }

        [JsonProperty("summary")]
        public string? Summary { get; set; }

        [JsonProperty("tags")]
        public ICollection<Tag>? Tags { get; set; }

        [JsonProperty("traits")]
        public JToken? Traits { get; set; }

        [JsonExtensionData]
        public IDictionary<string, object> ExtensionData { get; set; } = new Dictionary<string, object>();
    }
}
