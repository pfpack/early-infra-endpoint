using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi.Models;

namespace PrimeFuncPack;

partial class EndpointSwaggerConfigurator
{
    internal static void Configure<TEndpoint>(OpenApiDocument document)
        where TEndpoint : IEndpointMetadataProvider
    {
        if (document is null)
        {
            return;
        }

        var metadata = TEndpoint.Metadata;

        document.Paths ??= [];

        var tags = new List<OpenApiTag>();
        var paths = new List<KeyValuePair<string, OpenApiPathItem>>();

        var components = new List<OpenApiComponents>();
        if (metadata.OpenApiComponents is not null)
        {
            components.Add(metadata.OpenApiComponents);
        }

        foreach (var operation in metadata.Operations)
        {
            tags.AddTags(operation);
            paths.AddPaths(operation);
            components.AddComponents(operation);
        }

        document.InsertTags(tags);
        document.InsertPaths(paths);
        document.InsertComponents(components);
    }

    private static void AddTags(this List<OpenApiTag> tags, EndpointOperation operation)
    {
        if (operation.OpenApiOperation?.Tags?.Count > 0)
        {
            tags.AddRange(operation.OpenApiOperation.Tags);
        }
    }

    private static void AddPaths(this List<KeyValuePair<string, OpenApiPathItem>> paths, EndpointOperation operation)
    {
        var openApiOperation = operation.OpenApiOperation;
        if (openApiOperation is null)
        {
            return;
        }

        var operationType = operation.Verb switch
        {
            EndpointVerb.Get => OperationType.Get,
            EndpointVerb.Post => OperationType.Post,
            EndpointVerb.Put => OperationType.Put,
            EndpointVerb.Delete => OperationType.Delete,
            EndpointVerb.Options => OperationType.Options,
            EndpointVerb.Head => OperationType.Head,
            EndpointVerb.Patch => OperationType.Patch,
            EndpointVerb.Trace => OperationType.Trace,
            _ => throw new InvalidOperationException($"An unexpected endpoint verb: {operation.Verb}")
        };

        paths.Add(
            item: new(
                key: operation.Route,
                value: new()
                {
                    Summary = operation.OpenApiOperation?.Summary,
                    Description = operation.OpenApiOperation?.Description,
                    Operations = new Dictionary<OperationType, OpenApiOperation>
                    {
                        [operationType] = openApiOperation
                    }
                }));
    }

    private static void AddComponents(this List<OpenApiComponents> components, EndpointOperation operation)
    {
        if (operation.OpenApiComponents is null)
        {
            return;
        }

        components.Add(operation.OpenApiComponents);
    }

    private static void InsertTags(this OpenApiDocument document, List<OpenApiTag> tags)
    {
        if (tags.Count is 0)
        {
            return;
        }

        if (document.Tags?.Count is not > 0)
        {
            document.Tags = tags.ToArray();
            return;
        }

        var tagsDictionary = new Dictionary<string, OpenApiTag>(
            capacity: tags.Count + document.Tags.Count,
            comparer: StringComparer.InvariantCultureIgnoreCase);

        foreach (var tag in tags.Concat(document.Tags))
        {
            var key = tag.Name ?? string.Empty;
            _ = tagsDictionary.TryAdd(key, tag);
        }

        document.Tags = tagsDictionary.Values.ToList();
    }

    private static void InsertPaths(this OpenApiDocument document, List<KeyValuePair<string, OpenApiPathItem>> paths)
    {
        if (paths.Count is 0)
        {
            return;
        }

        var resultPaths = new OpenApiPaths();

        foreach (var path in paths.Concat(document.Paths ?? []))
        {
            if (resultPaths.TryGetValue(path.Key, out var actualPath))
            {
                actualPath.ConcatOperations(path.Value);
                continue;
            }

            resultPaths.Add(path.Key, path.Value);
        }

        document.Paths = resultPaths;
    }

    private static void ConcatOperations(this OpenApiPathItem source, OpenApiPathItem additional)
    {
        if (additional.Operations?.Count is not > 0)
        {
            return;
        }

        if (source.Operations?.Count is not > 0)
        {
            source.Operations = additional.Operations;
            return;
        }

        foreach (var operation in additional.Operations)
        {
            _ = source.Operations.TryAdd(operation.Key, operation.Value);
        }
    }

    private static void InsertComponents(this OpenApiDocument document, List<OpenApiComponents> components)
    {
        if (components.Count is 0)
        {
            return;
        }

        var result = new OpenApiComponents();
        var allComponents = document.Components is null ? components : components.Concat([document.Components]);

        foreach (var component in allComponents)
        {
            result.Schemas.AddValues(component.Schemas);
            result.Responses.AddValues(component.Responses);
            result.Parameters.AddValues(component.Parameters);
            result.Examples.AddValues(component.Examples);
            result.RequestBodies.AddValues(component.RequestBodies);
            result.Headers.AddValues(component.Headers);
            result.Links.AddValues(component.Links);
            result.Callbacks.AddValues(component.Callbacks);
            result.Extensions.AddValues(component.Extensions);
        }

        document.Components = result;
    }
}