namespace PrimeFuncPack;

internal sealed record class EndpointOperationResponse
{
    public EndpointOperationResponse(
        DisplayedTypeData? responseType,
        Mapper? responseMapper = null,
        MetadataProvider? responseMetadata = null,
        EndpointDisciminatedUnionResponse? union = null)
    {
        ResponseType = responseType;
        ResponseMapper = responseMapper;
        ResponseMetadata = responseMetadata;
        Union = union;
    }

    public DisplayedTypeData? ResponseType { get; }

    public Mapper? ResponseMapper { get; }

    public MetadataProvider? ResponseMetadata { get; }

    public EndpointDisciminatedUnionResponse? Union { get; }

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
        public Mapper(DisplayedTypeData type, string methodName, MapperMethodType methodType)
        {
            Type = type;
            MethodName = methodName ?? string.Empty;
            MethodType = methodType;
        }

        public DisplayedTypeData Type { get; }

        public string MethodName { get; }

        public MapperMethodType MethodType { get; }
    }

    internal enum MapperMethodType
    {
        Default,

        Task,

        ValueTask
    }
}