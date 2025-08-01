﻿using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.OpenApi;

namespace PrimeFuncPack;

public sealed class EndpointOperation
{
    public EndpointOperation([AllowNull] string id, EndpointVerb verb, [AllowNull] string route)
    {
        Id = id ?? string.Empty;
        Verb = verb;
        Route = route ?? string.Empty;
    }

    public string Id { get; }

    public EndpointVerb Verb { get; }

    public string Route { get; }

    public IReadOnlyCollection<OpenApiTag>? Tags { get; init; }

    public OpenApiOperation? OpenApiOperation { get; init; }

    public OpenApiComponents? OpenApiComponents { get; init; }
}