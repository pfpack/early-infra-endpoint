namespace PrimeFuncPack;

partial class SourceBuilder
{
    public SourceBuilder AppendCodeLine(params string[] codeLines)
    {
        if (codeLines?.Length is not > 0)
        {
            return this;
        }

        var builder = this;

        foreach (var line in codeLines)
        {
            builder = InnerAppendLineWithTabulation(line);
        }

        return builder;
    }
}