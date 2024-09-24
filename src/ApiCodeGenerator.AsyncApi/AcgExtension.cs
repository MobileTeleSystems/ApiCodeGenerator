using ApiCodeGenerator.Abstraction;
using ApiCodeGenerator.AsyncApi.CSharp;
using ApiCodeGenerator.AsyncApi.OperationNameGenerators;

namespace ApiCodeGenerator.AsyncApi
{
    public static class AcgExtension
    {
        public static Dictionary<string, ContentGeneratorFactory> CodeGenerators { get; } = new()
        {
            ["asyncApiToCSharpClient"] = CSharpClientContentGenerator.CreateAsync,
            ["asyncApiToCSharpAmqpService"] = Amqp.CSharp.CSharpAmqpContentGenerator.CreateAsync,
        };

        public static Dictionary<string, Type> OperationGenerators { get; } = new()
        {
            ["SingleClientFromOperationId"] = typeof(SingleClientFromOperationId),
            ["MultipleClientsFromFirstTagAndOperationId"] = typeof(MultipleClientsFromFirstTagAndOperationId),
        };
    }
}
