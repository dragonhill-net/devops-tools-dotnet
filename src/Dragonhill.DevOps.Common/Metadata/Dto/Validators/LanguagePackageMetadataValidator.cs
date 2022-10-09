using FluentValidation;

namespace Dragonhill.DevOps.Metadata.Dto.Validators;

public class LanguagePackageMetadataValidator : AbstractValidator<LanguagePackageMetadata>
{
    public LanguagePackageMetadataValidator()
    {
        RuleFor(x => x.CanonicalExtension)
            .NotEmpty();

        RuleFor(x => x.Extensions)
            .NotEmpty()
            .Must((meta, x) => x.Contains(meta.CanonicalExtension));

        RuleForEach(x => x.Extensions)
            .NotEmpty();

        RuleFor(x => x.DependantLanguages)
            .NotNull();

        RuleForEach(x => x.DependantLanguages)
            .NotEmpty()
            .Must((meta, x) => !meta.Extensions.Contains(x));
    }
}
