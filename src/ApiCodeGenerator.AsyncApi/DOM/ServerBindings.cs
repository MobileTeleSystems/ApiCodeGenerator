using Newtonsoft.Json;

namespace ApiCodeGenerator.AsyncApi.DOM
{
    public class ServerBindings : RefObject<ServerBindings>
    {
        public Bindings.Amqp.Server? Amqp { get; set; }
    }
}
