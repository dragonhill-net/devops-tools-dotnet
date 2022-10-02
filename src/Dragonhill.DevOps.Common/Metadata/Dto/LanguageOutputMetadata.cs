namespace Dragonhill.DevOps.Metadata.Dto;

public class LanguageOutputMetadata
{
    internal LanguageOutputMetadata(string canonicalExtension, LanguageMetaDto? inputMetadata = null)
    {
        CanonicalExtension = canonicalExtension;
        Extensions = inputMetadata?.AlternativeExtensions?.Prepend(canonicalExtension).ToArray() ?? new[] { canonicalExtension };
        DependantLanguages = inputMetadata?.DependantLanguages?.ToArray() ?? Array.Empty<string>();
    }

    public string CanonicalExtension { get; }
    public IReadOnlyList<string> Extensions { get; }
    public IReadOnlyList<string> DependantLanguages { get; }
}
