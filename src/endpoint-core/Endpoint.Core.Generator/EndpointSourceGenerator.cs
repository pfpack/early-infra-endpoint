using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace PrimeFuncPack;

[Generator(LanguageNames.CSharp)]
internal sealed class EndpointSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var provider = context.GetEndpointBuildersProvider();
        context.RegisterSourceOutput(provider, GenerateSource);
    }

    private static void GenerateSource(SourceProductionContext context, EndpointSourceBuilder builder)
    {
        foreach (var sourceFile in builder.BuildSourceFiles())
        {
            context.AddSource(sourceFile.FileName, SourceText.From(sourceFile.SourceCode, Encoding.UTF8));
        }
    }
}