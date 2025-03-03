using Newtonsoft.Json;

namespace ApiCodeGenerator.AsyncApi.DOM;

public class OperationReply : ExtensionRefObject
{
    /// <summary>Definition of the address that implementations MUST use for the reply.</summary>
    [JsonProperty("address")]
    public Reference<OperationReplyAddress>? Address { get; set; }

    [JsonProperty("channel")]
    public Reference<Channel>? Channel { get; set; }

    [JsonProperty("messages")]
    public ICollection<Reference<Message>>? Messages { get; set; }
}
