namespace PrimeFuncPack;

public static class EndpointEmptyResponseMapper<TResponse>
    where TResponse : struct
{
    public static EndpointResponse MapResponse(TResponse _)
        =>
        EndpointAbsentResponseProvider.GetResponse();
}