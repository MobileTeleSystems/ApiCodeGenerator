namespace ApiCodeGenerator.AsyncApi.DOM.Bindings.Amqp;

/// <summary>
/// This object contains information about the operation representation in AMQP.
/// </summary>
public class OperationV0_3 : OperationBase
{
    /// <summary>
    /// Name of the queue where the consumer should send the response.
    /// </summary>
    [Newtonsoft.Json.JsonProperty("replyTo", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
    public string? ReplayTo { get; set; }
}
