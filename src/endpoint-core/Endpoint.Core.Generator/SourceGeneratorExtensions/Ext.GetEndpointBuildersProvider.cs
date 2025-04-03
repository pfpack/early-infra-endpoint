using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PrimeFuncPack;

partial class SourceGeneratorExtensions
{
    internal static IncrementalValuesProvider<EndpointSourceBuilder> GetEndpointBuildersProvider(
        this IncrementalGeneratorInitializationContext context)
    {
        return context.SyntaxProvider.CreateSyntaxProvider(IsActualType, GetEndpointTypeBuilder).Where(IsNotNull).Select(AsNotNullable);

        static bool IsActualType(SyntaxNode syntaxNode, CancellationToken _)
            =>
            syntaxNode is InterfaceDeclarationSyntax or ClassDeclarationSyntax or StructDeclarationSyntax;

        static bool IsNotNull(EndpointSourceBuilder? metadata)
            =>
            metadata is not null;

        static EndpointSourceBuilder AsNotNullable(EndpointSourceBuilder? metadata, CancellationToken _)
            =>
            metadata!;
    }

    private static EndpointSourceBuilder? GetEndpointTypeBuilder(GeneratorSyntaxContext context, CancellationToken _)
    {
        if (context.SemanticModel.GetDeclaredSymbol((TypeDeclarationSyntax)context.Node) is not INamedTypeSymbol typeSymbol)
        {
            return null;
        }

        var existedOperationIds = new Dictionary<string, int>();
        var existedMethodNames = new Dictionary<string, int>();
        var typeTags = typeSymbol.GetTags().ToArray();

        var operations = typeSymbol.GetMembers().OfType<IMethodSymbol>().Select(InnerGetOperationSourceBuilder).FilterNotNull().ToArray();
        if (operations.Length is 0)
        {
            return null;
        }

        if (typeSymbol.IsStatic)
        {
            throw new InvalidOperationException($"Static type '{typeSymbol.Name}' can not be endpoint.");
        }

        if (typeSymbol.TypeParameters.Length > 0)
        {
            throw new InvalidOperationException($"Generic type '{typeSymbol.Name}' can not be endpoint.");
        }

        return new(
            type: new(
                typeName: $"{typeSymbol.GetEndpointTypeRootName()}Endpoint",
                apiType: new(
                    @namespace: typeSymbol.ContainingNamespace.ToString(),
                    typeName: typeSymbol.Name,
                    isPublic: typeSymbol.DeclaredAccessibility is Accessibility.Public,
                    isReferenceType: typeSymbol.IsReferenceType,
                    obsoleteData: typeSymbol.GetObsoleteData())),
            operations: operations);

        OperationSourceBuilder? InnerGetOperationSourceBuilder(IMethodSymbol method)
            =>
            method.GetOperationSourceBuilder(typeTags, existedOperationIds, existedMethodNames);
    }

    private static OperationSourceBuilder? GetOperationSourceBuilder(
        this IMethodSymbol method,
        EndpointTagData[] typeTags,
        Dictionary<string, int> existedOperationIds,
        Dictionary<string, int> existedMethodNames)
    {
        if (method.GetAttributes().FirstOrDefault(IsEndpointAttribute) is not AttributeData endpointAttribute)
        {
            return null;
        }

        var apiMethodName = method.Name;
        var rootMethodName = apiMethodName.CutOffAtEnd(AsyncSuffix);

        var operationId = endpointAttribute.GetNamedArgumentValue<string>("OperationId") ?? string.Empty;
        if (string.IsNullOrWhiteSpace(operationId))
        {
            operationId = rootMethodName;
        }

        if (existedOperationIds.TryGetValue(operationId, out var operationIdIndex))
        {
            operationIdIndex++;

            existedOperationIds[operationId] = operationIdIndex;
            operationId += operationIdIndex.ToString();
        }
        else
        {
            existedOperationIds.Add(operationId, 0);
        }

        var invokeMethodName = string.Concat(InnerPrefix, rootMethodName, AsyncSuffix);
        if (existedMethodNames.TryGetValue(invokeMethodName, out var methodIndex))
        {
            methodIndex++;

            existedMethodNames[invokeMethodName] = methodIndex;
            invokeMethodName = string.Concat(InnerPrefix, rootMethodName, methodIndex.ToString(), AsyncSuffix);
        }
        else
        {
            existedMethodNames.Add(invokeMethodName, 0);
        }

        return new(
            operation: new(
                operationId: operationId,
                metadata: new(
                    verb: endpointAttribute.GetConstructorArgumentValue<EndpointVerb>(0),
                    route: endpointAttribute.GetConstructorArgumentValue<string>(1) ?? string.Empty,
                    summary: endpointAttribute.GetNamedArgumentValue<string>("Summary"),
                    description: endpointAttribute.GetNamedArgumentValue<string>("Description"),
                    tags: typeTags.Concat(method.GetTags()).ToArray()),
                method: new(
                    apiMethodName: apiMethodName,
                    invokeMethodName: invokeMethodName),
                request: method.GetOperationRequest(),
                response: method.GetOperationResponse())
            {
                ObsoleteData = method.GetObsoleteData()
            });

        static bool IsEndpointAttribute(AttributeData attributeData)
            =>
            attributeData.AttributeClass?.IsType(PrimeFuncPackNamespace, "EndpointAttribute") is true;
    }

