namespace PrimeFuncPack;

internal sealed record class EndpointApiTypeData
{
    public EndpointApiTypeData(
        string @namespace,
        string typeName,
        bool isPublic,
        bool isReferenceType,
        ObsoleteData? obsoleteData)
    {
        Namespace = @namespace ?? string.Empty;
        TypeName = typeName ?? string.Empty;
        IsPublic = isPublic;
        IsReferenceType = isReferenceType;
        ObsoleteData = obsoleteData;
    }

    public string Namespace { get; }

    public string TypeName { get; }

    public bool IsPublic { get; }

    public bool IsReferenceType { get; }

    public ObsoleteData? ObsoleteData { get; }
}