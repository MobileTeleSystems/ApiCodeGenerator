using ApiCodeGenerator.AsyncApi.DOM.Security;
using ApiCodeGenerator.AsyncApi.DOM.Traits;
using Newtonsoft.Json;

namespace ApiCodeGenerator.AsyncApi.DOM;

public class Operation : ExtensionRefObject, ITraitsAware<Operation, OperationTraits>
{
    [JsonProperty("action", Required = Required.Always)]
    public required OperationAction Action { get; set; }

    [JsonProperty("channel", Required = Required.Always)]
    public required Reference<Channel> Channel { get; set; }

    [JsonProperty("title")]
    public string? Title { get; set; }

    [JsonProperty("summary")]
    public string? Summary { get; set; }

    [JsonProperty("description")]
    public string? Description { get; set; }

    [JsonProperty("security")]
    public ICollection<Reference<SecurityScheme>>? Security { get; set; }

    [JsonProperty("tags")]
    public ICollection<Reference<Tag>>? Tags { get; set; }

    [JsonProperty("externalDocs")]
    public Reference<ExternalDocumentation>? ExternalDocs { get; set; }

    [JsonProperty("bindings")]
    public Reference<OperationBindings>? Bindings { get; set; }

    [JsonProperty("traits")]
    public Reference<OperationTraits>? Traits { get; set; }

    [JsonProperty("messages")]
    public ICollection<Reference<Message>>? Messages { get; set; } = default!;

    [JsonProperty("reply")]
    public Reference<OperationReply>? Reply { get; set; }
}
