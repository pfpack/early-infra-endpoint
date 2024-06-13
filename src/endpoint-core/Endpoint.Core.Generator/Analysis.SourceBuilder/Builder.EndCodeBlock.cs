namespace PrimeFuncPack;

partial class SourceBuilder
{
    public SourceBuilder EndCodeBlock(string? finalSymbol = default)
    {
        tabulationLength--;
        _ = InnerAppendLineWithTabulation("}");

        if (string.IsNullOrWhiteSpace(finalSymbol) is false)
        {
            _ = codeBuilder.Append(finalSymbol);
        }

        return this;
    }
}