namespace PrimeFuncPack;

internal sealed record class SourceFileData
{
    public SourceFileData(string fileName, string sourceCode)
    {
        FileName = fileName ?? string.Empty;
        SourceCode = sourceCode ?? string.Empty;
    }

    public string FileName { get; }

    public string SourceCode { get; }
}