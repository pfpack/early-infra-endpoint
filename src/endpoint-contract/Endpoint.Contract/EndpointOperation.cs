using Microsoft.OpenApi.Models;

namespace PrimeFuncPack;

public sealed class EndpointOperation
{
    public EndpointOperation(string id, EndpointVerb verb, string route, OpenApiOperation? openApiOperation = null)
    {
        Id = id ?? string.Empty;
        Verb = verb;
        Route = route ?? string.Empty;
        OpenApiOperation = openApiOperation;
    }

    public string Id { get; }

    public EndpointVerb Verb { get; }

    public string Route { get; }

    public OpenApiOperation? OpenApiOperation { get; }
}