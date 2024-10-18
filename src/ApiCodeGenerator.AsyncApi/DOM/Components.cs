using Newtonsoft.Json;
using NJsonSchema;

namespace ApiCodeGenerator.AsyncApi.DOM
{
    public class Components
    {
        [JsonProperty("messages", DefaultValueHandling = DefaultValueHandling.Populate)]
        public IDictionary<string, Message> Messages { get; } = new Dictionary<string, Message>();

        [JsonProperty("parameters", DefaultValueHandling = DefaultValueHandling.Populate)]
        public IDictionary<string, Parameter> Parameters { get; } = new Dictionary<string, Parameter>();

        [JsonProperty("schemas", DefaultValueHandling = DefaultValueHandling.Populate)]
        public IDictionary<string, JsonSchema> Schemas { get; } = new Dictionary<string, JsonSchema>();

        [JsonProperty("servers", DefaultValueHandling = DefaultValueHandling.Populate)]
        public IDictionary<string, Server> Servers { get; } = new Dictionary<string, Server>();

        [JsonProperty("serverVariables", DefaultValueHandling = DefaultValueHandling.Populate)]
        public IDictionary<string, ServerVariable> ServerVariables { get; } = new Dictionary<string, ServerVariable>();
    }
}
