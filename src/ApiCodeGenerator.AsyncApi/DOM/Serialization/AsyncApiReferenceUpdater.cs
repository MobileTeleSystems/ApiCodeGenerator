using Newtonsoft.Json.Serialization;
using NJsonSchema;
using NJsonSchema.References;
using NJsonSchema.Visitors;

namespace ApiCodeGenerator.AsyncApi.DOM.Serialization;

internal sealed class AsyncApiReferenceUpdater : AsyncJsonReferenceVisitorBase
{
    private readonly AsyncApiDocument _rootObject;
    private readonly JsonReferenceResolver _referenceResolver;
    private readonly IContractResolver _contractResolver;

    public AsyncApiReferenceUpdater(AsyncApiDocument rootObject, JsonReferenceResolver referenceResolver, IContractResolver contractResolver)
        : base(contractResolver)
    {
        _rootObject = rootObject;
        _referenceResolver = referenceResolver;
        _contractResolver = contractResolver;
    }

    protected override async Task VisitAsync(object obj, string path, string? typeNameHint, ISet<object> checkedObjects, Action<object> replacer, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (obj == null || checkedObjects.Contains(obj))
        {
            return;
        }

        if (obj is IDocumentAware documentAware)
        {
            documentAware.Document = _rootObject;
        }

        // customize resolve reference for Reference<>
        if (IsReference(obj.GetType()) && obj is IJsonReference jr)
        {
            checkedObjects.Add(obj);

            if (jr.Reference is not null && jr.ReferencePath is null)
            {
                // if inline object declaration without $ref then needed visit nested object
                await VisitAsync(jr.Reference,
                             path,
                             typeNameHint,
                             checkedObjects,
                             o => jr.Reference = (IJsonReference)o,
                             cancellationToken);
                return;
            }
            else if (jr.Reference is null && jr.ReferencePath is not null)
            {
                // if set reference path need resol reference and set Reference property
                var newReference = await VisitJsonReferenceAsync(jr, path, typeNameHint, cancellationToken).ConfigureAwait(false);
                if (newReference != jr)
                {
                    jr.Reference = newReference.ActualObject;
                    return;
                }
            }
        }

        await base.VisitAsync(obj, path, typeNameHint, checkedObjects, replacer, cancellationToken);
    }

    protected override async Task<IJsonReference> VisitJsonReferenceAsync(IJsonReference reference, string path, string? typeNameHint, CancellationToken cancellationToken)
    {
        if (reference.ReferencePath != null && reference.Reference == null)
        {
            var targetType = reference.GetType();
            var target = await _referenceResolver
                .ResolveReferenceAsync(_rootObject, reference.ReferencePath, targetType, _contractResolver, cancellationToken);
            return target;
        }

        return reference;
    }

    private static bool IsReference(Type type)
    {
        return type.IsGenericType
            && (
                type.GetGenericTypeDefinition() == typeof(Reference<>)
                || type.GetGenericTypeDefinition() == typeof(NamedReference<>));
    }
}
