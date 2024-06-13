using Microsoft.CodeAnalysis;

namespace PrimeFuncPack;

partial class CodeAnalysisExtensions
{
    public static T? GetConstructorArgumentValue<T>(this AttributeData attributeData, int constructorOrder)
        =>
        (T?)attributeData.InnerGetConstructorArgumentValue(constructorOrder);

    private static object? InnerGetConstructorArgumentValue(this AttributeData? attributeData, int constructorOrder)
    {
        if (attributeData?.ConstructorArguments.Length <= constructorOrder)
        {
            return default;
        }

        return attributeData?.ConstructorArguments[constructorOrder].Value;
    }
}