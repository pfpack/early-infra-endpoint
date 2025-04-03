namespace PrimeFuncPack;

partial class EndpointSourceBuilder
{
    private string BuildEndpointFactorySource()
        =>
        new SourceBuilder(
            type.ApiType.Namespace)
        .AddUsing(
            "System",
            "Microsoft.Extensions.Logging",
            "PrimeFuncPack")
        .AppendCodeLine(
            $"public sealed partial class {type.TypeName} : IEndpoint")
        .BeginCodeBlock()
        .AppendResolveMethod(
            type)
        .AppendEmptyLine()
        .AppendTypeFields(
            type)
        .AppendEmptyLine()
        .AppendConstructor(
            type)
        .EndCodeBlock()
        .Build();
}

file static class FactoryBuilderExtensions
{
    internal static SourceBuilder AppendResolveMethod(this SourceBuilder builder, EndpointTypeData type)
        =>
        builder.AppendObsoleteAttributeIfNecessary(
            type.ApiType.ObsoleteData)
        .AppendCodeLine(
            $"{type.ApiType.GetVisibility()} static {type.TypeName} Resolve(IServiceProvider? serviceProvider, {type.ApiType.TypeName} endpointApi)")
        .BeginLambda()
        .AppendCodeLine(
            "new(")
        .BeginArguments()
        .AppendCodeLine(
            $"endpointApi: {GetNullValidatedValue("endpointApi", type.ApiType.IsReferenceType)},",
            $"logger: (serviceProvider?.GetService(typeof(ILoggerFactory)) as ILoggerFactory)?.CreateLogger<{type.TypeName}>());")
        .EndArguments()
        .EndLambda();

    internal static SourceBuilder AppendTypeFields(this SourceBuilder builder, EndpointTypeData type)
        =>
        builder.AppendObsoleteAttributeIfNecessary(
            type.ApiType.ObsoleteData)
        .AppendCodeLine(
            $"private readonly {type.ApiType.TypeName} {EndpointFieldNames.EndpointApi};")
        .AppendEmptyLine()
        .AppendCodeLine(
            $"private readonly ILogger? {EndpointFieldNames.Logger};");

    internal static SourceBuilder AppendConstructor(this SourceBuilder builder, EndpointTypeData type)
        =>
        builder.AppendObsoleteAttributeIfNecessary(
            type.ApiType.ObsoleteData)
        .AppendCodeLine(
            $"private {type.TypeName}({type.ApiType.TypeName} endpointApi, ILogger? logger)")
        .BeginCodeBlock()
        .AppendCodeLine(
            $"this.{EndpointFieldNames.EndpointApi} = endpointApi;",
            $"this.{EndpointFieldNames.Logger} = logger;")
        .EndCodeBlock();

    private static string GetVisibility(this EndpointApiTypeData type)
        =>
        type.IsPublic ? "public" : "internal";

    private static string GetNullValidatedValue(string argumentName, bool isReferenceType)
        =>
        isReferenceType ? $"{argumentName} ?? throw new ArgumentNullException(nameof({argumentName}))" : argumentName;
}