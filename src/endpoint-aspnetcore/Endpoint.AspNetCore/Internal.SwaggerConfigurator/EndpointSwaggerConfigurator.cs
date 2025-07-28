using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace PrimeFuncPack;

internal static partial class EndpointSwaggerConfigurator
{
    private static IDictionary<string, TValue> MergeWith<TValue>(
        [AllowNull] IDictionary<string, TValue> accumulate,
        [AllowNull] IDictionary<string, TValue> other)
    {
        if (other?.Count is not > 0)
        {
            return accumulate ?? new Dictionary<string, TValue>(capacity: 0);
        }

        if (accumulate?.Count is not > 0)
        {
            return new Dictionary<string, TValue>(other);
        }

        foreach (var pair in other)
        {
            _ = accumulate.TryAdd(pair.Key, pair.Value);
        }

        return accumulate;
    }
}