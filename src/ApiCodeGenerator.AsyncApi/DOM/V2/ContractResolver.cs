using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace ApiCodeGenerator.AsyncApi.DOM.V2;

internal class ContractResolver : DefaultContractResolver
{
    private readonly ChannelItemConverter _channelItemConverter;

    public ContractResolver(ChannelItemConverter channelItemConverter)
    {
        _channelItemConverter = channelItemConverter;
    }

    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
        var jProperty = base.CreateProperty(member, memberSerialization);
        if (member is PropertyInfo propertyInfo && ChannelItemConverter.CanConvert_(propertyInfo.PropertyType))
        {
            jProperty.Converter = _channelItemConverter;
        }

        return jProperty;
    }

    protected override JsonConverter? ResolveContractConverter(Type objectType)
    {
        // disable converter defined on JsonExtensionObject for use converter added to serializer settings
        if (MessageConverter.CanConvert_(objectType))
        {
            return null;
        }

        return base.ResolveContractConverter(objectType);
    }
}
