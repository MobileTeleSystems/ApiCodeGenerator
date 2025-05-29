using ApiCodeGenerator.AsyncApi.DOM.Serialization;

namespace ApiCodeGenerator.AsyncApi.DOM.Traits;

public static class TraitsExtensions
{
    public static TEntity ApplyTraits<TEntity, TTraits>(this TEntity entity)
        where TEntity : class, ITraitsAware<TEntity, TTraits>, new()
        where TTraits : Traits<TTraits, TEntity>
    {
        var traits = entity.Traits;
        if (traits is not null)
        {
            var version = ((IDocumentAware)traits).Document?.AsyncApi ?? "3.0.0";
            var overwrite = version.StartsWith("2.");
            var target = new TEntity();
            foreach (var t in traits)
            {
                t.ActualObject.ApplyTo(target, overwrite);
            }

            return target;
        }
        else
        {
            return entity;
        }
    }
}
