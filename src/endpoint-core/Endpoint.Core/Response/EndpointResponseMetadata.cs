using Microsoft.OpenApi.Models;

namespace PrimeFuncPack;

public sealed class EndpointResponseMetadata
{
    public OpenApiResponses? Responses { get; init; }

    public OpenApiComponents? Components { get; init; }
}