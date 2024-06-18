using System;

namespace PrimeFuncPack;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class EndpointAttribute : Attribute
{
    public EndpointAttribute(string operationId, EndpointVerb verb, string route)
    {
        OperationId = operationId ?? string.Empty;
        Verb = verb;
        Route = route ?? string.Empty;
    }

    public string OperationId { get; }

    public EndpointVerb Verb { get; }

    public string Route { get; }

    public string? Summary { get; set; }

    public string? Description { get; set; }
}