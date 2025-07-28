using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace PrimeFuncPack;

internal static partial class EndpointSwaggerConfigurator
{
    private static IDictionary<string, TValue> Combine<TValue>(
        [AllowNull] IDictionary<string, TValue> source,
        [AllowNull] IDictionary<string, TValue> values)
    {
        if (values?.Count is not > 0)
        {
            return source ?? new Dictionary<string, TValue>(capacity: 0);
        }

        if (source?.Count is not > 0)
        {
            return new Dictionary<string, TValue>(values);
        }

        foreach (var value in values)
        {
            _ = source.TryAdd(value.Key, value.Value);
        }

        return source;
    }
}