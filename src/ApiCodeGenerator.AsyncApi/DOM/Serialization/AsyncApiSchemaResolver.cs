using NJsonSchema;
using NJsonSchema.Generation;

namespace ApiCodeGenerator.AsyncApi.DOM.Serialization;

internal class AsyncApiSchemaResolver : JsonSchemaResolver
{
    private readonly ITypeNameGenerator _typenameGenerator;

    public AsyncApiSchemaResolver(AsyncApiDocument rootObject, JsonSchemaGeneratorSettings settings)
        : base(rootObject, settings)
    {
        Document = rootObject;
        _typenameGenerator = settings.TypeNameGenerator;
    }

    public AsyncApiDocument Document { get; }

    public override void AppendSchema(JsonSchema schema, string? typeNameHint)
    {
        // append schemas loaded from external documents
        if (Document.Components.Schemas?.Values.Contains(schema) != true)
        {
            Document.Components.Schemas ??= new Dictionary<string, AsyncApiSchema>();
            var typeName = _typenameGenerator.Generate(schema, typeNameHint, Document.Components.Schemas.Keys);
            Document.Components.Schemas[typeName] = (AsyncApiSchema)schema;
        }
    }
}
