using System.Collections.Generic;

namespace PrimeFuncPack;

public static partial class EndpointMetadataHelper
{
    private static void AddValues<TValue>(this IDictionary<string, TValue> source, IDictionary<string, TValue>? values)
    {
        if (values?.Count is not > 0)
        {
            return;
        }

        foreach (var value in values)
        {
            _ = source.TryAdd(value.Key, value.Value);
        }
    }
}