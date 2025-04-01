using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;

namespace PrimeFuncPack;

public static class EndpointApplicationBuilder
{
    private const string EnumTextFormat = "F";

    public static TApplicationBuilder UseEndpoint<TApplicationBuilder, TEndpoint>(
        this TApplicationBuilder app, Func<IServiceProvider, TEndpoint> endpointResolver)
        where TApplicationBuilder : IApplicationBuilder
        where TEndpoint : IEndpointMetadataProvider, IEndpointInvokeSupplier
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentNullException.ThrowIfNull(endpointResolver);

        var metadata = TEndpoint.Metadata;
        if (metadata?.Operations?.Count is not > 0)
        {
            return app;
        }

        var routeBuilder = new RouteBuilder(app);
        foreach (var operation in metadata.Operations)
        {
            var verb = operation.Verb.ToString(EnumTextFormat).ToUpperInvariant();
            _ = routeBuilder.MapVerb(verb, operation.Route, InnerInvokeAsync);

            Task InnerInvokeAsync(HttpContext context)
                =>
                InvokeAsync(context, endpointResolver.Invoke(context.RequestServices), operation.Id);
        }

        _ = app.UseRouter(routeBuilder.Build());

        if (app is ISwaggerBuilder swaggerBuilder)
        {
            _ = swaggerBuilder.Use(EndpointSwaggerConfigurator.Configure<TEndpoint>);
        }

        return app;
    }

    private static async Task InvokeAsync(HttpContext context, IEndpointInvokeSupplier endpoint, string operationId)
    {
        var request = new EndpointRequest(
            operationId: operationId,
            headers: context.Request.Headers?.SelectMany(GetValues).ToArray(),
            queryParameters: context.Request.Query?.SelectMany(GetValues).ToArray(),
            routeValues: context.Request.RouteValues?.Select(MapRouteValue).ToArray(),
            userClaims: context.User?.Claims?.Select(MapClaimValue).ToArray(),
            body: context.Request.Body);

        var response = await endpoint.InvokeAsync(request, context.RequestAborted);
        await context.WriteResponseAsync(response);

        static IEnumerable<KeyValuePair<string, string?>> GetValues(KeyValuePair<string, StringValues> pair)
        {
            foreach (var value in pair.Value)
            {
                yield return new(pair.Key, value);
            }
        }

        static KeyValuePair<string, string?> MapRouteValue(KeyValuePair<string, object?> pair)
            =>
            new(
                pair.Key, pair.Value?.ToString());

        static KeyValuePair<string, string?> MapClaimValue(Claim claim)
            =>
            new(claim.Type, claim.Value);
    }

    private static Task WriteResponseAsync(this HttpContext context, EndpointResponse response)
    {
        var httpResponse = context.Response;
        httpResponse.StatusCode = response.StatusCode;

        foreach (var header in response.Headers)
        {
            if (string.IsNullOrEmpty(header.Value))
            {
                continue;
            }

            httpResponse.AddHeader(header.Key, header.Value);
        }

        if ((response.Body is null) || (response.StatusCode is StatusCodes.Status204NoContent))
        {
            return Task.CompletedTask;
        }

        return httpResponse.WriteBodyAsync(response.Body, context.RequestAborted);
    }

    private static void AddHeader(this HttpResponse httpResponse, string name, string value)
    {
        if (httpResponse.Headers.ContainsKey(name) is false)
        {
            httpResponse.Headers.Append(name, value);
            return;
        }

        var headerValue = httpResponse.Headers[name];
        httpResponse.Headers[name] = StringValues.Concat(headerValue, value);
    }

    private static async Task WriteBodyAsync(this HttpResponse httpResponse, Stream body, CancellationToken cancellationToken)
    {
        var buffer = new Memory<byte>(new byte[body.Length]);

        _ = await body.ReadAsync(buffer, cancellationToken);
        _ = await httpResponse.BodyWriter.WriteAsync(buffer, cancellationToken);
    }
}