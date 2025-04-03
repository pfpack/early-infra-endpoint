using System;

namespace PrimeFuncPack;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct | AttributeTargets.Method, AllowMultiple = true)]
public sealed class EndpointTagAttribute : Attribute
{
    public EndpointTagAttribute(string name)
        =>
        Name = name ?? string.Empty;

    public string Name { get; }

    public string? Description { get; set; }
}