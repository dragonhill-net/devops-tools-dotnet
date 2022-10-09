namespace Dragonhill.DevOps.Metadata.Dto;

public class LanguagePackageMetadata
{
    public LanguagePackageMetadata() { }

    internal LanguagePackageMetadata(string canonicalExtension, LanguageMetaDto? inputMetadata = null)
    {
        CanonicalExtension = canonicalExtension;
        Extensions = inputMetadata?.AlternativeExtensions?.Prepend(canonicalExtension).ToHashSet() ?? new HashSet<string> { canonicalExtension };
        DependantLanguages = inputMetadata?.DependantLanguages?.ToHashSet() ?? new HashSet<string>();
    }

    public string CanonicalExtension { get; set; }
    public HashSet<string> Extensions { get; set; }
    public HashSet<string> DependantLanguages { get; set; }
}
