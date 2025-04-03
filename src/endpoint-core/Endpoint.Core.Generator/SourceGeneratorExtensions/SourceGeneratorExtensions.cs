using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace PrimeFuncPack;

internal static partial class SourceGeneratorExtensions
{
    private const string SystemNamespace = "System";

    private const string PrimeFuncPackNamespace = "PrimeFuncPack";

    private const string AsyncSuffix = "Async";

    private const string InnerPrefix = "Inner";

    private static readonly IReadOnlyList<string> DefaultEndpointNameEnds
        =
        [
            "HttpApi", "Api", "HttpFunc", "Func"
        ];

    private static readonly EndpointOperationResponse.MetadataProvider EmptyResponseMetadataProvider
        =
        new(
            type: new([PrimeFuncPackNamespace], "EndpointEmptyResponseMetadataProvider"),
            propertyName: "ResponseMetadata");

    private static IEnumerable<T> FilterNotNull<T>(this IEnumerable<T?> source)
    {
        foreach (var item in source)
        {
            if (item is not null)
            {
                yield return item;
            }
        }
    }

    private static bool IsEmptyStructure(this ITypeSymbol type)
    {
        return type.IsValueType && type.GetMembers().OfType<IFieldSymbol>().Any(IsNotStatic) is false;

        static bool IsNotStatic(IFieldSymbol field)
            =>
            field.IsStatic is false;
    }

    private static string GetEndpointTypeRootName(this ITypeSymbol endpointType)
    {
        var name = endpointType.Name;
        if (name.Length is not > 1)
        {
            return name;
        }

        if (endpointType.TypeKind is TypeKind.Interface && name.StartsWith("I", StringComparison.InvariantCultureIgnoreCase))
        {
            name = name.Substring(1);
        }

        foreach (var end in DefaultEndpointNameEnds)
        {
            var cutName = name.CutOffAtEnd(end);
            if (name.Length != cutName.Length)
            {
                return cutName;
            }
        }

        return name;
    }

    private static string CutOffAtEnd(this string source, string end)
        =>
        source.EndsWith(end, StringComparison.InvariantCultureIgnoreCase) ? source.Substring(0, source.Length - end.Length) : source;

    private static ITypeSymbol? GetGenericArgumentType(this ITypeSymbol type)
        =>
        (type is INamedTypeSymbol namedType && namedType.TypeArguments.Length is 1) ? namedType.TypeArguments[0] : null;

    private static bool IsGenericTask(this ITypeSymbol type)
        =>
        type.IsAnyType("System.Threading.Tasks", "Task");

    private static bool IsGenericValueTask(this ITypeSymbol type)
        =>
        type.IsAnyType("System.Threading.Tasks", "ValueTask");

    private static IEnumerable<EndpointTagData> GetTags(this ISymbol symbol)
    {
        foreach (var attribute in symbol.GetAttributes().Where(IsEndpointTagAttribute))
        {
            var name = attribute.GetConstructorArgumentValue<string>(0);
            if (string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            yield return new(
                name: name,
                description: attribute.GetNamedArgumentValue<string>("Description"));
        }

        static bool IsEndpointTagAttribute(AttributeData attributeData)
            =>
            attributeData.AttributeClass?.IsType(PrimeFuncPackNamespace, "EndpointTagAttribute") is true;
    }

    private static ObsoleteData? GetObsoleteData(this ISymbol symbol)
    {
        var obsoleteAttributeData = symbol.GetAttributes().FirstOrDefault(IsObsoleteAttribute);
        if (obsoleteAttributeData is null)
        {
            return null;
        }

        return new(
            message: obsoleteAttributeData.GetConstructorArgumentValue<string>(0),
            isError: obsoleteAttributeData.GetConstructorArgumentValue<bool?>(1),
            diagnosticId: obsoleteAttributeData.GetNamedArgumentValue<string>("DiagnosticId"),
            urlFormat: obsoleteAttributeData.GetNamedArgumentValue<string>("UrlFormat"));

        static bool IsObsoleteAttribute(AttributeData attributeData)
            =>
            attributeData.AttributeClass?.IsType(SystemNamespace, "ObsoleteAttribute") is true;
    }
}