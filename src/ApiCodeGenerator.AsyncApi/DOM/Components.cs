using ApiCodeGenerator.AsyncApi.DOM.Traits;
using Newtonsoft.Json;
using NJsonSchema;

namespace ApiCodeGenerator.AsyncApi.DOM;

public class Components : JsonExtensionObject
{
    [JsonProperty("messages")]
    public IDictionary<string, Reference<Message>>? Messages { get; set; }

    [JsonProperty("parameters")]
    public IDictionary<string, Reference<Parameter>>? Parameters { get; set; }

    [JsonProperty("schemas")]
    public IDictionary<string, AsyncApiSchema>? Schemas { get; set; }

    [JsonProperty("servers")]
    public IDictionary<string, Reference<Server>>? Servers { get; set; }

    [JsonProperty("serverVariables")]
    public IDictionary<string, Reference<ServerVariable>>? ServerVariables { get; set; }

    [JsonProperty("channels")]
    public IDictionary<string, Reference<Channel>>? Channels { get; set; }

    [JsonProperty("operations")]
    public IDictionary<string, Reference<Operation>>? Operations { get; set; }

    [JsonProperty("securitySchemes")]
    public IDictionary<string, Reference<Security.SecurityScheme>>? SecuritySchemes { get; set; }

    [JsonProperty("correlationIds")]
    public IDictionary<string, Reference<CorrelationId>>? CorrelationIds { get; set; }

    [JsonProperty("replies")]
    public IDictionary<string, Reference<OperationReply>>? Replies { get; set; }

    [JsonProperty("replyAddresses")]
    public IDictionary<string, Reference<OperationReplyAddress>>? ReplyAddresses { get; set; }

    [JsonProperty("externalDocs")]
    public IDictionary<string, Reference<ExternalDocumentation>>? ExternalDocs { get; set; }

    [JsonProperty("tags")]
    public IDictionary<string, Reference<Tag>>? Tags { get; set; }

    [JsonProperty("operationTraits")]
    public IDictionary<string, Reference<OperationTraits>>? OperationTraits { get; set; }

    [JsonProperty("messageTraits")]
    public IDictionary<string, Reference<MessageTraits>>? MessageTraits { get; set; }

    [JsonProperty("serverBindings")]
    public IDictionary<string, Reference<ServerBindings>>? ServerBindings { get; set; }

    [JsonProperty("channelBindings")]
    public IDictionary<string, Reference<ChannelBindings>>? ChannelBindings { get; set; }

    [JsonProperty("operationBindings")]
    public IDictionary<string, Reference<OperationBindings>>? OperationBindings { get; set; }

    [JsonProperty("messageBindings")]
    public IDictionary<string, Reference<MessageBindings>>? MessageBindings { get; set; }
}
