namespace PrimeFuncPack;

public static class EndpointEmptyResponseMetadataProvider
{
    public static EndpointResponseMetadata ResponseMetadata { get; }
        =
        new()
        {
            Responses = new()
            {
                [EndpointAbsentResponseProvider.StatusCode.ToString()] = new()
                {
                    Description = "NoContent"
                }
            }
        };
}