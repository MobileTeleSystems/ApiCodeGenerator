namespace ApiCodeGenerator.AsyncApi.DOM.Bindings.Amqp;

public enum ChannelType
{
#pragma warning disable SA1602 // Enumeration items should be documented
    [System.Runtime.Serialization.EnumMember(Value = @"queue")]
    Queue = 0,

    [System.Runtime.Serialization.EnumMember(Value = @"routingKey")]
    RoutingKey = 1,
#pragma warning restore SA1602 // Enumeration items should be documented
}
