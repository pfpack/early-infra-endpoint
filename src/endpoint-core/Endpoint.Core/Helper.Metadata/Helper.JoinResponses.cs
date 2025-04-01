using Microsoft.OpenApi.Models;

namespace PrimeFuncPack;

partial class EndpointMetadataHelper
{
    public static OpenApiResponses? JoinResponses(params OpenApiResponses?[] allResponses)
    {
        if (allResponses is null)
        {
            return null;
        }

        OpenApiResponses? result = null;

        foreach (var responses in allResponses)
        {
            if (responses is null)
            {
                continue;
            }

            result ??= [];

            foreach (var response in responses)
            {
                _ = result.TryAdd(response.Key, response.Value);
            }
        }

        return result;
    }
}