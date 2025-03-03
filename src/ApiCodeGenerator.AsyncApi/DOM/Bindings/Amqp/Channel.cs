using Newtonsoft.Json;

namespace ApiCodeGenerator.AsyncApi.DOM.Bindings.Amqp;

public class Channel
{
    private IDictionary<string, object>? _additionalProperties;

    /// <summary>
    /// Defines what type of channel is it. Can be either 'queue' or 'routingKey' (default).
    /// </summary>
    [JsonProperty("is", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
    [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
    public ChannelType Is { get; set; } = ChannelType.RoutingKey;

    /// <summary>
    /// When is=routingKey, this object defines the exchange properties.
    /// </summary>
    [JsonProperty("exchange", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
    public Exchange? Exchange { get; set; }

    /// <summary>
    /// When is=queue, this object defines the queue properties.
    /// </summary>
    [JsonProperty("queue", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
    public Queue? Queue { get; set; }

    /// <summary>
    /// The version of this binding. If omitted, 'latest' MUST be assumed.
    /// </summary>
    [JsonProperty("bindingVersion", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
    public string BindingVersion { get; set; } = default!;

    [JsonExtensionData]
    public IDictionary<string, object> AdditionalProperties
    {
        get => _additionalProperties ??= new Dictionary<string, object>();
        set => _additionalProperties = value;
    }
}
