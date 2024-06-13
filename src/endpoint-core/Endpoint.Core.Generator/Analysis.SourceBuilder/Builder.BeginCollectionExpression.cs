namespace PrimeFuncPack;

partial class SourceBuilder
{
    public SourceBuilder BeginCollectionExpression()
    {
        _ = InnerAppendLineWithTabulation("[");
        tabulationLength++;

        return this;
    }
}