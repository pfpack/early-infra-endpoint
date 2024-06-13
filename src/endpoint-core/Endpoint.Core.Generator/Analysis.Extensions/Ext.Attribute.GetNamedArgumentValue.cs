using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace PrimeFuncPack;

partial class CodeAnalysisExtensions
{
    public static T? GetNamedArgumentValue<T>(this AttributeData attributeData, string propertyName)
        =>
        (T?)attributeData.InnerGetNamedArgumentValue(propertyName);

    private static object? InnerGetNamedArgumentValue(this AttributeData? attributeData, string propertyName)
    {
        return attributeData?.NamedArguments.FirstOrDefault(IsNameMatched).Value.Value;

        bool IsNameMatched(KeyValuePair<string, TypedConstant> pair)
            =>
            string.Equals(pair.Key, propertyName, StringComparison.InvariantCulture);
    }
}