using FluentValidation;

namespace Dragonhill.DevOps.Metadata.Dto.Validators;

public class DevopsMetaDtoValidator : AbstractValidator<DevopsMetaDto>
{
    public DevopsMetaDtoValidator()
    {
        RuleFor(x => x.Nuget)
            .NotNull()
            .SetValidator(new DevopsMetaNugetDtoValidator());
    }
}
