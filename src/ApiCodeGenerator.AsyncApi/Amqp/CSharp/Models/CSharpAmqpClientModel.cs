using ApiCodeGenerator.AsyncApi.CSharp;
using ApiCodeGenerator.AsyncApi.CSharp.Models;
using ApiCodeGenerator.AsyncApi.DOM;

namespace ApiCodeGenerator.AsyncApi.Amqp.CSharp.Models;

public class CSharpAmqpClientModel : CSharpClientTemplateModel
{
    public CSharpAmqpClientModel(string className, AsyncApiDocument document, CSharpGeneratorBaseSettings settings, CSharpOperationModel[] operationModels)
        : base(className, document, settings, operationModels)
    {
    }

    public string ChannelPoolType => (Class ?? "Client") + "ChannelPool";
}
