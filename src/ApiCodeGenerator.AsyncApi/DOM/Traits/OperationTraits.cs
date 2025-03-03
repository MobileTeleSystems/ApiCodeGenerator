using Newtonsoft.Json;

namespace ApiCodeGenerator.AsyncApi.DOM.Traits;

public class OperationTraits : Traits<OperationTraits, Operation>
{
    [JsonProperty("security")]
    public ICollection<Reference<Security.SecurityScheme>>? Security { get; set; }

    [JsonProperty("bindings")]
    public Reference<OperationBindings>? Bindings { get; set; }

    internal override void ApplyTo(Operation target, bool overwrite)
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

        if (Security is not null && (target.Security is null || overwrite))
        {
            target.Security = Security;
        }

        if (Bindings is not null && (target.Bindings is null || overwrite))
        {
            target.Bindings = Bindings;
        }
    }
}
