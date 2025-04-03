using System.Collections.Generic;

namespace PrimeFuncPack;

partial class EndpointSourceBuilder
{
    private string BuildEndpointInvokeSource()
        =>
        new SourceBuilder(
            type.ApiType.Namespace)
        .AddUsing(
            "System",
            "System.Threading",
            "System.Threading.Tasks",
            "PrimeFuncPack")
        .AppendCodeLine(
            $"partial class {type.TypeName}")
        .BeginCodeBlock()
        .AppendInvokeMethod(
            operations)
        .AppendInnerInvokeMethods(
            operations)
        .EndCodeBlock()
        .Build();
}

file static class InvokeBuilderExtensions
{
    internal static SourceBuilder AppendInvokeMethod(this SourceBuilder builder, IReadOnlyList<OperationSourceBuilder> operations)
        =>
        builder.AppendCodeLine(
            "public Task<EndpointResponse> InvokeAsync(EndpointRequest request, CancellationToken cancellationToken = default)")
        .BeginCodeBlock()
        .AppendCodeLine(
            "ArgumentNullException.ThrowIfNull(request);")
        .AppendInnerInvokeCall(
            operations)
        .EndCodeBlock();

    internal static SourceBuilder AppendInnerInvokeMethods(this SourceBuilder builder, IReadOnlyList<OperationSourceBuilder> operations)
    {
        foreach (var operation in operations)
        {
            builder = builder.AppendEmptyLine();
            builder = operation.AppendOperationInvokeMethod(builder);
        }

        return builder;
    }

    private static SourceBuilder AppendInnerInvokeCall(this SourceBuilder builder, IReadOnlyList<OperationSourceBuilder> operations)
    {
        if (operations.Count is 1)
        {
            return builder.AppendCodeLine($"return {operations[0].InvokeMethodName}(request, cancellationToken);");
        }

        builder = builder
            .AppendEmptyLine()
            .AppendCodeLine("return request.OperationId switch")
            .BeginCodeBlock();

        foreach (var operation in operations)
        {
            builder = builder.AppendCodeLine(
                $"{operation.OperationId.AsStringSourceCodeOr()} => {operation.InvokeMethodName}(request, cancellationToken),");
        }

        return builder
            .AppendCodeLine("_ => throw new InvalidOperationException($\"Endpoint OperationId '{request.OperationId}' is unknown.\")")
            .EndCodeBlock(";");
    }
}