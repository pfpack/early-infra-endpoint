namespace PrimeFuncPack;

public interface IEndpointResponseMapper<TResponse>
{
    EndpointResponse MapResponse(TResponse response);
}