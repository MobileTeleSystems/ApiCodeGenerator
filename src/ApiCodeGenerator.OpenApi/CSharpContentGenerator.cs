using ApiCodeGenerator.OpenApi;
using NJsonSchema.CodeGeneration;
using NSwag;
using NSwag.CodeGeneration.CSharp;

public class CSharpContentGenerator<TContentGenerator, TGenerator, TSettings>
    : ContentGeneratorBase2<TContentGenerator, TGenerator, TSettings>
    where TContentGenerator : CSharpContentGenerator<TContentGenerator, TGenerator, TSettings>, new()
    where TGenerator : CSharpGeneratorBase
    where TSettings : CSharpGeneratorBaseSettings, new()
{
    public override string Generate()
        => Generator.GenerateFile();

    protected override TypeResolverBase CreateTypeResolver(TSettings settings, OpenApiDocument apiDocument)
        => CSharpGeneratorBase.CreateResolverWithExceptionSchema(settings.CSharpGeneratorSettings, apiDocument);
}
