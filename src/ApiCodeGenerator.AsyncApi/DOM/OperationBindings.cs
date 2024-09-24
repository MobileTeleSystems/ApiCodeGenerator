namespace ApiCodeGenerator.AsyncApi.DOM;

public class OperationBindings : RefObject<OperationBindings>
{
    public Bindings.Amqp.OperationBase? Amqp { get; set; }
}
