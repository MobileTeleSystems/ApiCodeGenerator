using ApiCodeGenerator.AsyncApi.DOM.Serialization;
using Newtonsoft.Json;

namespace ApiCodeGenerator.AsyncApi.DOM.Traits;

public abstract class Traits<TTraits, TTarget> : ExtensionRefObject, IDocumentAware
    where TTraits : Traits<TTraits, TTarget>
    where TTarget : class
{
    [JsonProperty("title")]
    public string? Title { get; set; }

    [JsonProperty("summary")]
    public string? Summary { get; set; }

    [JsonProperty("description")]
    public string? Description { get; set; }

    [JsonProperty("tags")]
    public ICollection<Reference<Tag>>? Tags { get; set; }

    [JsonProperty("externalDocs")]
    public Reference<ExternalDocumentation>? ExternalDocs { get; set; }

    AsyncApiDocument? IDocumentAware.Document { get; set; }

    internal abstract void ApplyTo(TTarget target, bool overwrite);
}
