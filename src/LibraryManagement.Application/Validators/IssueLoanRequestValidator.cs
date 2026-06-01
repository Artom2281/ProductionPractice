using FluentValidation;
using LibraryManagement.Application.Dtos;

namespace LibraryManagement.Application.Validators;

public class IssueLoanRequestValidator : AbstractValidator<IssueLoanRequest>
{
    public IssueLoanRequestValidator()
    {
        RuleFor(x => x.BookId).GreaterThan(0).WithMessage("Книга обязательна.");
        RuleFor(x => x.ReaderId).GreaterThan(0).WithMessage("Читатель обязателен.");

        RuleFor(x => x.DueDate)
            .GreaterThan(x => x.LoanDate)
            .WithMessage("Срок возврата должен быть позже даты выдачи.");

        RuleFor(x => x.Notes).MaximumLength(500);
    }
}
