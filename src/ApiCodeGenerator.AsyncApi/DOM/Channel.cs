using Newtonsoft.Json;

namespace ApiCodeGenerator.AsyncApi.DOM
{
    public class Channel
    {
        [JsonProperty("bindings", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public ChannelBindings? Bindings { get; set; }

        [JsonProperty("description", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string? Description { get; set; }

        [JsonProperty("parameters", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public IDictionary<string, Parameter> Parameters { get; set; } = new Dictionary<string, Parameter>();

        [JsonProperty("publish")]
        public Operation? Publish { get; set; }

        [JsonProperty("subscribe", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Operation? Subscribe { get; set; }

        [JsonExtensionData]
        public IDictionary<string, object> ExtensionData { get; set; } = new Dictionary<string, object>();
    }

    // #pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}
