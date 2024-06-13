namespace PrimeFuncPack;

public static partial class CodeAnalysisExtensions
{
    private const string SystemNamespace = "System";

    private const string SystemTextJsonSerializationNamespace = "System.Text.Json.Serialization";

    private static string InnerWithCamelCase(this string source)
    {
        if (string.IsNullOrEmpty(source))
        {
            return string.Empty;
        }

        if (source.Length is 1)
        {
            return source.ToLowerInvariant();
        }

        return source[0].ToString().ToLowerInvariant() + source.Substring(1);
    }
}