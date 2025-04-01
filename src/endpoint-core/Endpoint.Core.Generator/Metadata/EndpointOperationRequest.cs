namespace PrimeFuncPack;

internal sealed record class EndpointOperationRequest
{
    public EndpointOperationRequest(DisplayedTypeData requestType, Mapper? requestMapper, MetadataProvider? requestMetadata)
    {
        RequestType = requestType;
        RequestMapper = requestMapper;
        RequestMetadata = requestMetadata;
    }

    public DisplayedTypeData RequestType { get; }

    public Mapper? RequestMapper { get; }

    public MetadataProvider? RequestMetadata { get; }

    internal sealed record class MetadataProvider
    {
        public MetadataProvider(DisplayedTypeData type, string propertyName)
        {
            Type = type;
            PropertyName = propertyName ?? string.Empty;
        }

        public DisplayedTypeData Type { get; }

        public string PropertyName { get; }
    }

    internal sealed record class Mapper
    {
        public Mapper(DisplayedTypeData type, string methodName, bool isAsyncMethod, EndpointDisciminatedUnionResponse? resultUnion)
        {
            Type = type;
            MethodName = methodName ?? string.Empty;
            IsAsyncMethod = isAsyncMethod;
            ResultUnion = resultUnion;
        }

        public DisplayedTypeData Type { get; }

        public string MethodName { get; }

        public bool IsAsyncMethod { get; }

        public EndpointDisciminatedUnionResponse? ResultUnion { get; }
    }
}