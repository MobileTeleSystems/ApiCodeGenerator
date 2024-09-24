namespace ApiCodeGenerator.AsyncApi.DOM;

public class ChannelBindings : RefObject<ChannelBindings>
{
    public Bindings.Amqp.Channel? Amqp { get; set; }
}
