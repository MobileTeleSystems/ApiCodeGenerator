namespace ApiCodeGenerator.AsyncApi.DOM.Bindings.Amqp;

public class Queue
{
    private IDictionary<string, object>? _additionalProperties;

    /// <summary>
    /// The name of the queue. It MUST NOT exceed 255 characters long.
    /// </summary>
    [Newtonsoft.Json.JsonProperty("name", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
    public string Name { get; set; } = default!;

    /// <summary>
    /// Whether the queue should survive broker restarts or not.
    /// </summary>
    [Newtonsoft.Json.JsonProperty("durable", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
    public bool Durable { get; set; } = default!;

    /// <summary>
    /// Whether the queue should be used only by one connection or not.
    /// </summary>
    [Newtonsoft.Json.JsonProperty("exclusive", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
    public bool Exclusive { get; set; } = default!;

    /// <summary>
    /// Whether the queue should be deleted when the last consumer unsubscribes.
    /// </summary>
    [Newtonsoft.Json.JsonProperty("autoDelete", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
    public bool AutoDelete { get; set; } = default!;

    /// <summary>
    /// The virtual host of the queue. Defaults to '/'.
    /// </summary>
    [Newtonsoft.Json.JsonProperty("vhost", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
    public string Vhost { get; set; } = "/";

    [Newtonsoft.Json.JsonExtensionData]
    public IDictionary<string, object> AdditionalProperties
    {
        get => _additionalProperties ??= new Dictionary<string, object>();
        set => _additionalProperties = value;
    }
}
