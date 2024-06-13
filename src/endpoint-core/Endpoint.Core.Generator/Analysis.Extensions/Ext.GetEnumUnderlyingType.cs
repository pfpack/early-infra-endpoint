using Microsoft.CodeAnalysis;

namespace PrimeFuncPack;

partial class CodeAnalysisExtensions
{
    public static INamedTypeSymbol? GetEnumUnderlyingType(this ITypeSymbol typeSymbol)
        =>
        typeSymbol is INamedTypeSymbol namedTypeSymbol ? namedTypeSymbol.EnumUnderlyingType : null;
}