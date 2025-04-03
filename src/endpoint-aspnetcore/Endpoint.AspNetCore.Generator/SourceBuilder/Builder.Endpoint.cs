using System.Linq;

namespace PrimeFuncPack;

partial class EndpointExtensionsSourceBuilder
{
    internal static string BuildEndpointSourceCode(this EndpointExtensionsMetadata metadata, string resolverMethodName)
        =>
        new SourceBuilder(
            metadata.Namespace)
        .AddUsing(
            "Microsoft.AspNetCore.Builder",
            "PrimeFuncPack")
        .AddUsing(
            metadata.ProviderType.AllNamespaces.ToArray())
        .AppendCodeLine(
            $"partial class {metadata.TypeName}")
        .BeginCodeBlock()
        .AppendCodeLine(
            $"internal static TBuilder {resolverMethodName}<TBuilder>(this TBuilder builder) where TBuilder : IApplicationBuilder")
        .BeginLambda()
        .AppendCodeLine(
            $"builder.UseEndpoint({metadata.ProviderType.DisplayedTypeName}.{resolverMethodName}().Resolve);")
        .EndLambda()
        .EndCodeBlock()
        .Build();
}