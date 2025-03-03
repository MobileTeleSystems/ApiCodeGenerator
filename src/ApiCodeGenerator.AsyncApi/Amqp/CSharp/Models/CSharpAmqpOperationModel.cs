using ApiCodeGenerator.AsyncApi.DOM;
using ApiCodeGenerator.AsyncApi.DOM.Bindings.Amqp;
using ApiCodeGenerator.AsyncApi.Helpers;
using NJsonSchema.CodeGeneration.CSharp;

namespace ApiCodeGenerator.AsyncApi.CSharp.Models;

public class CSharpAmqpOperationModel : CSharpOperationModel
{
    public CSharpAmqpOperationModel(
        string name,
        Operation operation,
        CSharpGeneratorBaseSettings settings,
        CSharpTypeResolver typeResolver)
        : base(name, operation, settings, typeResolver)
    {
        var channelBinding = operation.Channel.ActualObject.Bindings?.ActualObject.Amqp;
        if (channelBinding != null)
        {
            var exchange = channelBinding.Exchange;
            if (exchange != null)
            {
                ExchangeName = exchange.Name;
                ExchangeType = exchange.Type.ToEnumMemberString();
                ExchangeDurable = exchange.Durable;
                ExchangeAutoDelete = exchange.AutoDelete;
            }

            var queue = channelBinding.Queue;
            if (queue != null)
            {
                QueueName = queue.Name;
                if (queue.AdditionalProperties.TryGetValue("x-prefetch-count", out var prefetchCount))
                {
                    PrefetchCount = Convert.ToInt32(prefetchCount);
                }

                if (queue.AdditionalProperties.TryGetValue("x-confirm", out var confirm))
                {
                    Confirm = Convert.ToBoolean(confirm);
                }
            }
        }

        var operationBinding = operation.Bindings?.ActualObject.Amqp;
        if (operationBinding != null)
        {
            Bcc = operationBinding.Bcc.ToArray();
            Cc = operationBinding.Cc.ToArray();
            DeliveryMode = (int)operationBinding.DeliveryMode;
            Expiration = operationBinding.Expiration;
            HasTimestamp = operationBinding.Timestamp;
            Mandatory = operationBinding.Mandatory.ToString().ToLower();
            Priority = operationBinding.Priority;

            if (operationBinding is OperationV0_2 operationBinding2
                && !string.IsNullOrEmpty(operationBinding2.ReplyTo))
            {
                ReplyTo = operationBinding2.ReplyTo!;
            }
        }
    }

    public string[] Bcc { get; } = [];

    public string[] Cc { get; } = [];

    public bool Confirm { get; } = false;

    public int DeliveryMode { get; } = 1;

    public string? ExchangeName { get; }

    public string? ExchangeType { get; }

    public bool ExchangeDurable { get; }

    public string ExchangeDurableStr => ExchangeDurable.ToString().ToLower();

    public bool ExchangeAutoDelete { get; }

    public string ExchangeAutoDeleteStr => ExchangeAutoDelete.ToString().ToLower();

    public int Expiration { get; set; } = 1000;

    public bool HasBcc => Bcc.Any();

    public bool HasCc => Cc.Any();

    public string Mandatory { get; } = false.ToString().ToLower();

    public bool HasTimestamp { get; }

    public int PrefetchCount { get; } = 0;

    public int Priority { get; }

    public string ReplyTo { get; } = string.Empty;

    public string QueueName { get; } = string.Empty;
}
