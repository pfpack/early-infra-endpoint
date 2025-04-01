namespace PrimeFuncPack;

internal static partial class EndpointSourceBuilderExtensions
{

    private static string AsStringValueOrDefault(this string? source)
        =>
        source.AsStringSourceCodeOr("default");
}