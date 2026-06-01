using FluentValidation;
using LibraryManagement.Application.Dtos;

namespace LibraryManagement.Application.Validators;

public class BookDtoValidator : AbstractValidator<BookDto>
{
    public BookDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Название книги обязательно.")
            .MaximumLength(300);

        RuleFor(x => x.Isbn)
            .MaximumLength(20)
            .Matches(@"^[0-9\-Xx]+$")
            .When(x => !string.IsNullOrWhiteSpace(x.Isbn))
            .WithMessage("ISBN может содержать только цифры, тире и символ X.");

        RuleFor(x => x.PublicationDate)
            .LessThanOrEqualTo(DateTime.UtcNow.AddYears(1))
            .When(x => x.PublicationDate.HasValue)
            .WithMessage("Дата издания не может быть слишком далеко в будущем.");

        RuleFor(x => x.TotalCopies)
            .GreaterThanOrEqualTo(1).WithMessage("Должен быть хотя бы 1 экземпляр.");

        RuleFor(x => x.AuthorId)
            .GreaterThan(0).WithMessage("Автор обязателен.");

        RuleFor(x => x.GenreId)
            .GreaterThan(0).WithMessage("Жанр обязателен.");

        RuleFor(x => x.Description).MaximumLength(2000);
    }
}
