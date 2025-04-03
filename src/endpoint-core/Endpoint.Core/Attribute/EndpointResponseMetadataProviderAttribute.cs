using System;

namespace PrimeFuncPack;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct, AllowMultiple = false)]
public sealed class EndpointResponseMetadataProviderAttribute : Attribute
{
    public EndpointResponseMetadataProviderAttribute(Type providerType, string propertyName)
    {
        ProviderType = providerType;
        PropertyName = propertyName ?? string.Empty;
    }

    public Type ProviderType { get; }

    public string PropertyName { get; }
}