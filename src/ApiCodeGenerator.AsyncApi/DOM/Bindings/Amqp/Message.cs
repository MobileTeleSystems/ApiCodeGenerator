namespace ApiCodeGenerator.AsyncApi.DOM.Bindings.Amqp;

/// <summary>
/// This object contains information about the message representation in AMQP.
/// </summary>
[System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "14.0.2.0 (NJsonSchema v11.0.0.0 (Newtonsoft.Json v13.0.0.0))")]
public partial class Message : RefObject<Message>
{
    /// <summary>
    /// A MIME encoding for the message content.
    /// </summary>
    [Newtonsoft.Json.JsonProperty("contentEncoding", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
    public string ContentEncoding { get; set; } = default!;

    /// <summary>
    /// Application-specific message type.
    /// </summary>
    [Newtonsoft.Json.JsonProperty("messageType", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
    public string MessageType { get; set; } = default!;

    /// <summary>
    /// The version of this binding. If omitted, "latest" MUST be assumed.
    /// </summary>
    [Newtonsoft.Json.JsonProperty("bindingVersion", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
    public string BindingVersion { get; set; } = default!;

}
