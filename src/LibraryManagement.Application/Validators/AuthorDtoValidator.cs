using FluentValidation;
using LibraryManagement.Application.Dtos;

namespace LibraryManagement.Application.Validators;

public class AuthorDtoValidator : AbstractValidator<AuthorDto>
{
    public AuthorDtoValidator()
    {
        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Фамилия автора обязательна.")
            .MaximumLength(100);

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Имя автора обязательно.")
            .MaximumLength(100);

        RuleFor(x => x.MiddleName)
            .MaximumLength(100);

        RuleFor(x => x.BirthDate)
            .LessThan(DateTime.UtcNow)
            .When(x => x.BirthDate.HasValue)
            .WithMessage("Дата рождения не может быть в будущем.");

        // Совпадает с HasMaxLength(4000) в AuthorConfiguration; иначе EF упадёт DbUpdateException
        RuleFor(x => x.Biography).MaximumLength(4000);
    }
}
