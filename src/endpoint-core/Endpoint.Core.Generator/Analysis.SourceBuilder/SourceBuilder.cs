using System.Collections.Generic;
using System.Text;

namespace PrimeFuncPack;

public sealed partial class SourceBuilder
{
    private const int TabInterval = 4;

    private readonly List<string> usings = [];

    private readonly string @namespace;

    private readonly List<string> aliases = [];

    private readonly StringBuilder codeBuilder = new();

    private int tabulationLength = 0;

    public SourceBuilder(string? @namespace)
        =>
        this.@namespace = string.IsNullOrWhiteSpace(@namespace) ? "PrimeFuncPack" : @namespace!;

    private SourceBuilder InnerAppendLineWithTabulation(string codeLine)
    {
        if (tabulationLength is not > 0)
        {
            return this;
        }

        var tabulation = new string(' ', TabInterval * tabulationLength);
        _ = codeBuilder.AppendLine().Append(tabulation).Append(codeLine);

        return this;
    }
}