#pragma warning disable SA1601 // Partial elements should be documented
namespace ApiCodeGenerator.OpenApi.Tests.Infrastructure
{
    internal static partial class TestHelpers
    {
        private static CSharpClientGenerator CreateGenerator(OpenApiDocument openApiDocument, CSharpClientGeneratorSettings settings)
            => new(openApiDocument, settings);

        private static string GetAdditionalUsings() => string.Empty;
    }
}
#pragma warning restore SA1601 // Partial elements should be documented
