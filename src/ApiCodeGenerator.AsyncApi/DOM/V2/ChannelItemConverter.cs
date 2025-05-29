using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NJsonSchema.References;

namespace ApiCodeGenerator.AsyncApi.DOM.V2;

internal class ChannelItemConverter : JsonConverter
{
    private readonly AsyncApiDocument _document;
    private readonly ReferenceResolver _referenceResolver;

    public ChannelItemConverter(AsyncApiDocument document, ReferenceResolver referenceResolver)
    {
        _document = document;
        _referenceResolver = referenceResolver;
    }

    public override bool CanWrite => false;

    public static bool CanConvert_(Type objectType) =>
        typeof(IDictionary<string, NamedReference<Channel>>) == objectType
        || typeof(IDictionary<string, Reference<Channel>>) == objectType;

    public override bool CanConvert(Type objectType) => CanConvert_(objectType);

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
        {
            return null;
        }

        var targetRefType = objectType.GetGenericArguments()[1];
        var contract = serializer.ContractResolver.ResolveContract(objectType);
        if (targetRefType.GetGenericTypeDefinition() == typeof(NamedReference<>))
        {
            return ReadJson<NamedReference<Channel>>(reader, existingValue, serializer, c => c);
        }
        else if (targetRefType.GetGenericTypeDefinition() == typeof(Reference<>))
        {
            return ReadJson<Reference<Channel>>(reader, existingValue, serializer, c => c);
        }

