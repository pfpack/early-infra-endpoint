namespace PrimeFuncPack;

public interface IEndpointRequestMetadataProvider
{
    static abstract EndpointRequestMetadata RequestMetadata { get; }
}