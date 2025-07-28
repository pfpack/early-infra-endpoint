using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PrimeFuncPack;

partial class OperationSourceBuilder
{
    internal SourceBuilder AppendOperationMetadata(SourceBuilder builder)
        =>
        builder.AppendCodeLine(
            "new(")
        .BeginArguments()
        .AppendCodeLine(
            $"id: {operation.OperationId.AsStringSourceCodeOrStringEmpty()},",
            $"verb: {GetVerbSourceCode()},",
            $"route: {operation.Metadata.Route.AsStringSourceCodeOrStringEmpty()})")
        .BeginCodeBlock()
        .AppendOpenApiTags(
            operation)
        .AppendOpenApiOperation(
            operation)
        .AppendOpenApiComponents(
            operation)
        .EndCodeBlock(",")
        .EndArguments();

    private string GetVerbSourceCode()
        =>
        operation.Metadata.Verb switch
        {
            EndpointVerb.Get => "EndpointVerb.Get",
            EndpointVerb.Post => "EndpointVerb.Post",
            EndpointVerb.Put => "EndpointVerb.Put",
            EndpointVerb.Delete => "EndpointVerb.Delete",
            EndpointVerb.Options => "EndpointVerb.Options",
            EndpointVerb.Head => "EndpointVerb.Head",
            EndpointVerb.Patch => "EndpointVerb.Patch",
            EndpointVerb.Trace => "EndpointVerb.Trace",
            _ => "default"
        };
}

file static class OperationSourceBuilderExtensions
{
    internal static SourceBuilder AppendOpenApiOperation(this SourceBuilder builder, EndpointOperationData operation)
        =>
        builder.AppendCodeLine(
            "OpenApiOperation = new()")
        .BeginCodeBlock()
        .AppendCodeLine(
            $"Deprecated = {operation.GetDeprecatedCode()},",
            $"Summary = {operation.Metadata.Summary.AsStringSourceCodeOrStringEmpty()},",
            $"Description = {operation.Metadata.Description.AsStringSourceCodeOrStringEmpty()},")
        .AppendOpenApiRequest(
            operation)
        .AppendOpenApiResponses(
            operation)
        .EndCodeBlock(
            ",");

    internal static SourceBuilder AppendOpenApiComponents(this SourceBuilder builder, EndpointOperationData operation)
    {
        var responseLines = new List<string>();

        var requestMetadata = operation.Request.RequestMetadata;
        if (requestMetadata is not null)
        {
            builder = builder.AddUsing(
                requestMetadata.Type.AllNamespaces.ToArray());

            responseLines.Add(
                $"{requestMetadata.Type.DisplayedTypeName}.{requestMetadata.PropertyName}?.Components");
        }

        var responses = new List<EndpointOperationResponse>();
        if (operation.Request?.RequestMapper?.ResultUnion?.Responses?.Length > 0)
        {
            responses.AddRange(operation.Request.RequestMapper.ResultUnion?.Responses);
        }

        if (operation.Response?.ResponseMetadata is not null)
        {
            responses.Add(operation.Response);
        }
        else if (operation.Response?.Union?.Responses?.Length > 0)
        {
            responses.AddRange(operation.Response.Union.Responses);
        }

        foreach (var response in responses)
        {
            if (response.ResponseMetadata is null)
            {
                continue;
            }

            builder = builder.AddUsing(
                response.ResponseMetadata.Type.AllNamespaces.ToArray());

            responseLines.Add(
                $"{response.ResponseMetadata.Type.DisplayedTypeName}.{response.ResponseMetadata.PropertyName}?.Components");
        }

        if (responseLines.Count is 0)
        {
            return builder;
        }

        if (responseLines.Count is 1)
        {
            return builder.AppendCodeLine(
                $"OpenApiComponents = {responseLines[0]}");
        }

        builder = builder.AppendCodeLine(
            "OpenApiComponents = EndpointMetadataHelper.JoinComponents(")
        .BeginArguments();

        for (var i = 0; i < responseLines.Count; i++)
        {
            var line = new StringBuilder(responseLines[i]);
            line = (i == responseLines.Count - 1) ? line.Append(')') : line.Append(',');

            builder = builder.AppendCodeLine(line.ToString());
        }

        return builder.EndArguments();
    }

    internal static SourceBuilder AppendOpenApiTags(this SourceBuilder builder, EndpointOperationData operation)
    {
        var tags = operation.Metadata.Tags;
        if (tags.Length is 0)
        {
            return builder;
        }

        builder = builder.AppendCodeLine(
            "Tags =")
        .BeginCollectionExpression();

        for (var i = 0; i < tags.Length; i++)
        {
            var tag = tags[i];

            builder = builder.AppendCodeLine(
                "new()")
            .BeginCodeBlock()
            .AppendCodeLine(
                $"Name = {tag.Name.AsStringSourceCodeOrStringEmpty()},",
                $"Description = {tag.Description.AsStringSourceCodeOr("null")}")
            .EndCodeBlock(
                i == (tags.Length - 1) ? null : ",");
        }

        return builder.EndCollectionExpression(",");
    }

    private static SourceBuilder AppendOpenApiRequest(this SourceBuilder builder, EndpointOperationData operation)
    {
        var request = operation.Request?.RequestMetadata;
        if (request is null)
        {
            return builder;
        }

        return builder.AddUsing(
            request.Type.AllNamespaces.ToArray())
        .AppendCodeLine(
            $"Parameters = {request.Type.DisplayedTypeName}.{request.PropertyName}?.Parameters,",
            $"RequestBody = {request.Type.DisplayedTypeName}.{request.PropertyName}?.RequestBody,");
    }

    internal static SourceBuilder AppendOpenApiResponses(this SourceBuilder builder, EndpointOperationData operation)
    {
        var allResponses = new List<EndpointOperationResponse>();

        var validationResponses = operation.Request?.RequestMapper?.ResultUnion?.Responses;
        if (validationResponses?.Length > 0)
        {
            allResponses.AddRange(validationResponses);
        }

        if (operation.Response?.ResponseMetadata is not null)
        {
            allResponses.Add(operation.Response);
        }
        else
        {
            var responses = operation.Response?.Union?.Responses;
            if (responses?.Length > 0)
            {
                allResponses.AddRange(responses);
            }
        }

        return builder.AppendOpenApiResponses(allResponses.ToArray());
    }

    private static SourceBuilder AppendOpenApiResponses(
        this SourceBuilder builder, EndpointOperationResponse[] responses)
    {
        var responseLines = new List<string>(responses.Length);

        foreach (var response in responses)
        {
            var metadata = response.ResponseMetadata;
            if (metadata is null)
            {
                continue;
            }

            builder = builder.AddUsing(
                metadata.Type.AllNamespaces.ToArray());

            responseLines.Add(
                $"{metadata.Type.DisplayedTypeName}.{metadata.PropertyName}?.Responses");
        }

        if (responseLines.Count is 0)
        {
            return builder;
        }

        if (responseLines.Count is 1)
        {
            return builder.AppendCodeLine(
                $"Responses = {responseLines[0]}");
        }

        builder = builder.AppendCodeLine(
            "Responses = EndpointMetadataHelper.JoinResponses(")
        .BeginArguments();

        for (var i = 0; i < responseLines.Count; i++)
        {
            var line = new StringBuilder(responseLines[i]);
            line = (i == responseLines.Count - 1) ? line.Append(')') : line.Append(',');

            builder = builder.AppendCodeLine(line.ToString());
        }

        return builder.EndArguments();
    }

    private static string GetDeprecatedCode(this EndpointOperationData operation)
        =>
        operation.ObsoleteData is null ? "false" : "true";
}