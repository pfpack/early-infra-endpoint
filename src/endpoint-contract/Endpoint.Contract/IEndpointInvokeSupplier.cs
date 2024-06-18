using System.Threading;
using System.Threading.Tasks;

namespace PrimeFuncPack;

public interface IEndpointInvokeSupplier
{
    Task<EndpointResponse> InvokeAsync(EndpointRequest request, CancellationToken cancellationToken);
}