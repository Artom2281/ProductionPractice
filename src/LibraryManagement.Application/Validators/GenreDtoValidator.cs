using FluentValidation;
using LibraryManagement.Application.Dtos;

namespace LibraryManagement.Application.Validators;

public class GenreDtoValidator : AbstractValidator<GenreDto>
{
    public GenreDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Название жанра обязательно.")
            .MaximumLength(100);

        RuleFor(x => x.Description)
            .MaximumLength(500);
    }
}
