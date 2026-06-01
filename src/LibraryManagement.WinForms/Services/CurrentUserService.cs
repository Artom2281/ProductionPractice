using LibraryManagement.Application.Abstractions;
using LibraryManagement.Domain.Enums;

namespace LibraryManagement.WinForms.Services;

// Хранит контекст текущей UI-сессии. Инициализируется при успешном логине.
// Регистрируется как Singleton - на одно WinForms-приложение всегда одна сессия.
public class CurrentUserService : ICurrentUserService
{
    public int? UserId { get; private set; }
    public string? Username { get; private set; }
    public UserRole? Role { get; private set; }
    public bool IsAuthenticated => UserId.HasValue;

    public void SignIn(int userId, string username, UserRole role)
    {
        UserId = userId;
        Username = username;
        Role = role;
    }

    public void SignOut()
    {
        UserId = null;
        Username = null;
        Role = null;
    }
}
