using ApiCodeGenerator.AsyncApi.CSharp;
using ApiCodeGenerator.AsyncApi.CSharp.Models;
using ApiCodeGenerator.AsyncApi.DOM;
using NJsonSchema.CodeGeneration;
using NJsonSchema.CodeGeneration.CSharp;

namespace ApiCodeGenerator.AsyncApi.Amqp.CSharp;

public class CSharpAmqpServiceGenerator : CSharpGeneratorBase<CSharpAmqpServiceGeneratorSettings>
{
    public CSharpAmqpServiceGenerator(AsyncApiDocument document, CSharpAmqpServiceGeneratorSettings settings, CSharpTypeResolver resolver)
        : base(document, settings, resolver)
    {
        Usages.Add("System.Text");
        Usages.Add(Settings.CSharpGeneratorSettings.JsonLibrary == CSharpJsonLibrary.NewtonsoftJson
                ? "Newtonsoft.Json"
                : "System.Text.Json");
        Usages.Add("RabbitMQ.Client");
        Usages.Add("RabbitMQ.Client.Events");
    }

    protected override CSharpClientTemplateModel CreateClientModel(string clasName, CSharpOperationModel[] operations)
    {
        return new Models.CSharpAmqpClientModel(clasName, Document, Settings, operations);
    }

    protected override IEnumerable<CodeArtifact> GenerateClientTypes(string className, CSharpOperationModel[] operations)
    {
        var poolArtifact = GenerateChannelPool(className, operations);

        return base.GenerateClientTypes(className, operations)
            .Append(poolArtifact);
    }

    protected override CSharpOperationModel CreateOperationModel(string name, string channelPath, Channel channel, Operation operation)
        => new CSharpAmqpOperationModel(name, channelPath, channel, operation, Settings, Resolver);

    protected CodeArtifact GenerateChannelPool(string className, CSharpOperationModel[] operations)
    {
        var model = (Models.CSharpAmqpClientModel)CreateClientModel(className, operations);
        var template = Settings.CodeGeneratorSettings.TemplateFactory.CreateTemplate("CSharp", "Client.ChannelPool", model);

        return new CodeArtifact(
            model.ChannelPoolType,
            CodeArtifactType.Class,
            CodeArtifactLanguage.CSharp,
            CodeArtifactCategory.Client,
            template);
    }
}
