namespace ApiCodeGenerator.AsyncApi.DOM;

public class MessageBindings : RefObject<MessageBindings>
{
    public Bindings.Amqp.Message? Amqp { get; set; }
}
