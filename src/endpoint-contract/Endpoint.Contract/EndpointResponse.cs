﻿using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace PrimeFuncPack;

public sealed class EndpointResponse
{
    public EndpointResponse(
        int statusCode,
        [AllowNull] IReadOnlyCollection<KeyValuePair<string, string?>> headers = null,
        [AllowNull] Stream body = null)
    {
        StatusCode = statusCode;
        Headers = headers ?? [];
        Body = body ?? Stream.Null;
    }

    public int StatusCode { get; }

    public IReadOnlyCollection<KeyValuePair<string, string?>> Headers { get; }

    public Stream Body { get; }
}