using Newtonsoft.Json;

namespace ApiCodeGenerator.AsyncApi.DOM.V2;

internal class ChannelItem : RefObject
{
    [JsonProperty("bindings")]
    public Reference<ChannelBindings>? Bindings { get; set; }

    [JsonProperty("description")]
    public string? Description { get; set; }

    [JsonProperty("parameters")]
    public IDictionary<string, Reference<Parameter>>? Parameters { get; set; }

    [JsonProperty("publish")]
    public Operation? Publish { get; set; }

    [JsonProperty("subscribe")]
    public Operation? Subscribe { get; set; }

    [JsonProperty("servers")]
    public string[]? Servers { get; set; }

    [JsonExtensionData]
    public IDictionary<string, object?>? ExtensionData { get; set; }
}
