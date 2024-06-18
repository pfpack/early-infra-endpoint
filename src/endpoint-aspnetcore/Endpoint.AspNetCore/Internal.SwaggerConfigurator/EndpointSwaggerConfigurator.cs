using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi.Models;

namespace PrimeFuncPack;

internal static partial class EndpointSwaggerConfigurator
{
    private static List<OpenApiTag> InsertTags(this IList<OpenApiTag> documentTags, IEnumerable<OpenApiTag> tags)
    {
        var tagsDictionary = new Dictionary<string, OpenApiTag>(StringComparer.InvariantCultureIgnoreCase);
        foreach (var tag in tags.Reverse().Concat(documentTags))
        {
            var key = GetTagKey(tag);
            if (tagsDictionary.ContainsKey(key))
            {
                continue;
            }

            tagsDictionary[key] = tag;
        }

        return tagsDictionary.Values.ToList();

        static string GetTagKey(OpenApiTag tag)
            =>
            tag.Name ?? string.Empty;
    }

    private static (OpenApiPaths Paths, OpenApiPathItem Item) GetOrCreatePathItem(this OpenApiPaths paths, EndpointOperation operation)
    {
        if (paths.TryGetValue(operation.Route, out var pathItem))
        {
            return (paths, pathItem);
        }

        var createdItem = new OpenApiPathItem
        {
            Summary = operation.OpenApiOperation?.Summary,
            Description = operation.OpenApiOperation?.Description
        };

        return(paths.Insert(operation.Route, createdItem), createdItem);
    }

    private static OperationType ToOperationType(this EndpointVerb verb)
        =>
        verb switch
        {
            EndpointVerb.Get => OperationType.Get,
            EndpointVerb.Post => OperationType.Post,
            EndpointVerb.Put => OperationType.Put,
            EndpointVerb.Delete => OperationType.Delete,
            EndpointVerb.Options => OperationType.Options,
            EndpointVerb.Head => OperationType.Head,
            EndpointVerb.Patch => OperationType.Patch,
            EndpointVerb.Trace => OperationType.Trace,
            _ => throw new InvalidOperationException($"An unexpected endpoint verb: {verb}")
        };

    private static TDictionary Insert<TDictionary, TKey, TValue>(this TDictionary source, TKey key, TValue value)
        where TDictionary : IDictionary<TKey, TValue>, new()
        where TKey : notnull
    {
        var result = new TDictionary
        {
            { key, value }
        };

        foreach (var kv in source)
        {
            result.Add(kv.Key, kv.Value);
        }

        return result;
    }

    private static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> source)
        where TKey : notnull
    {
        return source.ToDictionary(GetKey, GetValue);

        static TKey GetKey(KeyValuePair<TKey, TValue> kv)
            =>
            kv.Key;

        static TValue GetValue(KeyValuePair<TKey, TValue> kv)
            =>
            kv.Value;
    }
}