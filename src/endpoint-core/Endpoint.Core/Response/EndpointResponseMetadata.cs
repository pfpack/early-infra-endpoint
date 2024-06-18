using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.OpenApi.Models;

namespace PrimeFuncPack;

public sealed class EndpointResponseMetadata
{
    public EndpointResponseMetadata(
        [AllowNull] OpenApiResponses responses,
        IReadOnlyDictionary<string, OpenApiSchema>? schemas = null)
    {
        Responses = responses ?? [];
        Schemas = schemas;
    }

    public OpenApiResponses Responses { get; }

    public IReadOnlyDictionary<string, OpenApiSchema>? Schemas { get; }
}