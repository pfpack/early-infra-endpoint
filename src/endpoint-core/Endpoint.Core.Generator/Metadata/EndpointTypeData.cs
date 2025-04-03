namespace PrimeFuncPack;

internal sealed record class EndpointTypeData
{
    public EndpointTypeData(string typeName, EndpointApiTypeData apiType)
    {
        TypeName = typeName ?? string.Empty;
        ApiType = apiType;
    }

    public string TypeName { get; }

    public EndpointApiTypeData ApiType { get; }
}