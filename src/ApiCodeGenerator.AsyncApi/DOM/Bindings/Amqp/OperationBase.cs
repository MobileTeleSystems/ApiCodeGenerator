using Newtonsoft.Json;

namespace ApiCodeGenerator.AsyncApi.DOM.Bindings.Amqp;

/// <summary>
/// This object contains information about the operation representation in AMQP.
/// </summary>
[JsonConverter(typeof(Serialization.InheritanceConverter<OperationBase>), nameof(BindingVersion), "latest")]
[Serialization.KnownType("0.2.0", typeof(OperationV0_2))]
[Serialization.KnownType("0.3.0", typeof(OperationV0_3))]
[Serialization.KnownType("latest", typeof(OperationV0_3))]
public abstract class OperationBase : RefObject<OperationBase>
{
    /// <summary>
    /// TTL (Time-To-Live) for the message. It MUST be greater than or equal to zero.
    /// </summary>
    [JsonProperty("expiration", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
    public int Expiration { get; set; } = default!;

    /// <summary>
    /// Identifies the user who has sent the message.
    /// </summary>
    [JsonProperty("userId", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
    public string UserId { get; set; } = default!;

    /// <summary>
    /// The routing keys the message should be routed to at the time of publishing.
    /// </summary>
    [JsonProperty("cc", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
    public ICollection<string> Cc { get; set; } = default!;

    /// <summary>
    /// A priority for the message.
    /// </summary>
    [JsonProperty("priority", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
    public int Priority { get; set; } = default!;

    /// <summary>
    /// Delivery mode of the message. Its value MUST be either 1 (transient) or 2 (persistent).
    /// </summary>
    [JsonProperty("deliveryMode", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
    public DeliviryMode DeliveryMode { get; set; } = default!;

    /// <summary>
    /// Whether the message is mandatory or not.
    /// </summary>
    [JsonProperty("mandatory", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
    public bool Mandatory { get; set; } = default!;

    /// <summary>
    /// Like cc but consumers will not receive this information.
    /// </summary>
    [JsonProperty("bcc", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
    public ICollection<string> Bcc { get; set; } = default!;

    /// <summary>
    /// Whether the message should include a timestamp or not.
    /// </summary>
    [JsonProperty("timestamp", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
    public bool Timestamp { get; set; } = default!;

    /// <summary>
    /// Whether the consumer should ack the message or not.
    /// </summary>
    [JsonProperty("ack", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
    public bool Ack { get; set; } = default!;

    /// <summary>
    /// The version of this binding. If omitted, "latest" MUST be assumed.
    /// </summary>
    [JsonProperty("bindingVersion", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
    public string BindingVersion { get; set; } = default!;
}
