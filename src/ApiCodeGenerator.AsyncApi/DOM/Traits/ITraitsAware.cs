using Newtonsoft.Json;

namespace ApiCodeGenerator.AsyncApi.DOM.Traits;

public interface ITraitsAware<TEntity, TTraits>
    where TEntity : class, ITraitsAware<TEntity, TTraits>
    where TTraits : Traits<TTraits, TEntity>
{
    [JsonProperty("traits")]
    public ICollection<Reference<TTraits>>? Traits { get; set; }
}
