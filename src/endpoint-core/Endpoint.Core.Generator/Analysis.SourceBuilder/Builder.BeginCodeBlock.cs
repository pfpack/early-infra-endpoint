namespace PrimeFuncPack;

partial class SourceBuilder
{
    public SourceBuilder BeginCodeBlock()
    {
        _ = InnerAppendLineWithTabulation("{");
        tabulationLength++;

        return this;
    }
}