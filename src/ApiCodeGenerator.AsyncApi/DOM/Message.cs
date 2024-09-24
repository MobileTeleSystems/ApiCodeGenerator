using Newtonsoft.Json;
using NJsonSchema;

namespace ApiCodeGenerator.AsyncApi.DOM
{
    public class Message : RefObject<Message>
    {
        [JsonProperty("bindings")]
        public MessageBindings? Bindings { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; } = default!;

        [JsonProperty("payload")]
        public JsonSchema Payload { get; set; } = default!;
    }
}
