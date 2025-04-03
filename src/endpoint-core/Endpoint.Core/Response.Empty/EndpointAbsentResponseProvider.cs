namespace PrimeFuncPack;

public static class EndpointAbsentResponseProvider
{
    internal const int StatusCode = 204;

    public static EndpointResponse GetResponse()
        =>
        new(
            statusCode: StatusCode);
}