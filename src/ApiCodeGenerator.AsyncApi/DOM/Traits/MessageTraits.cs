using Newtonsoft.Json;

namespace ApiCodeGenerator.AsyncApi.DOM.Traits;

public class MessageTraits : Traits<MessageTraits, Message>
{
    [JsonProperty("headers")]
    public Reference<AsyncApiSchema>? Headers { get; set; }

    [JsonProperty("correlationId")]
    public Reference<CorrelationId>? CorrelationId { get; set; }

    [JsonProperty("contentType")]
    public string? ContentType { get; set; }

    [JsonProperty("bindings")]
    public Reference<MessageBindings>? Bindings { get; set; }

    [JsonProperty("examples")]
    public ICollection<MessageExample>? Examples { get; set; }

    internal override void ApplyTo(Message target, bool overwrite)
    {
        if (Title is not null && (target.Title is null || overwrite))
        {
            target.Title = Title;
        }

        if (Summary is not null && (target.Summary is null || overwrite))
        {
            target.Summary = Summary;
        }

        if (Description is not null && (target.Description is null || overwrite))
        {
            target.Description = Description;
        }

        if (Tags is not null && (target.Tags is null || overwrite))
        {
            target.Tags = Tags;
        }

        if (ExternalDocs is not null && (target.ExternalDocs is null || overwrite))
        {
            target.ExternalDocs = ExternalDocs;
        }

        if (Headers is not null && (target.Headers is null || overwrite))
        {
            target.Headers = Headers;
        }

        if (CorrelationId is not null && (target.CorrelationId is null || overwrite))
        {
            target.CorrelationId = CorrelationId;
        }

        if (ContentType is not null && (target.ContentType is null || overwrite))
        {
            target.ContentType = ContentType;
        }

        if (Bindings is not null && (target.Bindings is null || overwrite))
        {
            target.Bindings = Bindings;
        }

        if (Examples is not null && (target.Examples is null || overwrite))
        {
            target.Examples = Examples;
        }
    }
}
