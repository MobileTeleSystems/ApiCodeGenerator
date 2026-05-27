using System;
using System.IO;
using System.Threading.Tasks;
using NJsonSchema.CodeGeneration;
using NJsonSchema.CodeGeneration.TypeScript;
using NSwag;
using NSwag.CodeGeneration.TypeScript;

namespace ApiCodeGenerator.OpenApi
{
    internal sealed class TypeScriptClientContentGenerator
        : ContentGeneratorBase2<TypeScriptClientContentGenerator, TypeScriptClientGenerator, TypeScriptClientGeneratorSettings>
    {
        public override string Generate()
            => Generator.GenerateFile();

        protected override TypeResolverBase CreateTypeResolver(TypeScriptClientGeneratorSettings settings, OpenApiDocument apiDocument)
        {
            var resolver = new TypeScriptTypeResolver(settings.TypeScriptGeneratorSettings);
            resolver.RegisterSchemaDefinitions(apiDocument.Definitions);
            return resolver;
        }
    }
}
