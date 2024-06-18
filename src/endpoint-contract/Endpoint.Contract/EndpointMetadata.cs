using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.OpenApi.Models;

namespace PrimeFuncPack;

public sealed class EndpointMetadata
{
    public EndpointMetadata(
        [AllowNull] IReadOnlyCollection<EndpointOperation> operations,
        [AllowNull] IReadOnlyCollection<KeyValuePair<string, OpenApiSchema>> schemas = null)
    {
        Operations = operations ?? [];
        Schemas = schemas ?? [];
    }

    public IReadOnlyCollection<EndpointOperation> Operations { get; }

    public IReadOnlyCollection<KeyValuePair<string, OpenApiSchema>> Schemas { get; }
}