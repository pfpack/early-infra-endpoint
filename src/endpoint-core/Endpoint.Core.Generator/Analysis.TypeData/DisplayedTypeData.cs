using System.Collections.Generic;

namespace PrimeFuncPack;

public sealed record class DisplayedTypeData
{
    public DisplayedTypeData(IReadOnlyCollection<string> allNamespaces, string displayedTypeName)
    {
        AllNamespaces = allNamespaces ?? [];
        DisplayedTypeName = displayedTypeName ?? string.Empty;
    }

    public IReadOnlyCollection<string> AllNamespaces { get; }

    public string DisplayedTypeName { get; }
}