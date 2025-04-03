namespace PrimeFuncPack;

internal sealed partial class OperationSourceBuilder(EndpointOperationData operation)
{
    public string OperationId { get; }
        =
        operation.OperationId;

    public string InvokeMethodName { get; }
        =
        operation.Method.InvokeMethodName;
}