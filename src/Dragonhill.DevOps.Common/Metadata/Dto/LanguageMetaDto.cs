namespace Dragonhill.DevOps.Metadata.Dto;

public class LanguageMetaDto
{
    public IEnumerable<string>? DependantLanguages { get; set; }
    public IEnumerable<string>? AlternativeExtensions { get; set; }
}
