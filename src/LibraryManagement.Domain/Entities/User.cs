using LibraryManagement.Domain.Common;
using LibraryManagement.Domain.Enums;

namespace LibraryManagement.Domain.Entities;

// Пользователь системы (библиотекарь или администратор)
// PasswordHash хранит производную PBKDF2 (не пароль в открытом виде)
public class User : Entity
{
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Librarian;
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }

    public ICollection<Loan> IssuedLoans { get; set; } = new List<Loan>();
}
