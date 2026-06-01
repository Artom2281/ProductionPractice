using FluentValidation;
using LibraryManagement.Application.Dtos;

namespace LibraryManagement.Application.Validators;

public class PublisherDtoValidator : AbstractValidator<PublisherDto>
{
    public PublisherDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Название издательства обязательно.")
            .MaximumLength(200);

        RuleFor(x => x.City).MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(1000);
    }
}