    private static EndpointOperationRequest GetOperationRequest(this IMethodSymbol apiMethod)
    {
        var inputType = apiMethod.Parameters.FirstOrDefault()?.Type ?? throw CreateRequestTypeMustBeSpecifiedException(apiMethod);

        return new(
            requestType: inputType.GetDisplayedData(),
            requestMapper: inputType.GetOperationRequestMapper(),
            requestMetadata: inputType.GetRequestMetadataProvider());

        static InvalidOperationException CreateRequestTypeMustBeSpecifiedException(IMethodSymbol method)
            =>
            new($"Request type must be specified for method '{method.Name}'");
    }

    private static EndpointOperationRequest.Mapper? GetOperationRequestMapper(this ITypeSymbol inputType)
    {
        var mapperAttribute = inputType.GetAttributes().FirstOrDefault(IsMapperAttribute);
        var mapperType = mapperAttribute?.GetConstructorArgumentValue<INamedTypeSymbol>(0);

        if (mapperAttribute is null || mapperType is null)
        {
            if (inputType.IsEmptyStructure())
            {
                return null;
            }

            throw CreateMapperNotSpecifiedException(inputType);
        }

        var mapperMethodName = mapperAttribute.GetConstructorArgumentValue<string>(1);
        if (string.IsNullOrWhiteSpace(mapperMethodName))
        {
            throw CreateMapperMethodNotSpecifiedException(inputType);
        }

        foreach (var method in mapperType.GetMembers(mapperMethodName!).OfType<IMethodSymbol>())
        {
            if ((method.IsStatic is false) || (method.DeclaredAccessibility is not (Accessibility.Public or Accessibility.Internal)))
            {
                continue;
            }

            if (method.Parameters.FirstOrDefault()?.Type?.IsType(PrimeFuncPackNamespace, "EndpointRequest") is not true)
            {
                continue;
            }

            var returnType = method.ReturnType;
            if (returnType is null)
            {
                continue;
            }

            var taskType = returnType.GetGenericArgumentType();
            if ((taskType is null) && (returnType.IsGenericTask() is false) && (returnType.IsGenericValueTask() is false))
            {
                if (method.Parameters.Length is not 1)
                {
                    continue;
                }

                var mapperSync = returnType.GetOperationRequestMapper(inputType, method.Name, false);
                if (mapperSync is not null)
                {
                    return mapperSync;
                }

                continue;
            }

            if ((method.Parameters.Length is not 2) || (method.Parameters[1].Type?.IsType("System.Threading", "CancellationToken") is not true))
            {
                continue;
            }

            var mapperAsync = taskType?.GetOperationRequestMapper(inputType, method.Name, true);
            if (mapperAsync is not null)
            {
                return mapperAsync;
            }
        }

        throw CreateMapperMethodNotFoundException(mapperMethodName, mapperType);

        static bool IsMapperAttribute(AttributeData attributeData)
            =>
            attributeData.AttributeClass?.IsType(PrimeFuncPackNamespace, "EndpointRequestMapperAttribute") is true;

        static InvalidOperationException CreateMapperNotSpecifiedException(ITypeSymbol type)
            =>
            new($"Request mapper must be specified for type '{type.ToDisplayString()}'.");

        static InvalidOperationException CreateMapperMethodNotSpecifiedException(ITypeSymbol type)
            =>
            new($"Request mapper method must be specified for type '{type.ToDisplayString()}'.");

        static InvalidOperationException CreateMapperMethodNotFoundException(string? methodName, ITypeSymbol type)
            =>
            new($"Request mapper method '{methodName}' is not found in the type '{type.ToDisplayString()}'.");
    }

