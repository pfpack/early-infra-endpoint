namespace PrimeFuncPack;

internal sealed record class EndpointOperationMetadata
{
    public EndpointOperationMetadata(EndpointVerb verb, string route, string? summary, string? description, EndpointTagData[]? tags)
    {
        Verb = verb;
        Route = route ?? string.Empty;
        Summary = summary;
        Description = description;
        Tags = tags ?? [];
    }

    public EndpointVerb Verb { get; }

    public string Route { get; }

    public string? Summary { get; }

    public string? Description { get; }

    public EndpointTagData[] Tags { get; }
}