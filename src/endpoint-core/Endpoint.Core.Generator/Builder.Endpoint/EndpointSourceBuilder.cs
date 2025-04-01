using System.Collections.Generic;

namespace PrimeFuncPack;

internal sealed partial class EndpointSourceBuilder(EndpointTypeData type, IReadOnlyList<OperationSourceBuilder> operations)
{
    internal IReadOnlyList<SourceFileData> BuildSourceFiles()
        =>
        [
            new($"{type.TypeName}.g.cs", BuildEndpointFactorySource()),
            new($"{type.TypeName}.Metadata.g.cs", BuildEndpointMetadataSource()),
            new($"{type.TypeName}.Invoke.g.cs", BuildEndpointInvokeSource())
        ];
}