using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Microsoft.OpenApi;

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

        var sourceTags = document.Tags;
        document.Tags = new HashSet<OpenApiTag>();

        var sourcePaths = document.Paths;
        document.Paths = [];

        var sourceComponents = document.Components;
        document.Components = new();

        if (metadata.OpenApiComponents is not null)
        {
            document.AddComponents(metadata.OpenApiComponents);
        }

        foreach (var operation in metadata.Operations)
        {
            document.AddTags(operation);
            document.AddPaths(operation);
            document.AddComponents(operation.OpenApiComponents);
        }

        document.AddTags(sourceTags);
        document.AddPaths(sourcePaths);
        document.AddComponents(sourceComponents);
    }

    private static void AddTags(this OpenApiDocument document, EndpointOperation operation)
    {
        if (operation.Tags?.Count is not > 0)
        {
            return;
        }

        if (document.Tags?.Count is not > 0)
        {
            document.Tags = operation.Tags.ToHashSet();
        }
        else
        {
            foreach (var tag in operation.Tags)
            {
                document.Tags.Add(tag);
            }
        }

        if (operation.OpenApiOperation is not null)
        {
            operation.OpenApiOperation.Tags ??= new HashSet<OpenApiTagReference>(capacity: operation.Tags.Count);
            foreach (var tag in operation.Tags)
            {
                if (string.IsNullOrEmpty(tag.Name))
                {
                    continue;
                }

                operation.OpenApiOperation.Tags.Add(
                    item: new(
                        referenceId: tag.Name,
                        hostDocument: document));
            }
        }
    }

    private static void AddPaths(this OpenApiDocument document, EndpointOperation operation)
    {
        var openApiOperation = operation.OpenApiOperation;
        if (openApiOperation is null)
        {
            return;
        }

        document.Paths ??= [];

        var httpMethod = operation.Verb switch
        {
            EndpointVerb.Get => HttpMethod.Get,
            EndpointVerb.Post => HttpMethod.Post,
            EndpointVerb.Put => HttpMethod.Put,
            EndpointVerb.Delete => HttpMethod.Delete,
            EndpointVerb.Options => HttpMethod.Options,
            EndpointVerb.Head => HttpMethod.Head,
            EndpointVerb.Patch => HttpMethod.Patch,
            EndpointVerb.Trace => HttpMethod.Trace,
            _ => throw new InvalidOperationException($"An unexpected endpoint verb: {operation.Verb}")
        };

        var path = new OpenApiPathItem
        {
            Summary = operation.OpenApiOperation?.Summary,
            Description = operation.OpenApiOperation?.Description,
            Operations = new Dictionary<HttpMethod, OpenApiOperation>
            {
                [httpMethod] = openApiOperation
            }
        };

        document.AddPath(operation.Route, path);
    }

    private static void AddPath(this OpenApiDocument document, string route, IOpenApiPathItem item)
    {
        if (document.Paths.TryGetValue(route, out var actualPath))
        {
            actualPath.AddOperations(item);
            return;
        }

        document.Paths[route] = item;
    }

    private static void AddOperations(this IOpenApiPathItem source, IOpenApiPathItem path)
    {
        if (path.Operations?.Count is not > 0)
        {
            return;
        }

        if (source.Operations is null)
        {
            if (source is OpenApiPathItem sourceItem)
            {
                sourceItem.Operations = path.Operations;
            }
            return;
        }

        foreach (var operation in path.Operations)
        {
            _ = source.Operations?.TryAdd(operation.Key, operation.Value);
        }
    }

    private static void AddTags(this OpenApiDocument document, ISet<OpenApiTag>? tags)
    {
        if (tags?.Count is not > 0)
        {
            return;
        }

        if (document.Tags?.Count is not > 0)
        {
            document.Tags = tags;
            return;
        }

        foreach (var tag in tags)
        {
            _ = document.Tags.Add(tag);
        }
    }

    private static void AddPaths(this OpenApiDocument document, OpenApiPaths? paths)
    {
        if (paths?.Count is not > 0)
        {
            return;
        }

        if (document.Paths?.Count is not > 0)
        {
            document.Paths = paths;
            return;
        }

        foreach (var path in paths)
        {
            document.AddPath(path.Key, path.Value);
        }
    }

    private static void AddComponents(this OpenApiDocument document, OpenApiComponents? components)
    {
        if (components is null)
        {
            return;
        }

        document.Components ??= new();

        document.Components.Schemas = MergeWith(document.Components.Schemas, components.Schemas);
        document.Components.SecuritySchemes = MergeWith(document.Components.SecuritySchemes, components.SecuritySchemes);
        document.Components.Responses = MergeWith(document.Components.Responses, components.Responses);
        document.Components.Parameters = MergeWith(document.Components.Parameters, components.Parameters);
        document.Components.Examples = MergeWith(document.Components.Examples, components.Examples);
        document.Components.RequestBodies = MergeWith(document.Components.RequestBodies, components.RequestBodies);
        document.Components.Headers = MergeWith(document.Components.Headers, components.Headers);
        document.Components.Links = MergeWith(document.Components.Links, components.Links);
        document.Components.Callbacks = MergeWith(document.Components.Callbacks, components.Callbacks);
        document.Components.Extensions = MergeWith(document.Components.Extensions, components.Extensions);
    }
}