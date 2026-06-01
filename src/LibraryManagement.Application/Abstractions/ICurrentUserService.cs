using LibraryManagement.Domain.Enums;

namespace LibraryManagement.Application.Abstractions;

// Информация о текущем авторизованном пользователе. Реализация на стороне UI (WinForms),
// потому что владельцем сессии является приложение, а не доменный слой.
public interface ICurrentUserService
{
    int? UserId { get; }
    string? Username { get; }
    UserRole? Role { get; }
    bool IsAuthenticated { get; }

    void SignIn(int userId, string username, UserRole role);
    void SignOut();
}
