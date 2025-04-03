using System.Text;

namespace PrimeFuncPack;

partial class EndpointSourceBuilderExtensions
{
    internal static SourceBuilder AppendObsoleteAttributeIfNecessary(this SourceBuilder builder, ObsoleteData? obsoleteData)
    {
        if (obsoleteData is null)
        {
            return builder;
        }

        var attributeBuilder = new StringBuilder("[Obsolete(").Append(obsoleteData.Message.AsStringValueOrDefault());

        attributeBuilder = obsoleteData.IsError switch
        {
            true => attributeBuilder.Append(", true"),
            false => attributeBuilder.Append(", false"),
            _ => attributeBuilder
        };

        if (string.IsNullOrEmpty(obsoleteData.DiagnosticId) is false)
        {
            attributeBuilder = attributeBuilder.Append(", DiagnosticId = ").Append(obsoleteData.DiagnosticId.AsStringValueOrDefault());
        }

        if (string.IsNullOrEmpty(obsoleteData.UrlFormat) is false)
        {
            attributeBuilder = attributeBuilder.Append(", UrlFormat = ").Append(obsoleteData.UrlFormat.AsStringValueOrDefault());
        }

        attributeBuilder = attributeBuilder.Append(")]");
        return builder.AppendCodeLine(attributeBuilder.ToString());
    }
}