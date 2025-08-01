﻿using System.Collections.Generic;

namespace PrimeFuncPack;

internal sealed class EndpointExtensionsMetadata
{
    public EndpointExtensionsMetadata(
        string @namespace,
        string typeName,
        DisplayedTypeData providerType,
        IReadOnlyList<string> resolverMethodNames)
    {
        Namespace = @namespace ?? string.Empty;
        TypeName = typeName ?? string.Empty;
        ProviderType = providerType;
        ResolverMethodNames = resolverMethodNames ?? [];
    }

    public string Namespace { get; }

    public string TypeName { get; }

    public DisplayedTypeData ProviderType { get; }

    public IReadOnlyList<string> ResolverMethodNames { get; }
}