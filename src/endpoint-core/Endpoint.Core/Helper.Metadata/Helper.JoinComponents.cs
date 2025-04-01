using Microsoft.OpenApi.Models;

namespace PrimeFuncPack;

partial class EndpointMetadataHelper
{
    public static OpenApiComponents? JoinComponents(params OpenApiComponents?[] allComponents)
    {
        if (allComponents is null)
        {
            return null;
        }

        OpenApiComponents? result = null;

        foreach (var components in allComponents)
        {
            if (components is null)
            {
                continue;
            }

            result ??= new();

            result.Schemas.AddValues(components.Schemas);
            result.Responses.AddValues(components.Responses);
            result.Parameters.AddValues(components.Parameters);
            result.Examples.AddValues(components.Examples);
            result.RequestBodies.AddValues(components.RequestBodies);
            result.Headers.AddValues(components.Headers);
            result.Links.AddValues(components.Links);
            result.Callbacks.AddValues(components.Callbacks);
            result.Extensions.AddValues(components.Extensions);
        }

        return result;
    }
}