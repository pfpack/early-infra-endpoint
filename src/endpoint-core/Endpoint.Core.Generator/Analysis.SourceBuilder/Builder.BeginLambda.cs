namespace PrimeFuncPack;

partial class SourceBuilder
{
    public SourceBuilder BeginLambda()
    {
        tabulationLength++;
        return InnerAppendLineWithTabulation("=>");
    }
}