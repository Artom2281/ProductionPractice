using LibraryManagement.Domain.Enums;

namespace LibraryManagement.Application.Dtos;

// DTO пользователя системы. Password используется только при создании или смене пароля.
public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Librarian;
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }

    // Не возвращается из БД и не отображается на UI как текст. Только ввод формы.
    public string? Password { get; set; }
}
