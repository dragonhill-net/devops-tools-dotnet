using FluentValidation;

namespace Dragonhill.DevOps.Metadata.Settings.Validators;

public class ClientDevopsMetaSettingsValidator : AbstractValidator<ClientDevopsMetaSettings>
{
    public ClientDevopsMetaSettingsValidator()
    {
        RuleFor(x => x.RepositoryUrl)
            .NotEmpty();

        RuleFor(x => x.Package)
            .NotEmpty();

        RuleFor(x => x.Languages)
            .NotNull();

        RuleForEach(x => x.Languages)
            .NotEmpty();
    }
}
