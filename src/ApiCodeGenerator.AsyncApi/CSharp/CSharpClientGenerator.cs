using ApiCodeGenerator.AsyncApi.CSharp.Models;
using ApiCodeGenerator.AsyncApi.DOM;
using NJsonSchema.CodeGeneration.CSharp;

namespace ApiCodeGenerator.AsyncApi.CSharp;

public class CSharpClientGenerator : CSharpGeneratorBase<CSharpClientGeneratorSettings>
{
    public CSharpClientGenerator(AsyncApiDocument document, CSharpClientGeneratorSettings settings, CSharpTypeResolver resolver)
        : base(document, settings, resolver)
    {
    }

    protected new CSharpClientGeneratorSettings Settings => (CSharpClientGeneratorSettings)base.Settings;

    protected override CSharpClientTemplateModel CreateClientModel(string className, CSharpOperationModel[] operations)
    {
        return new CSharpClientTemplateModel(className, Document, Settings, operations);
    }
}
