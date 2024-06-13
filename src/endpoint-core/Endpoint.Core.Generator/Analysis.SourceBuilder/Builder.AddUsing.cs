using System;
using System.Linq;

namespace PrimeFuncPack;

partial class SourceBuilder
{
    public SourceBuilder AddUsing(params string[] usings)
    {
        if (usings?.Length is not > 0)
        {
            return this;
        }

        foreach (var @using in usings)
        {
            _ = InnerAddUsing(@using);
        }

        return this;
    }

    private SourceBuilder InnerAddUsing(string @using)
    {
        if (string.IsNullOrWhiteSpace(@using))
        {
            return this;
        }

        if (string.Equals(@using, @namespace, StringComparison.InvariantCulture))
        {
            return this;
        }

        if (@namespace.StartsWith(@using + ".", StringComparison.InvariantCulture))
        {
            return this;
        }

        if (usings.Any(UsingEquals))
        {
            return this;
        }

        usings.Add(@using);
        return this;

        bool UsingEquals(string usingValue)
            =>
            string.Equals(usingValue, @using, StringComparison.InvariantCulture);
    }
}