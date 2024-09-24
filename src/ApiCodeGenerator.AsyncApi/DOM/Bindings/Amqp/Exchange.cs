using Newtonsoft.Json;

namespace ApiCodeGenerator.AsyncApi.DOM.Bindings.Amqp;

public class Exchange
{
    private IDictionary<string, object>? _additionalProperties;

    /// <summary>
    /// The name of the exchange. It MUST NOT exceed 255 characters long.
    /// </summary>
    [JsonProperty("name", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
    public string Name { get; set; } = default!;

    /// <summary>
    /// The type of the exchange. Can be either 'topic', 'direct', 'fanout', 'default' or 'headers'.
    /// </summary>
    [JsonProperty("type", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
    [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
    public ExchangeType Type { get; set; } = default!;

    /// <summary>
    /// Whether the exchange should survive broker restarts or not.
    /// </summary>
    [JsonProperty("durable", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
    public bool Durable { get; set; } = default!;

    /// <summary>
    /// Whether the exchange should be deleted when the last queue is unbound from it.
    /// </summary>
    [JsonProperty("autoDelete", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
    public bool AutoDelete { get; set; } = default!;

    /// <summary>
    /// The virtual host of the exchange. Defaults to '/'.
    /// </summary>
    [JsonProperty("vhost", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
    public string Vhost { get; set; } = "/";

    [JsonExtensionData]
    public IDictionary<string, object> AdditionalProperties
    {
        get => _additionalProperties ??= new Dictionary<string, object>();
        set => _additionalProperties = value;
    }
}