    private static EndpointOperationRequest.Mapper? GetOperationRequestMapper(
        this ITypeSymbol returnType, ITypeSymbol requestType, string methodName, bool isAsyncMethod)
    {
        if (returnType.Equals(requestType, SymbolEqualityComparer.Default))
        {
            return new(requestType.GetDisplayedData(), methodName, isAsyncMethod, null);
        }

        var union = returnType.GetDisciminatedUnion(requestType);
        if (union is null)
        {
            return null;
        }

        return new(requestType.GetDisplayedData(), methodName, isAsyncMethod, union);
    }

    private static EndpointOperationRequest.MetadataProvider? GetRequestMetadataProvider(this ITypeSymbol inputType)
    {
        var providerAttribute = inputType.GetAttributes().FirstOrDefault(IsMetadataProviderAttribute);
        var type = providerAttribute?.GetConstructorArgumentValue<INamedTypeSymbol>(0);

        if (providerAttribute is null || type is null)
        {
            return null;
        }

        var propertyName = providerAttribute.GetConstructorArgumentValue<string>(1);
        if (string.IsNullOrWhiteSpace(propertyName))
        {
            throw CreatePropertyNotSpecifiedException(inputType);
        }

        var property = type.GetMembers(propertyName!).OfType<IPropertySymbol>().FirstOrDefault() ?? throw CreatePropertyNotFoundException();
        if (property.Type?.IsType(PrimeFuncPackNamespace, "EndpointRequestMetadata") is not true)
        {
            throw CreatePropertyInvalidTypeException("EndpointRequestMetadata");
        }

        if (property.IsStatic is false)
        {
            throw CreatePropertyNotStaticException();
        }

        if (property.DeclaredAccessibility is not (Accessibility.Public or Accessibility.Internal))
        {
            throw CreatePropertyNotPublicException();
        }

        return new(type.GetDisplayedData(), propertyName!);

        static bool IsMetadataProviderAttribute(AttributeData attributeData)
            =>
            attributeData.AttributeClass?.IsType(PrimeFuncPackNamespace, "EndpointRequestMetadataProviderAttribute") is true;

        static InvalidOperationException CreatePropertyNotSpecifiedException(ITypeSymbol type)
            =>
            new($"Request metadata provider property must be specified for type '{type.ToDisplayString()}'.");

        InvalidOperationException CreatePropertyNotFoundException()
            =>
            new($"Request metadata provider property '{propertyName}' is not found in the type '{type.ToDisplayString()}'.");

        InvalidOperationException CreatePropertyNotStaticException()
            =>
            new($"Request metadata provider property '{propertyName}' in the type '{type.ToDisplayString()}' must be static.");

        InvalidOperationException CreatePropertyNotPublicException()
            =>
            new($"Request metadata provider property '{propertyName}' in the type '{type.ToDisplayString()}' must be public or internal.");

        InvalidOperationException CreatePropertyInvalidTypeException(string expectedType)
            =>
            new($"Request metadata provider property '{propertyName}' type in the type '{type.ToDisplayString()}' must be {expectedType}.");
    }

    private static EndpointOperationResponse GetOperationResponse(this IMethodSymbol method)
    {
        var responseType = method.ReturnType ?? throw CreateResponseTypeMustBeSpecifiedException(method);

        var taskType = responseType.GetGenericArgumentType();
        if ((taskType is not null) && (responseType.IsGenericTask() || responseType.IsGenericValueTask()))
        {
            responseType = taskType;
        }

        return responseType.GetOperationResponse(true);

        static InvalidOperationException CreateResponseTypeMustBeSpecifiedException(IMethodSymbol method)
            =>
            new($"Response type must be specified for method '{method.Name}'");
    }

    private static EndpointOperationResponse GetOperationResponse(this ITypeSymbol responseType, bool findUnion)
    {
        var responseTypeData = responseType.GetDisplayedData();
        var responseMapper = responseType.GetOperationResponseMapper();

        var union = findUnion ? responseType.GetDisciminatedUnion(null) : null;
        if (responseMapper is null && union is null)
        {
            throw new InvalidOperationException($"Response mapper must be specified for type '{responseTypeData.DisplayedTypeName}'");
        }

        return new(
            responseType: responseTypeData,
            responseMapper: responseMapper,
            responseMetadata: responseType.GetResponseMetadataProvider(),
            union: union);
    }

