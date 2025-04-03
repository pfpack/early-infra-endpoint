using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace PrimeFuncPack;

[Generator(LanguageNames.CSharp)]
internal sealed class EndpointExtensionsSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var provider = context.GetEndpointExtensionsValueProvider();
        context.RegisterSourceOutput(provider, GenerateSource);
    }

    private static void GenerateSource(SourceProductionContext context, EndpointExtensionsMetadata extension)
    {
        var constructorSourceCode = extension.BuildConstructorSourceCode();
        context.AddSource($"{extension.TypeName}.g.cs", SourceText.From(constructorSourceCode, Encoding.UTF8));

        foreach (var resolverMethodName in extension.ResolverMethodNames)
        {
            var endpointSourceCode = extension.BuildEndpointSourceCode(resolverMethodName);
            context.AddSource($"{extension.TypeName}.{resolverMethodName}.g.cs", SourceText.From(endpointSourceCode, Encoding.UTF8));
        }
    }
}