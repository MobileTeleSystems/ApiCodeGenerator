using System.Collections;
using ApiCodeGenerator.AsyncApi.DOM;
using ApiCodeGenerator.AsyncApi.DOM.Bindings.Amqp;
using YamlDotNet.Core.Tokens;

namespace ApiCodeGenerator.AsyncApi.Tests.Infrastructure;

internal static partial class TestHelpers
{
    public static string GetExpectedAmqpServiceCode(string className, int identCnt)
        => GetExpectedAmqpServiceCode(className, identCnt, [
                GetExpectedSummary("Inform about environmental lighting conditions of a particular streetlight.", identCnt + 4) +
                GetExpectedAmqpPublisherCode("ReceiveLightMeasurement", "LightMeasuredPayload", identCnt + 4)
           ]);

    public static string GetExpectedAmqpServiceCode(string className, int identCnt, params string[] operationsCode)
    {
        var ident = new string(' ', identCnt);
        return
        ident + "/// <summary>\n" +
        ident + "/// The Smartylighting Streetlights API allows you to remotely manage the city lights.\n" +
        ident + "/// <br/>\n" +
        ident + "/// <br/>### Check out its awesome features:\n" +
        ident + "/// <br/>\n" +
        ident + "/// <br/>* Turn a specific streetlight on/off 🌃\n" +
        ident + "/// <br/>* Dim a specific streetlight 😎\n" +
        ident + "/// <br/>* Receive real-time information about environmental lighting conditions 📈\n" +
        ident + "/// <br/>\n" +
        ident + "/// </summary>\n" +
        ident + GENERATED_CODE_ATTRIBUTE + '\n' +
        ident + $"public partial class {className}\n" +
        ident + "{\n" +
        ident + $"    private readonly {className}ChannelPool _channelPool;\n" +
        "\n" +
        ident + $"    public {className}(IConnection connection)\n" +
        ident + $"      : this({className}ChannelPool.Create(connection))\n" +
        ident + "    {\n" +
        ident + "    }\n" +
        "\n" +
        ident + $"    public {className}({className}ChannelPool pool)\n" +
        ident + "    {\n" +
        ident + "        _channelPool = pool;\n" +
        ident + "    }\n" +

        string.Join("\n", operationsCode) + "\n" +

        ident + "}\n";
    }

    public static string GetExpectedAmqpPublisherCode(
        string name,
        string payloadType,
        int identCnt,

        Exchange? exchange = null,
        OperationBase? operationBinding = null)
    {
        string[] body = [
           $"var exchange = \"{exchange?.Name}\";",
            "var routingKey = \"smartylighting.streetlights.1.0.event.{streetlightId}.lighting.measured\";",
            string.Empty,
            "var channel = _channelPool.GetChannel(\"receiveLightMeasurement\");",
            "var exchangeProps = new Dictionary<string, object>",
            "{",
            .. GetExchangeProps(operationBinding),
            "};",
            string.Empty,
            "channel.ExchangeDeclare(",
            "    exchange: exchange, // exchange.name from channel binding",
           $"    type: \"{exchange?.Type.ToString().ToLower()}\", // type from channel binding",
           $"    {(exchange?.Durable ?? false).ToString().ToLower()}, // durable from channel binding",
           $"    {(exchange?.AutoDelete ?? false).ToString().ToLower()}, // autoDelete from channel binding",
            "    exchangeProps);",
            string.Empty,
            "var props = channel.CreateBasicProperties();",
            string.Empty,
            "props.CorrelationId = \"receiveLightMeasurement_smartylighting.streetlights.1.0.event.{streetlightId}.lighting.measured\";",
           $"props.DeliveryMode = {((int?)operationBinding?.DeliveryMode) ?? 1};",
           $"props.Priority = {operationBinding?.Priority ?? 0};",
           $"props.Expiration = \"{operationBinding?.Expiration ?? 1000}\";",
            string.Empty,
            string.Empty,
            "    var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload));",
            string.Empty,
            string.Empty,
            "channel.BasicPublish(exchange: exchange,",
            "    routingKey: routingKey,",
           $"    mandatory: {(operationBinding?.Mandatory ?? false).ToString().ToLower()},",
            "    basicProperties: props,",
            "    body: body);",
            string.Empty,
            "return Task.CompletedTask;",
        ];
        return GetExpectedPublisherCode(name, payloadType, identCnt, body);

        static IEnumerable<string> GetExchangeProps(OperationBase? operationBinding)
        {
            if (operationBinding is null)
            {
                yield break;
            }

            (string Key, ICollection<string> Val)[] items = [
                ("CC", operationBinding.Cc),
                ("BCC", operationBinding.Bcc),
            ];
            foreach (var item in items)
            {
                if (item.Val.Any())
                {
                    yield return $"    [\"{item.Key}\"] = new List<object>{{";
                    foreach (var val in item.Val)
                    {
                        yield return $"        \"{val}\",";
                    }

                    yield return "    },";
                }
            }
        }
    }

