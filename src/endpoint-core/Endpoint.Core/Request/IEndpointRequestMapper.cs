using System.Threading;
using System.Threading.Tasks;

namespace PrimeFuncPack;

public interface IEndpointRequestMapper<TRequest>
{
    static abstract ValueTask<TRequest> MapRequestAsync(EndpointRequest request, CancellationToken cancellationToken);
}