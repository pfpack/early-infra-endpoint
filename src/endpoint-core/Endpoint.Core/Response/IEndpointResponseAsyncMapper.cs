using System.Threading;
using System.Threading.Tasks;

namespace PrimeFuncPack;

public interface IEndpointResponseAsyncMapper<TResponse>
{
    Task<EndpointResponse> MapResponseAsync(TResponse response, CancellationToken cancellationToken = default);
}