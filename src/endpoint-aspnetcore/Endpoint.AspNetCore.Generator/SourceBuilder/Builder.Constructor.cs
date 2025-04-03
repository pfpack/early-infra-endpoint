namespace PrimeFuncPack;

partial class EndpointExtensionsSourceBuilder
{
    internal static string BuildConstructorSourceCode(this EndpointExtensionsMetadata metadata)
        =>
        new SourceBuilder(
            metadata.Namespace)
        .AppendCodeLine(
            $"internal static partial class {metadata.TypeName}")
        .BeginCodeBlock()
        .EndCodeBlock()
        .Build();
}