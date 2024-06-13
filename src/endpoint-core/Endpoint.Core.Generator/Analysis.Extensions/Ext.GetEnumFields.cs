using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace PrimeFuncPack;

partial class CodeAnalysisExtensions
{
    public static IEnumerable<IFieldSymbol> GetEnumFields(this ITypeSymbol typeSymbol)
    {
        if (typeSymbol is null)
        {
            return [];
        }

        return typeSymbol.GetMembers().OfType<IFieldSymbol>().Where(IsPublic).Where(IsNameNotEmpty);

        static bool IsPublic(IFieldSymbol fieldSymbol)
            =>
            fieldSymbol.DeclaredAccessibility is Accessibility.Public;

        static bool IsNameNotEmpty(IFieldSymbol fieldSymbol)
            =>
            string.IsNullOrEmpty(fieldSymbol.Name) is false;
    }
}