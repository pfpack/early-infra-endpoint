using System;

namespace PrimeFuncPack;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class EndpointAttribute : Attribute
{
    public EndpointAttribute(EndpointVerb verb, string route)
    {
        Verb = verb;
        Route = route ?? string.Empty;
    }

    public EndpointVerb Verb { get; }

    public string Route { get; }

    public string? OperationId { get; set; }

    public string? Summary { get; set; }

    public string? Description { get; set; }
}