using System;

namespace PrimeFuncPack;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct, AllowMultiple = false)]
public sealed class EndpointRequestMapperAttribute : Attribute
{
    public EndpointRequestMapperAttribute(Type mapperType, string methodName)
    {
        MapperType = mapperType;
        MethodName = methodName ?? string.Empty;
    }

    public Type MapperType { get; }

    public string MethodName { get; }
}