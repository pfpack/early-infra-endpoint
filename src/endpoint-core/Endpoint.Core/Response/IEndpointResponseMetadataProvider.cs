namespace PrimeFuncPack;

public interface IEndpointResponseMetadataProvider
{
    static abstract EndpointResponseMetadata ResponseMetadata { get; }
}