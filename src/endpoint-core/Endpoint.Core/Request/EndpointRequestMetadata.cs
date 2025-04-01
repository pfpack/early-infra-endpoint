using Microsoft.OpenApi.Models;

namespace PrimeFuncPack;

public sealed class EndpointRequestMetadata
{
    public OpenApiParameter[]? Parameters { get; init; }

    public OpenApiRequestBody? RequestBody { get; init; }

    public OpenApiComponents? Components { get; init; }
}