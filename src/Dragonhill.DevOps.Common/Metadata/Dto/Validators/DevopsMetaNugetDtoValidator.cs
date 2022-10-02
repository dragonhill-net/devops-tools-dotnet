using FluentValidation;

namespace Dragonhill.DevOps.Metadata.Dto.Validators;

public class DevopsMetaNugetDtoValidator : AbstractValidator<DevopsMetaNugetDto>
{
    public DevopsMetaNugetDtoValidator()
    {
        RuleFor(x => x.PackageName)
            .NotEmpty();

        RuleFor(x => x.Authors)
            .NotEmpty();

        RuleForEach(x => x.Authors)
            .NotEmpty();
    }
}
