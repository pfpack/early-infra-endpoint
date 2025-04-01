using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.OpenApi.Models;

namespace PrimeFuncPack;

public sealed class EndpointMetadata
{
    public EndpointMetadata([AllowNull] IReadOnlyCollection<EndpointOperation> operations)
        =>
        Operations = operations ?? [];

    public IReadOnlyCollection<EndpointOperation> Operations { get; }

    public OpenApiComponents? OpenApiComponents { get; init; }
}