    public static string GetExpectedAmqpSubscriberCode(
        string name,
        string payloadType,
        int identCnt,
        DOM.Bindings.Amqp.Channel? channelBinding = null)
    {
        string[] body = [
           $"var queue = \"{channelBinding?.Queue.Name}\"; // queue from specification",
            "var channel = _channelPool.GetChannel(\"dimLight\");",
           $"var exchange = \"{channelBinding?.Exchange.Name}\";",
            "var routingKey = \"smartylighting.streetlights.1.0.action.{streetlightId}.dim\";",
            string.Empty,
            "// TODO: declare passive?",
            "channel.QueueDeclare(queue);",
            string.Empty,
            "// IMPORTANT! If the routing key contains {some-parameter-name}",
            "// you must change the routing key below to something meaningful for amqp service listening for messages.",
            "// For demo purposes you can just replace it with the wildcard '#' which means it recieves",
            "// all messages no matter what the parameter is.",
            "channel.QueueBind(queue: queue,",
            "            exchange: exchange,",
            "            routingKey: routingKey);",
            string.Empty,
            "var consumer = new EventingBasicConsumer(channel);",
            "consumer.Received += (_, ea) =>",
            "{",
            "    var body = ea.Body.ToArray();",
           $"    var message = JsonConvert.DeserializeObject<{payloadType}>(Encoding.UTF8.GetString(body));",
            string.Empty,
            "    try",
            "    {",
            "        callback.Invoke(message);",
            string.Empty,
            "        channel.BasicAck(ea.DeliveryTag, true);",
            "    }",
            "    catch (Exception e)",
            "    {",
            "        channel.BasicReject(ea.DeliveryTag, false);",
            "    }",
            "};",
            string.Empty,
            "channel.BasicConsume(queue: queue,",
            "    autoAck: false,",
            "    consumer: consumer);",
        ];

        return GetExpectedSubscriberCode(name, payloadType, identCnt, body);
    }

    public static string GetExpectedPoolCode(string className, int identCnt)
        => GetExpectedPoolCode(className, identCnt, new DOM.Channel() { Publish = new() { OperationId = "receiveLightMeasurement" } });

