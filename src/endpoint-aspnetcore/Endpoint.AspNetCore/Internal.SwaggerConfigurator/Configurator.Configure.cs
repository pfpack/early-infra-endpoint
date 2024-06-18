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
        if (metadata is null)
        {
            return;
        }

        document.Tags ??= [];
        document.Paths ??= [];

        foreach (var operation in metadata.Operations)
        {
            document.AddOperation(operation);
        }

        document.AddSchemas(metadata);
    }

    private static void AddOperation(this OpenApiDocument document, EndpointOperation operation)
    {
        var openApiOperation = operation.OpenApiOperation;
        if (openApiOperation is null)
        {
            return;
        }

        if (openApiOperation.Tags?.Count > 0)
        {
            document.Tags = document.Tags.InsertTags(openApiOperation.Tags);
        }

        var (paths, pathItem) = document.Paths.GetOrCreatePathItem(operation);

        var operationType = operation.Verb.ToOperationType();
        if (pathItem.Operations?.ContainsKey(operationType) is true)
        {
            document.Paths = paths;
            return;
        }

        if (pathItem.Operations is not null)
        {
            pathItem.Operations = pathItem.Operations.ToDictionary().Insert(operationType, openApiOperation);
            document.Paths = paths;

            return;
        }

        pathItem.Operations = new Dictionary<OperationType, OpenApiOperation>
        {
            [operationType] = openApiOperation
        };

        document.Paths = paths;
    }

    private static void AddSchemas(this OpenApiDocument document, EndpointMetadata endpoint)
    {
        if (endpoint.Schemas?.Count is not > 0)
        {
            return;
        }

        document.Components ??= new();
        var currentSchemas = document.Components.Schemas ?? Enumerable.Empty<KeyValuePair<string, OpenApiSchema>>();
        var schemas = new Dictionary<string, OpenApiSchema>(currentSchemas, StringComparer.InvariantCultureIgnoreCase);

        foreach (var schema in endpoint.Schemas)
        {
            if (schemas.ContainsKey(schema.Key))
            {
                continue;
            }

            schemas.Add(schema.Key, schema.Value);
        }
    }
}