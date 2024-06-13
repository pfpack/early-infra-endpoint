using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace PrimeFuncPack;

partial class CodeAnalysisExtensions
{
    public static bool IsAnyType(this ITypeSymbol typeSymbol, string @namespace, params string[] types)
    {
        if (typeSymbol is null || types?.Length is not > 0)
        {
            return false;
        }

        if (string.Equals(typeSymbol.ContainingNamespace?.ToString(), @namespace, StringComparison.InvariantCulture) is false)
        {
            return false;
        }

        return types.Any(IsEqualToType);

        bool IsEqualToType(string type)
            =>
            string.Equals(typeSymbol.Name, type, StringComparison.InvariantCulture);
    }
}