    private static EndpointOperationResponse.Mapper? GetOperationResponseMapper(this ITypeSymbol outputType)
    {
        var outputTypeData = outputType.GetDisplayedData();

        var mapperAttribute = outputType.GetAttributes().FirstOrDefault(IsMapperAttribute);
        var mapperType = mapperAttribute?.GetConstructorArgumentValue<INamedTypeSymbol>(0);

        if (mapperAttribute is null || mapperType is null)
        {
            if (outputType.IsEmptyStructure() is false)
            {
                return null;
            }

            return new(
                type: new(
                    allNamespaces: new List<string>(outputTypeData.AllNamespaces)
                    {
                        PrimeFuncPackNamespace
                    },
                    displayedTypeName: $"EndpointEmptyResponseMapper<{outputTypeData.DisplayedTypeName}>"),
                methodName: "MapResponse",
                methodType: EndpointOperationResponse.MapperMethodType.Default);
        }

        var mapperMethodName = mapperAttribute.GetConstructorArgumentValue<string>(1);
        if (string.IsNullOrWhiteSpace(mapperMethodName))
        {
            throw CreateMapperMethodNotSpecifiedException(outputType);
        }

        foreach (var method in mapperType.GetMembers(mapperMethodName!).OfType<IMethodSymbol>())
        {
            if ((method.IsStatic is false) || (method.DeclaredAccessibility is not (Accessibility.Public or Accessibility.Internal)))
            {
                continue;
            }

            if (method.Parameters.FirstOrDefault()?.Type?.Equals(outputType, SymbolEqualityComparer.Default) is not true)
            {
                continue;
            }

            var returnType = method.ReturnType;
            if (returnType is null)
            {
                continue;
            }

            var taskType = returnType.GetGenericArgumentType();

            var methodType = taskType switch
            {
                not null when returnType.IsGenericTask() => EndpointOperationResponse.MapperMethodType.Task,
                not null when returnType.IsGenericValueTask() => EndpointOperationResponse.MapperMethodType.ValueTask,
                _ => EndpointOperationResponse.MapperMethodType.Default
            };

            if (methodType is EndpointOperationResponse.MapperMethodType.Default)
            {
                if ((method.Parameters.Length is not 1) || (returnType.IsType(PrimeFuncPackNamespace, "EndpointResponse") is false))
                {
                    continue;
                }

                return new(outputTypeData, method.Name, default);
            }

            if ((method.Parameters.Length is not 2) || (taskType?.IsType(PrimeFuncPackNamespace, "EndpointResponse") is false))
            {
                continue;
            }

            if (method.Parameters[1].Type?.IsType("System.Threading", "CancellationToken") is false)
            {
                continue;
            }

            return new(outputTypeData, method.Name, methodType);
        }

        throw CreateMapperMethodNotFoundException(mapperMethodName, mapperType);

        static bool IsMapperAttribute(AttributeData attributeData)
            =>
            attributeData.AttributeClass?.IsType(PrimeFuncPackNamespace, "EndpointResponseMapperAttribute") is true;

        static InvalidOperationException CreateMapperMethodNotSpecifiedException(ITypeSymbol type)
            =>
            new($"Response mapper method must be specified for type '{type.ToDisplayString()}'.");

        static InvalidOperationException CreateMapperMethodNotFoundException(string? methodName, ITypeSymbol type)
            =>
            new($"Response mapper method '{methodName}' is not found in the type '{type.ToDisplayString()}'.");
    }

