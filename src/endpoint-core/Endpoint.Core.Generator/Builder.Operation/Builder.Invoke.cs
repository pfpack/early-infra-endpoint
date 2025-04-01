using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PrimeFuncPack;

partial class OperationSourceBuilder
{
    internal SourceBuilder AppendOperationInvokeMethod(SourceBuilder builder)
        =>
        builder.AddUsing(
            "System.Threading",
            "System.Threading.Tasks",
            "PrimeFuncPack")
        .AppendCodeLine(
            $"private async Task<EndpointResponse> {operation.Method.InvokeMethodName}(EndpointRequest request, CancellationToken cancellationToken)")
        .BeginCodeBlock()
        .AppendInvokeMethodBody(
            operation)
        .EndCodeBlock();
}

file static class OperationSourceBuilderExtensions
{
    internal static SourceBuilder AppendInvokeMethodBody(this SourceBuilder builder, EndpointOperationData operation)
    {
        var requestMapper = operation.Request.RequestMapper;
        if (requestMapper is not null)
        {
            builder = builder.AddUsing(requestMapper.Type.AllNamespaces.ToArray());
        }

        var responseMapper = operation.Response.ResponseMapper;
        if (responseMapper is not null)
        {
            builder = builder.AddUsing(responseMapper.Type.AllNamespaces.ToArray());
        }

        if (requestMapper is null)
        {
            builder = builder.AddUsing(
                operation.Request.RequestType.AllNamespaces.ToArray())
            .AppendCodeLine(
                $"var input = default({operation.Request.RequestType.DisplayedTypeName});");
        }
        else
        {
            var requestMapperMethod = $"{requestMapper.Type.DisplayedTypeName}.{requestMapper.MethodName}";
            if (requestMapper.IsAsyncMethod)
            {
                builder = builder.AppendCodeLine(
                    $"var input = await {requestMapperMethod}(request, cancellationToken).ConfigureAwait(false);");
            }
            else
            {
                builder = builder.AppendCodeLine(
                    $"var input = {requestMapperMethod}(request);");
            }
        }

        var requestUnion = operation.Request.RequestMapper?.ResultUnion;
        if (requestUnion is null)
        {
            return builder.AppendApiCall(
                operation, "input", false)
            .AppendInnerResponseMappers(
                operation.Response.Union?.Responses ?? []);
        }

        var requestFoldParametersBuilder = new StringBuilder();
        for (var i = 0; i < requestUnion.Responses.Length; i++)
        {
            if (requestFoldParametersBuilder.Length > 0)
            {
                requestFoldParametersBuilder = requestFoldParametersBuilder.Append(", ");
            }

            var requestUnionResponse = requestUnion.Responses[i];
            if (requestUnionResponse.ResponseMapper is null)
            {
                requestFoldParametersBuilder = requestFoldParametersBuilder.Append("InnerInvokeAsync");
                continue;
            }

            requestFoldParametersBuilder = requestFoldParametersBuilder.Append(GetInnerMapResponseName(i));
        }

        var responses = new List<EndpointOperationResponse>(requestUnion.Responses);
        if (operation.Response.Union?.Responses?.Length > 0)
        {
            responses.AddRange(operation.Response.Union.Responses);
        }

        return builder.AppendCodeLine(
            $"return await input.{requestUnion.FoldMethodName}({requestFoldParametersBuilder}).ConfigureAwait(false);")
        .AppendEmptyLine()
        .AddUsing(
            operation.Request.RequestType.AllNamespaces.ToArray())
        .AppendCodeLine(
            $"async Task<EndpointResponse> InnerInvokeAsync({operation.Request.RequestType.DisplayedTypeName} @in)")
        .BeginCodeBlock()
        .AppendApiCall(
            operation, "@in", true)
        .EndCodeBlock()
        .AppendInnerResponseMappers(
            responses.ToArray());
    }

    private static SourceBuilder AppendApiCall(
        this SourceBuilder builder, EndpointOperationData operation, string inputName, bool isLocalMethod)
    {
        var apiMethod = $"{EndpointFieldNames.EndpointApi}.{operation.Method.ApiMethodName}";

        builder = builder.AppendCodeLine(
            $"var output = await {apiMethod}({inputName}, cancellationToken).ConfigureAwait(false);");

        if (isLocalMethod is false)
        {
            builder = builder.AppendEmptyLine();
        }

        var responseMapper = operation.Response.ResponseMapper;
        if (responseMapper is not null)
        {
            var responseMapperMethod = $"{responseMapper.Type.DisplayedTypeName}.{responseMapper.MethodName}";
            if (responseMapper.MethodType is EndpointOperationResponse.MapperMethodType.Default)
            {
                return builder.AppendCodeLine(
                    $"return {responseMapperMethod}(output);");
            }

            return builder.AppendCodeLine(
                $"return await {responseMapperMethod}(output, cancellationToken).ConfigureAwait(false);");
        }

        var responseUnion = operation.Response.Union;
        if (responseUnion is null)
        {
            return builder.AppendCodeLine(
                "return EndpointAbsentResponseProvider.GetResponse();");
        }

        var foldLineBuilder = new StringBuilder(
            $"return await output.{responseUnion.FoldMethodName}(");

        var requestUnionResponsesLength = operation.Request.RequestMapper?.ResultUnion?.Responses.Length ?? default;
        for (var i = 0; i < responseUnion.Responses.Length; i++)
        {
            if (i > 0)
            {
                foldLineBuilder = foldLineBuilder.Append(", ");
            }

            foldLineBuilder = foldLineBuilder.Append(GetInnerMapResponseName(requestUnionResponsesLength + i));
        }

        return builder.AppendCodeLine(
            foldLineBuilder.Append(").ConfigureAwait(false);").ToString());
    }

    private static SourceBuilder AppendInnerResponseMappers(this SourceBuilder builder, EndpointOperationResponse[] responses)
    {
        for (var i = 0; i < responses.Length; i++)
        {
            var unionResponse = responses[i];

            var responseType = unionResponse.ResponseType;
            var responseMapper = unionResponse.ResponseMapper;

            if (responseType is not null && responseMapper is null)
            {
                continue;
            }

            var methodLine = $"Task<EndpointResponse> {GetInnerMapResponseName(i)}(";
            if (responseType is not null)
            {
                builder = builder.AddUsing(responseType.AllNamespaces.ToArray());
                methodLine += $"{responseType.DisplayedTypeName} response";
            }

            if (responseMapper is null || responseMapper.MethodType is EndpointOperationResponse.MapperMethodType.Default)
            {
                methodLine = "static " + methodLine;
            }

            builder = builder.AppendEmptyLine()
            .AppendCodeLine(
                methodLine + ")")
            .BeginLambda();

            if (responseMapper is null)
            {
                builder = builder.AppendCodeLine(
                    "Task.FromResult(EndpointAbsentResponseProvider.GetResponse());");
            }
            else if (responseMapper.MethodType is EndpointOperationResponse.MapperMethodType.Default)
            {
                builder = builder.AppendCodeLine(
                    $"Task.FromResult({responseMapper.Type.DisplayedTypeName}.{responseMapper.MethodName}(response));");
            }
            else if (responseMapper.MethodType is EndpointOperationResponse.MapperMethodType.ValueTask)
            {
                builder = builder.AppendCodeLine(
                    $"{responseMapper.Type.DisplayedTypeName}.{responseMapper.MethodName}(response, cancellationToken).AsTask();");
            }
            else
            {
                builder = builder.AppendCodeLine(
                    $"{responseMapper.Type.DisplayedTypeName}.{responseMapper.MethodName}(response, cancellationToken);");
            }

            builder = builder.EndLambda();
        }

        return builder;
    }

    private static string GetInnerMapResponseName(int index)
        =>
        $"InnerMapResponse{index}Async";
}