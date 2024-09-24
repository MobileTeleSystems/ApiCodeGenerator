using ApiCodeGenerator.AsyncApi.DOM;
using NJsonSchema;
using NJsonSchema.Generation;

namespace ApiCodeGenerator.AsyncApi;

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
        if (Document.Components?.Schemas?.Values.Contains(schema) != true)
        {
            var typeName = _typenameGenerator.Generate(schema, typeNameHint, Document.Components!.Schemas.Keys);
            Document.Components!.Schemas[typeName] = schema;
        }
    }
}