    private static EndpointOperationResponse.MetadataProvider? GetResponseMetadataProvider(this ITypeSymbol inputType)
    {
        var providerAttribute = inputType.GetAttributes().FirstOrDefault(IsMetadataProviderAttribute);
        var type = providerAttribute?.GetConstructorArgumentValue<INamedTypeSymbol>(0);

        if (providerAttribute is null || type is null)
        {
            if (inputType.IsEmptyStructure())
            {
                return EmptyResponseMetadataProvider;
            }

            return null;
        }

        var propertyName = providerAttribute.GetConstructorArgumentValue<string>(1);
        if (string.IsNullOrWhiteSpace(propertyName))
        {
            throw CreatePropertyNotSpecifiedException(inputType);
        }

        var property = type.GetMembers(propertyName!).OfType<IPropertySymbol>().FirstOrDefault() ?? throw CreatePropertyNotFoundException();
        if (property.Type?.IsType(PrimeFuncPackNamespace, "EndpointResponseMetadata") is not true)
        {
            throw CreatePropertyInvalidTypeException("EndpointResponseMetadata");
        }

        if (property.IsStatic is false)
        {
            throw CreatePropertyNotStaticException();
        }

        if (property.DeclaredAccessibility is not (Accessibility.Public or Accessibility.Internal))
        {
            throw CreatePropertyNotPublicException();
        }

        return new(type.GetDisplayedData(), propertyName!);

        static bool IsMetadataProviderAttribute(AttributeData attributeData)
            =>
            attributeData.AttributeClass?.IsType(PrimeFuncPackNamespace, "EndpointResponseMetadataProviderAttribute") is true;

        static InvalidOperationException CreatePropertyNotSpecifiedException(ITypeSymbol type)
            =>
            new($"Response metadata provider property must be specified for type '{type.ToDisplayString()}'.");

        InvalidOperationException CreatePropertyNotFoundException()
            =>
            new($"Response metadata provider property '{propertyName}' is not found in the type '{type.ToDisplayString()}'.");

        InvalidOperationException CreatePropertyNotStaticException()
            =>
            new($"Response metadata provider property '{propertyName}' in the type '{type.ToDisplayString()}' must be static.");

        InvalidOperationException CreatePropertyNotPublicException()
            =>
            new($"Response metadata provider property '{propertyName}' in the type '{type.ToDisplayString()}' must be public or internal.");

        InvalidOperationException CreatePropertyInvalidTypeException(string expectedType)
            =>
            new($"Response metadata provider property '{propertyName}' type in the type '{type.ToDisplayString()}' must be {expectedType}.");
    }

    private static EndpointDisciminatedUnionResponse? GetDisciminatedUnion(this ITypeSymbol responseType, ITypeSymbol? requestType)
    {
        foreach (var method in responseType.GetMembers().OfType<IMethodSymbol>().Where(IsFoldMethod))
        {
            var union = GetDisciminatedUnion(method, requestType);
            if (union is not null)
            {
                return union;
            }
        }

        return null;

        static bool IsFoldMethod(IMethodSymbol method)
        {
            if (method.IsStatic || method.DeclaredAccessibility is not Accessibility.Public)
            {
                return false;
            }

            if (method.Name.StartsWith("Fold", StringComparison.InvariantCultureIgnoreCase) is false)
            {
                return false;
            }

            if (method.TypeArguments.Length is not 1 || method.Parameters.Length < 2)
            {
                return false;
            }

            if (method.ReturnType?.IsGenericTask() is not true)
            {
                return false;
            }

            if (method.ReturnType is not INamedTypeSymbol namedType || namedType.TypeArguments.Length is not 1)
            {
                return false;
            }

            return namedType.TypeArguments[0].Equals(method.TypeParameters[0], SymbolEqualityComparer.Default);
        }
    }

    private static EndpointDisciminatedUnionResponse? GetDisciminatedUnion(this IMethodSymbol foldMethod, ITypeSymbol? requestType)
    {
        var responses = new EndpointOperationResponse[foldMethod.Parameters.Length];

        for (var i = 0; i < foldMethod.Parameters.Length; i++)
        {
            var response = GetOperationResponseFromFoldDelegate(foldMethod.Parameters[i]);
            if (response is null)
            {
                return null;
            }

            responses[i] = response;
        }

        return new(foldMethod.Name, responses);

        EndpointOperationResponse? GetOperationResponseFromFoldDelegate(IParameterSymbol parameter)
        {
            if (parameter.Type?.TypeKind is not TypeKind.Delegate)
            {
                return null;
            }

            if (parameter.Type.GetMembers("Invoke").FirstOrDefault() is not IMethodSymbol invokeMethod)
            {
                return null;
            }

            if (invokeMethod.Parameters.Length > 1)
            {
                return null;
            }

            if (invokeMethod.ReturnType?.Equals(foldMethod.ReturnType, SymbolEqualityComparer.Default) is not true)
            {
                return null;
            }

            var parameterType = invokeMethod.Parameters.FirstOrDefault()?.Type;
            if (parameterType is null)
            {
                return new(
                    responseType: null,
                    responseMetadata: EmptyResponseMetadataProvider);
            }

            if ((requestType is not null) && (parameterType.Equals(requestType, SymbolEqualityComparer.Default) is true))
            {
                return new(parameterType.GetDisplayedData());
            }

            return parameterType.GetOperationResponse(false);
        }
    }
}