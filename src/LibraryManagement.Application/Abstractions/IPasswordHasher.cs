namespace LibraryManagement.Application.Abstractions;

// Хеширование паролей пользователей системы. Реализация на PBKDF2 в слое Data.
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}
