using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace PrimeFuncPack;

public sealed class EndpointRequest
{
    public EndpointRequest(
        [AllowNull] string operationName,
        [AllowNull] IReadOnlyCollection<KeyValuePair<string, string?>> headers = null,
        [AllowNull] IReadOnlyCollection<KeyValuePair<string, string?>> queryParameters = null,
        [AllowNull] IReadOnlyCollection<KeyValuePair<string, string?>> routeValues = null,
        [AllowNull] IReadOnlyCollection<KeyValuePair<string, string?>> userClaims = null,
        Stream? body = null)
    {
        OperationName = operationName ?? string.Empty;
        Headers = headers ?? [];
        QueryParameters = queryParameters ?? [];
        RouteValues = routeValues ?? [];
        UserClaims = userClaims ?? [];
        Body = body;
    }

    public string OperationName { get; }

    public IReadOnlyCollection<KeyValuePair<string, string?>> Headers { get; }

    public IReadOnlyCollection<KeyValuePair<string, string?>> QueryParameters { get; }

    public IReadOnlyCollection<KeyValuePair<string, string?>> RouteValues { get; }

    public IReadOnlyCollection<KeyValuePair<string, string?>> UserClaims { get; }

    public Stream? Body { get; }
}