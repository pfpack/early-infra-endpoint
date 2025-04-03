using System.Threading;
using System.Threading.Tasks;

namespace PrimeFuncPack;

public interface IEndpointResponseMapper<TResponse>
{
    static abstract ValueTask<EndpointResponse> MapResponseAsync(TResponse response, CancellationToken cancellationToken);
}