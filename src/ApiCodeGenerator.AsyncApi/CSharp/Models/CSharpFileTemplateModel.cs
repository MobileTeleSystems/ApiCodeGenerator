using ApiCodeGenerator.AsyncApi.DOM;
using NJsonSchema.CodeGeneration;
using NJsonSchema.CodeGeneration.CSharp;

namespace ApiCodeGenerator.AsyncApi.CSharp.Models
{
    internal class CSharpFileTemplateModel
    {
        private readonly IEnumerable<CodeArtifact> _clientTypes;
        private readonly IEnumerable<CodeArtifact> _dtoTypes;
        private readonly ClientGeneratorOutputType _outputType;
        private readonly AsyncApiDocument _document;
        private readonly CSharpGeneratorBaseSettings _settings;
        private readonly AsyncApiGeneratorBase _cSharpGeneratorBase;
        private readonly CSharpTypeResolver _resolver;

        public CSharpFileTemplateModel(IEnumerable<CodeArtifact> clientTypes, IEnumerable<CodeArtifact> dtoTypes, ClientGeneratorOutputType outputType, AsyncApiDocument document, CSharpGeneratorBaseSettings settings, AsyncApiGeneratorBase cSharpGeneratorBase, CSharpTypeResolver resolver)
        {
            _clientTypes = clientTypes;
            _dtoTypes = dtoTypes;
            _outputType = outputType;
            _document = document;
            _settings = settings;
            _cSharpGeneratorBase = cSharpGeneratorBase;
            _resolver = resolver;

            var clients = _clientTypes.Select(a => a.Code);
            var dtos = _dtoTypes.Select(a => a.Code);

            Artifacts = clients
                .Concat(dtos)
                .ToArray();
        }

        public string[] Artifacts { get; }

        public IDictionary<string, string> DisabledWarnings { get; } = new Dictionary<string, string>();

        public bool GenerateNullableReferenceTypes
            => _settings.CSharpGeneratorSettings.GenerateNullableReferenceTypes;

        public string[] NamespaceUsages
            => (_outputType == ClientGeneratorOutputType.Contracts
                ? _settings.AdditionalContractNamespaceUsages?.Where(n => n != null).ToArray()
                : _settings.AdditionalNamespaceUsages?.Where(n => n != null).ToArray())
                    ?? [];

        public string Namespace => _settings.CSharpGeneratorSettings.Namespace;
    }
}
