using FluentValidation;
using LibraryManagement.Application.Dtos;

namespace LibraryManagement.Application.Validators;

public class UserDtoValidator : AbstractValidator<UserDto>
{
    public UserDtoValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Логин обязателен.")
            .MaximumLength(50)
            .Matches(@"^[a-zA-Z0-9_\-\.]+$")
            .WithMessage("Логин может содержать только латинские буквы, цифры, точку, дефис и подчёркивание.");

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("ФИО пользователя обязательны.")
            .MaximumLength(200);

        RuleFor(x => x.Password)
            .MinimumLength(6)
            .When(x => !string.IsNullOrWhiteSpace(x.Password))
            .WithMessage("Пароль должен содержать минимум 6 символов.");
    }
}
