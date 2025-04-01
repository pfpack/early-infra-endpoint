namespace PrimeFuncPack;

internal sealed record class EndpointDisciminatedUnionResponse
{
    public EndpointDisciminatedUnionResponse(string foldMethodName, EndpointOperationResponse[] responses)
    {
        FoldMethodName = foldMethodName ?? string.Empty;
        Responses = responses ?? [];
    }

    public string FoldMethodName { get; }

    public EndpointOperationResponse[] Responses { get; }
}