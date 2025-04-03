using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace PrimeFuncPack;

internal static partial class SourceGeneratorExtensions
{
    private const string PrimeFuncPackNamespace = "PrimeFuncPack";

    private static IEnumerable<string> FilterNotEmpty(this IEnumerable<string?> source)
    {
        foreach (var item in source)
        {
            if (string.IsNullOrEmpty(item))
            {
                continue;
            }

            yield return item!;
        }
    }

    private static bool IsEndpointApplicationExtensionAttribute(AttributeData attributeData)
        =>
        attributeData.AttributeClass?.IsType(PrimeFuncPackNamespace, "EndpointApplicationExtensionAttribute") is true;

    private static bool IsResolveMethod(IMethodSymbol method)
    {
        if (method.IsStatic || method.IsGenericMethod || method.DeclaredAccessibility is not Accessibility.Public)
        {
            return false;
        }

        if (string.Equals("Resolve", method.Name, StringComparison.InvariantCulture) is false)
        {
            return false;
        }

        if (method.Parameters.Length is not 1)
        {
            return false;
        }

        if (method.Parameters[0].Type is not INamedTypeSymbol methodType)
        {
            return false;
        }

        if (methodType.IsType("System", "IServiceProvider") is false)
        {
            return false;
        }

        if (method.ReturnType is not INamedTypeSymbol returnType)
        {
            return false;
        }

        if (returnType.AllInterfaces.Any(IsEndpointInvokeSupplier) is not true)
        {
            return false;
        }

        if (returnType.AllInterfaces.Any(IsEndpointMetadataProvider) is not true)
        {
            return false;
        }

        return true;

        static bool IsEndpointInvokeSupplier(INamedTypeSymbol typeSymbol)
            =>
            typeSymbol.IsType(PrimeFuncPackNamespace, "IEndpointInvokeSupplier");

        static bool IsEndpointMetadataProvider(INamedTypeSymbol typeSymbol)
            =>
            typeSymbol.IsType(PrimeFuncPackNamespace, "IEndpointMetadataProvider");
    }

    private static InvalidOperationException CreateInvalidMethodException(this IMethodSymbol resolverMethod, string message)
        =>
        new($"Endpoint resolver method {resolverMethod.ContainingType?.Name}.{resolverMethod.Name} {message}");
}