namespace PrimeFuncPack;

internal sealed record class EndpointTagData
{
    public EndpointTagData(string? name, string? description)
    {
        Name = name ?? string.Empty;
        Description = description;
    }

    public string Name { get; }

    public string? Description { get; }
}