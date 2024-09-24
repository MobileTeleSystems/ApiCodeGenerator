using ApiCodeGenerator.AsyncApi.CSharp.Models;
using ApiCodeGenerator.AsyncApi.DOM;
using NJsonSchema.CodeGeneration;
using NJsonSchema.CodeGeneration.CSharp;

namespace ApiCodeGenerator.AsyncApi.Tests.Infrastructure;

public sealed class FakeGenerator : CSharpGeneratorBase<FakeGeneratorSettings>
{
    public FakeGenerator(AsyncApiDocument document, FakeGeneratorSettings settings, CSharpTypeResolver resolver)
        : base(document, settings, resolver)
    {
        Document = document;
        Settings = settings;
    }

    public new AsyncApiDocument Document { get; }

    public new FakeGeneratorSettings Settings { get; }

    public IDictionary<string, string> DisabledWarnings { get; } = new Dictionary<string, string>();

    public string GenerateFileWithoutClasses()
    {
        return GenerateFile(Enumerable.Empty<CodeArtifact>(), Enumerable.Empty<CodeArtifact>(), ClientGeneratorOutputType.Implementation);
    }

    protected override CSharpClientTemplateModel CreateClientModel(string className, CSharpOperationModel[] operations)
        => new(className, Document, Settings, []);

    protected override void FillDisabledWarningsList(IDictionary<string, string> warnings)
    {
        base.FillDisabledWarningsList(warnings);
        foreach (var item in DisabledWarnings)
        {
            warnings.Add(item.Key, item.Value);
        }
    }
}
