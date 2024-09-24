using ApiCodeGenerator.AsyncApi.DOM;
using NJsonSchema.CodeGeneration;

namespace ApiCodeGenerator.AsyncApi
{
    public abstract class AsyncApiGeneratorBase
    {
        public AsyncApiGeneratorBase(AsyncApiDocument document, ClientGeneratorBaseSettings settings, TypeResolverBase resolver)
        {
            Document = document;
            BaseSettings = settings;
            Resolver = resolver;
        }

        protected ClientGeneratorBaseSettings BaseSettings { get; private set; }

        protected AsyncApiDocument Document { get; }

        protected TypeResolverBase Resolver { get; }

        public string Generate()
        {
            var clientTypes = GenerateAllClientTypes()
                .Where(ca => ca.Type != CodeArtifactType.Class || BaseSettings.GenerateClientClasses)
                .Where(ca => ca.Type != CodeArtifactType.Interface || BaseSettings.GenerateClientInterfaces)
                .ToArray();
            var dtoTypes = BaseSettings.GenerateDtoTypes
                ? GenerateDtoTypes().ToArray()
                : [];

            return GenerateFile(clientTypes, dtoTypes, ClientGeneratorOutputType.Full);
        }

        protected abstract IEnumerable<CodeArtifact> GenerateAllClientTypes();

        protected abstract IEnumerable<CodeArtifact> GenerateDtoTypes();

        protected abstract string GenerateFile(IEnumerable<CodeArtifact> clientTypes, IEnumerable<CodeArtifact> dtoTypes, ClientGeneratorOutputType outputType);
    }
}