    public static string GetExpectedPoolCode(string className, int identCnt, params DOM.Channel[] channels)
    {
        var ident = new string(' ', identCnt);
        return
        ident + "/// <summary>\n" +
        ident + "/// A channel pool for all channels defined in the async api specification\n" +
        ident + "/// </summary>\n" +
        ident + GENERATED_CODE_ATTRIBUTE + '\n' +
        ident + $"public class {className}ChannelPool //: IChannelPool\n" +
        ident + "{\n" +
        ident + "    private class Channel : IDisposable\n" +
        ident + "    {\n" +
        ident + "        /// <summary>\n" +
        ident + "        /// The confirm mode for the channel\n" +
        ident + "        /// </summary>\n" +
        ident + "        public bool Confirm { get; init; }\n" +
        "\n" +
        ident + "        /// <summary>\n" +
        ident + "        /// The prefetch count for the channel\n" +
        ident + "        /// </summary>\n" +
        ident + "        public ushort PrefetchCount { get; init; }\n" +
        "\n" +
        ident + "        /// <summary>\n" +
        ident + "        /// The underlying amqp model/channel\n" +
        ident + "        /// </summary>\n" +
        ident + "        public IModel Model { get; init; }\n" +
        "\n" +
        ident + "        public void Dispose()\n" +
        ident + "        {\n" +
        ident + "            Model?.Close();\n" +
        ident + "            Model?.Dispose();\n" +
        ident + "        }\n" +
        ident + "    }\n" +
        "\n" +
        ident + "    private readonly IConnection _connection;\n" +
        ident + "    private readonly IDictionary<string, Channel> _channels = new Dictionary<string, Channel>();\n" +
        "\n" +
        ident + $"    private {className}ChannelPool(IConnection connection)\n" +
        ident + "    {\n" +
        ident + "        _connection = connection;\n" +
        "\n" +
                         string.Join(string.Empty, GetChannelDeclarations()) +
        ident + "    }\n" +
        "\n" +
        ident + $"    public static {className}ChannelPool Create(IConnection connection)\n" +
        ident + "    {\n" +
        ident + $"        return new {className}ChannelPool(connection);\n" +
        ident + "    }\n" +
        "\n" +
        ident + "    public IModel GetChannel(string operationId)\n" +
        ident + "    {\n" +
        ident + "        // check for channel\n" +
        ident + "        if (!_channels.TryGetValue(operationId, out var channel))\n" +
        ident + "        {\n" +
        ident + "            throw new KeyNotFoundException($\"No channel found for {operationId}\");\n" +
        ident + "        }\n" +
        "\n" +
        ident + "        if (!channel.Model.IsClosed)\n" +
        ident + "        {\n" +
        ident + "            return channel.Model;\n" +
        ident + "        }\n" +
        "\n" +
        ident + "        // recreate channel if it is closed\n" +
        ident + "        _channels[operationId] = CreateChannel(\n" +
        ident + "            _connection,\n" +
        ident + "            channel.PrefetchCount, // prefetch from x-prefetch-count on channel binding\n" +
        ident + "            channel.Confirm); // confirm from confirm on operation binding\n" +
        "\n" +
        ident + "        return _channels[operationId].Model;\n" +
        ident + "    }\n" +
        "\n" +
        ident + "    private Channel CreateChannel(\n" +
        ident + "        IConnection connection,\n" +
        ident + "        ushort prefetchCount = 100,\n" +
        ident + "        bool confirm = false)\n" +
        ident + "    {\n" +
        ident + "        var model = connection.CreateModel();\n" +
        "\n" +
        ident + "        if (confirm)\n" +
        ident + "        {\n" +
        ident + "            model.ConfirmSelect();\n" +
        ident + "        }\n" +
        "\n" +
        ident + "        model.BasicQos(0, prefetchCount, false);\n" +
        "\n" +
        ident + "        return new Channel\n" +
        ident + "        {\n" +
        ident + "            PrefetchCount = prefetchCount,\n" +
        ident + "            Confirm = confirm,\n" +
        ident + "            Model = model\n" +
        ident + "        };\n" +
        ident + "    }\n" +
        "\n" +
        ident + "    public void Dispose()\n" +
        ident + "    {\n" +
        ident + "        foreach (var (_, channel) in _channels)\n" +
        ident + "        {\n" +
        ident + "            channel?.Dispose();\n" +
        ident + "        }\n" +
        ident + "    }\n" +
        ident + "}\n";

        IEnumerable<string> GetChannelDeclarations()
        {
            foreach (var channel in channels)
            {
                var code = (channel.Publish ?? channel.Subscribe)?.OperationId switch
                {
                    "receiveLightMeasurement" => "_channels.Add(\"receiveLightMeasurement\", CreateChannel(connection));",
                    "dimLight" => GetSubscriberChannelDeclaration(channel, "dimLight"),
                    _ => throw new InvalidOperationException("Unknown operationId"),
                };
                yield return $"{ident}        {code}\n";
            }
        }

        string GetSubscriberChannelDeclaration(DOM.Channel channel, string operationId)
        {
            var queue = channel.Bindings?.Amqp?.Queue;
            object? prefetchCount = 0;
            object? confirm = false;
            queue?.AdditionalProperties?.TryGetValue("x-prefetch-count", out prefetchCount);
            queue?.AdditionalProperties?.TryGetValue("x-confirm", out confirm);
            return "_channels.Add(\n" +
                       $"{ident}            \"{operationId}\",\n" +
                       $"{ident}            CreateChannel(\n" +
                       $"{ident}                connection,\n" +
                       $"{ident}                {prefetchCount ?? 1},\n" +
                       $"{ident}                {(confirm ?? false).ToString()!.ToLower()}));";
        }
    }

    public static string GetAmqpUsings()
        => "using System.Text;\n" +
           "using Newtonsoft.Json;\n" +
           "using RabbitMQ.Client;\n" +
           "using RabbitMQ.Client.Events;\n";
}
