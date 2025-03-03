namespace ApiCodeGenerator.AsyncApi.DOM.Serialization;

public interface IDocumentAware
{
    internal AsyncApiDocument? Document { get; set; }
}
