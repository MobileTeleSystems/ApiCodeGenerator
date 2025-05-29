namespace ApiCodeGenerator.AsyncApi.CSharp;

[Flags]
public enum OperationTypes
{
    Send = 1,
    Receive = 2,
    All = 3,
    Subscribe = 1,
    Publish = 2,
}
