using System.Collections.Generic;

namespace PrimeFuncPack;

partial class EndpointSourceBuilder
{
    private string BuildEndpointMetadataSource()
        =>
        new SourceBuilder(
            type.ApiType.Namespace)
        .AddUsing(
            "PrimeFuncPack")
        .AppendCodeLine(
            $"partial class {type.TypeName}")
        .BeginCodeBlock()
        .AppendMetadataProperty(
            operations)
        .EndCodeBlock()
        .Build();
}

file static class MetadataBuilderExtensions
{
    internal static SourceBuilder AppendMetadataProperty(this SourceBuilder builder, IReadOnlyList<OperationSourceBuilder> operations)
        =>
        builder.AppendCodeLine(
            "public static EndpointMetadata Metadata { get; }")
        .BeginArguments()
        .AppendCodeLine(
            "=",
            "new(")
        .BeginArguments()
        .AppendCodeLine(
            "operations:")
        .BeginCollectionExpression()
        .AppendOperationsMetadata(
            operations)
        .EndCollectionExpression(");")
        .EndArguments()
        .EndArguments();

    private static SourceBuilder AppendOperationsMetadata(this SourceBuilder builder, IReadOnlyList<OperationSourceBuilder> operations)
    {
        foreach (var operation in operations)
        {
            builder = operation.AppendOperationMetadata(builder);
        }

        return builder;
    }
}