        return null;
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) => throw new NotSupportedException();

    private static string GetChannelId(string address)
    {
        var segments = address.Split(['/', '.', '{', '}'], StringSplitOptions.RemoveEmptyEntries)
        .Select((s, i) => (i == 0 ? s.Substring(0, 1).ToLowerInvariant() : s.Substring(0, 1).ToUpperInvariant()) + s.Substring(1));
        return string.Join(string.Empty, segments);
    }

    private IDictionary<string, TRef> ReadJson<TRef>(
        JsonReader reader,
        object? existingValue,
        JsonSerializer serializer,
        Func<Channel, TRef> refFactory)
        where TRef : Reference<Channel>, new()
    {
        var channelItems = serializer.Deserialize<IDictionary<string, Reference<ChannelItem>>>(reader)!;
        var dictionary = existingValue as IDictionary<string, TRef> ?? new Dictionary<string, TRef>();

        foreach (var item in channelItems)
        {
            var address = item.Key;

            var channelId = GetChannelId(address);
            TRef? newRef = null;
            if (item.Value is not null)
            {
                if (string.IsNullOrEmpty(item.Value.ReferencePath))
                {
                    var isComponent = reader.Path.StartsWith("components.channels");
                    if (isComponent)
                    {
                        address = GetAddressFromRef(address);
                    }

                    var channelItem = item.Value.ActualObject;
                    var channel = ChannelItemToV3(channelItem, address);
                    ReadOperations(channelItem, channelId, channel.Messages, serializer, isComponent);
                    newRef = refFactory(channel);
                }
                else
                {
                    newRef = new() { ReferencePath = item.Value.ReferencePath };
                }
            }

            dictionary[channelId] = newRef!;
        }

        return dictionary;
    }

    private string GetAddressFromRef(string address)
    {
        var refPath = $"#/components/channels/{address}";
        return _referenceResolver.GetChannelIdByRefPath(refPath);
    }

    private void ReadOperations(
        ChannelItem channelItem,
        string channelId,
        IDictionary<string, NamedReference<DOM.Message>> channelMessages,
        JsonSerializer serializer,
        bool isComponent)
    {
        var channelRef = $"#/{(isComponent ? "components/" : string.Empty)}channels/{channelId}";
        var path = $"{(isComponent ? "components." : string.Empty)}channels.{channelId}";
        var counter = 1;
        ReadOperation(channelItem.Subscribe, OperationAction.Send);
        ReadOperation(channelItem.Publish, OperationAction.Receive);

        void ReadOperation(Operation? op, OperationAction action)
        {
            if (op is null)
            {
                return;
            }

            var operation = new DOM.Operation
            {
                Action = action,
                Channel = new Reference<Channel> { ReferencePath = channelRef },
                Bindings = op.Bindings,
                Description = op.Desciption,
                ExtensionData = op.ExtensionData,
                ExternalDocs = op.ExternalDocs,
                Security = _referenceResolver.GetSecuritySchemeRefs(op.Security, path + ".security"),
                Summary = op.Summary,
                Tags = op.Tags,
                Traits = op.Traits,
            };

            if (op.Message is not null && op.Message.Type == JTokenType.Object)
            {
                var oneOf = (JArray?)op.Message.GetValue("oneOf");
                var messagesV2 = oneOf is null
                    ? Enumerable.Repeat(op.Message.ToObject<Reference<Message>>(serializer)!, 1)
                    : oneOf.ToObject<Reference<Message>[]>(serializer)!;

                List<Reference<DOM.Message>> operationMessages = new();

                foreach (var message in messagesV2)
                {
                    if (!string.IsNullOrEmpty(message.ReferencePath))
                    {
                        var name = message.ReferencePath!.Substring(message.ReferencePath.LastIndexOf("/") + 1);
                        if (!channelMessages.ContainsKey(name))
                        {
                            channelMessages.Add(name, new() { ReferencePath = message.ReferencePath });
                        }

                        operationMessages.Add(new Reference<DOM.Message> { ReferencePath = $"{channelRef}/messages/{name}" });
                    }
                    else
                    {
                        var msg2 = message.ActualObject;
                        var name = msg2.MessageId ?? $"msg{counter++}";
                        if (string.IsNullOrEmpty(msg2.MessageId) || !channelMessages.ContainsKey(msg2.MessageId!))
                        {
                            var msg3 = MessageConverter.MessageToV3(msg2, serializer);
                            channelMessages.Add(name, msg3);
                        }

                        operationMessages.Add(new Reference<DOM.Message> { ReferencePath = $"{channelRef}/messages/{name}" });
                    }
                }

                operation.Messages = operationMessages.ToArray();
            }

            var operName = op.OperationId ?? $"channelId{action}";
            if (isComponent)
            {
                (_document.Components.Operations ??= new Dictionary<string, Reference<DOM.Operation>>()).Add(operName, operation);
                var operRef = new NamedReference<DOM.Operation> { ReferencePath = $"#/components/operations/{operName}" };
                _document.Operations.Add(operName, operRef);
            }
            else
            {
                _document.Operations.Add(operName, operation);
            }
        }
    }

    private Channel ChannelItemToV3(ChannelItem channelItem, string address)
    {
        Channel result = new()
        {
            Address = address,
            Bindings = channelItem.Bindings,
            Description = channelItem.Description,
            ExtensionData = channelItem.ExtensionData,
            Servers = GetServers(_referenceResolver),
        };
        PopulateParameters(result);
        return result;

        ICollection<Reference<DOM.Server>>? GetServers(ReferenceResolver serverResolver)
        {
            if (channelItem.Servers is not null)
            {
                return channelItem.Servers
                    .Select(serverResolver.GetServerRefByName)
                    .Where(r => !string.IsNullOrEmpty(r))
                    .Select(r => new Reference<DOM.Server> { ReferencePath = r })
                    .ToArray();
            }

            return null;
        }

        void PopulateParameters(Channel result)
        {
            if (channelItem.Parameters is not null)
            {
                foreach (var pair in channelItem.Parameters)
                {
                    var r = pair.Value.ReferencePath;
                    if (r is not null)
                    {
                        result.Parameters.Add(pair.Key, new() { ReferencePath = r });
                    }
                    else
                    {
                        result.Parameters.Add(pair.Key, pair.Value.ActualObject);
                    }
                }
            }
        }
    }
}
