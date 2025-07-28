using Microsoft.OpenApi;

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

            result.Schemas = Join(result.Schemas, components.Schemas);
            result.Responses = Join(result.Responses, components.Responses);
            result.Parameters = Join(result.Parameters, components.Parameters);
            result.Examples = Join(result.Examples, components.Examples);
            result.RequestBodies = Join(result.RequestBodies, components.RequestBodies);
            result.Headers = Join(result.Headers, components.Headers);
            result.Links = Join(result.Links, components.Links);
            result.Callbacks = Join(result.Callbacks, components.Callbacks);
            result.Extensions = Join(result.Extensions, components.Extensions);
        }

        return result;
    }
}