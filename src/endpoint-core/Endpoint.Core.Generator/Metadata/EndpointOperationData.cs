namespace PrimeFuncPack;

internal sealed record class EndpointOperationData
{
    public EndpointOperationData(
        string operationId,
        EndpointOperationMetadata metadata,
        EndpointOperationMethod method,
        EndpointOperationRequest request,
        EndpointOperationResponse response)
    {
        OperationId = operationId ?? string.Empty;
        Metadata = metadata;
        Method = method;
        Request = request;
        Response = response;
    }

    public string OperationId { get; }

    public EndpointOperationMetadata Metadata { get; }

    public EndpointOperationMethod Method { get; }

    public EndpointOperationRequest Request { get; }

    public EndpointOperationResponse Response { get; }

    public ObsoleteData? ObsoleteData { get; set; }
}