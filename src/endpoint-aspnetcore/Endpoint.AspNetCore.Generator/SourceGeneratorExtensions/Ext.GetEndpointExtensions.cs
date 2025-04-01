using System;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PrimeFuncPack;

partial class SourceGeneratorExtensions
{
    internal static IncrementalValuesProvider<EndpointExtensionsMetadata> GetEndpointExtensionsValueProvider(
        this IncrementalGeneratorInitializationContext context)
    {
        return context.SyntaxProvider.CreateSyntaxProvider(IsActualType, GetMetadata).Where(IsNotNull).Select(AsNotNullable);

        static bool IsActualType(SyntaxNode syntaxNode, CancellationToken _)
        {
            if (syntaxNode is not ClassDeclarationSyntax classDeclaration)
            {
                return false;
            }

            var modifiers = classDeclaration.Modifiers;
            return modifiers.Any(SyntaxKind.PublicKeyword) || modifiers.Any(SyntaxKind.InternalKeyword);
        }

        static bool IsNotNull(EndpointExtensionsMetadata? metadata)
            =>
            metadata is not null;

        static EndpointExtensionsMetadata AsNotNullable(EndpointExtensionsMetadata? metadata, CancellationToken _)
            =>
            metadata!;
    }

    private static EndpointExtensionsMetadata? GetMetadata(GeneratorSyntaxContext context, CancellationToken _)
    {
        if (context.SemanticModel.GetDeclaredSymbol((ClassDeclarationSyntax)context.Node) is not INamedTypeSymbol typeSymbol)
        {
            return null;
        }

        var resolverMethodNames = typeSymbol.GetMembers().OfType<IMethodSymbol>().Select(GetResolverMethodName).FilterNotEmpty().ToArray();
        if (resolverMethodNames.Length is 0)
        {
            return null;
        }

        if (typeSymbol.TypeArguments.Length > 0)
        {
            throw new InvalidOperationException($"Endpoint resolver type '{typeSymbol.Name}' must have no generic parameters");
        }

        return new(
            @namespace: typeSymbol.ContainingNamespace.ToString(),
            typeName: typeSymbol.Name + "EndpointExtensions",
            providerType: typeSymbol.GetDisplayedData(),
            resolverMethodNames: resolverMethodNames);
    }

    private static string? GetResolverMethodName(IMethodSymbol methodSymbol)
    {
        if (methodSymbol.GetAttributes().Any(IsEndpointApplicationExtensionAttribute) is false)
        {
            return null;
        }

        if (methodSymbol.IsStatic is false)
        {
            throw methodSymbol.CreateInvalidMethodException("must be static");
        }

        if (methodSymbol.DeclaredAccessibility is not (Accessibility.Public or Accessibility.Internal))
        {
            throw methodSymbol.CreateInvalidMethodException("must be public or internal");
        }

        if (methodSymbol.TypeParameters.Length > 0)
        {
            throw methodSymbol.CreateInvalidMethodException("must have no generic parameters");
        }

        if (methodSymbol.Parameters.Length > 0)
        {
            throw methodSymbol.CreateInvalidMethodException("must have no parameters");
        }

        if (methodSymbol.ReturnType.GetMembers().OfType<IMethodSymbol>().Any(IsResolveMethod) is false)
        {
            throw methodSymbol.CreateInvalidMethodException("must return a type that contains Resolve method");
        }

        return methodSymbol.Name;
    }
}