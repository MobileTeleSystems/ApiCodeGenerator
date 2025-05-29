namespace ApiCodeGenerator.AsyncApi.DOM.Serialization;

public class AsyncApiSerializationException : Exception
{
    public AsyncApiSerializationException(string message)
        : this(message, null)
    {
    }

    public AsyncApiSerializationException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
