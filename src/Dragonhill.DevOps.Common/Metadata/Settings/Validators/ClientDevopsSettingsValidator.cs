using FluentValidation;

namespace Dragonhill.DevOps.Metadata.Settings.Validators;

public class ClientDevopsSettingsValidator : AbstractValidator<ClientDevopsSettings>
{
    public ClientDevopsSettingsValidator()
    {
        RuleFor(x => x.Meta)
            .SetValidator(new ClientDevopsMetaSettingsValidator());
    }
}
