namespace PrimeFuncPack;

internal sealed record class EndpointOperationMethod
{
    public EndpointOperationMethod(string apiMethodName, string invokeMethodName)
    {
        ApiMethodName = apiMethodName ?? string.Empty;
        InvokeMethodName = invokeMethodName ?? string.Empty;
    }

    public string ApiMethodName { get; }

    public string InvokeMethodName { get; }
}