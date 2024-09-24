namespace ApiCodeGenerator.AsyncApi.CSharp;

[Flags]
public enum OperationTypes
{
    Publish = 1,
    Subscribe = 2,
    All = 3,
}
