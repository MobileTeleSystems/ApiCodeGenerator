namespace ApiCodeGenerator.AsyncApi.DOM.Bindings.Amqp;

public enum ExchangeType
{
#pragma warning disable SA1602 // Enumeration items should be documented
    [System.Runtime.Serialization.EnumMember(Value = @"topic")]
    Topic = 0,

    [System.Runtime.Serialization.EnumMember(Value = @"direct")]
    Direct = 1,

    [System.Runtime.Serialization.EnumMember(Value = @"fanout")]
    Fanout = 2,

    [System.Runtime.Serialization.EnumMember(Value = @"default")]
    Default = 3,

    [System.Runtime.Serialization.EnumMember(Value = @"headers")]
    Headers = 4,
#pragma warning restore SA1602 // Enumeration items should be documented
}
