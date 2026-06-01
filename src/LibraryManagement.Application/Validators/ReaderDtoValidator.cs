using FluentValidation;
using LibraryManagement.Application.Dtos;

namespace LibraryManagement.Application.Validators;

public class ReaderDtoValidator : AbstractValidator<ReaderDto>
{
    public ReaderDtoValidator()
    {
        RuleFor(x => x.CardNumber)
            .NotEmpty().WithMessage("Номер читательского билета обязателен.")
            .MaximumLength(20);

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Фамилия читателя обязательна.")
            .MaximumLength(100);

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Имя читателя обязательно.")
            .MaximumLength(100);

        RuleFor(x => x.MiddleName).MaximumLength(100);

        RuleFor(x => x.Phone)
            .MaximumLength(30)
            .Matches(@"^\+7 \(\d{3}\) \d{3}-\d{2}-\d{2}$")
            .When(x => !string.IsNullOrWhiteSpace(x.Phone))
            .WithMessage("Телефон должен быть в формате +7 (000) 000-00-00.");

        RuleFor(x => x.Email)
            .MaximumLength(150)
            .EmailAddress()
            .When(x => !string.IsNullOrWhiteSpace(x.Email))
            .WithMessage("Некорректный формат email.");

        RuleFor(x => x.Address).MaximumLength(300);

        RuleFor(x => x.BirthDate)
            .LessThan(DateTime.UtcNow)
            .When(x => x.BirthDate.HasValue)
            .WithMessage("Дата рождения не может быть в будущем.");
    }
}
