using System.Security.Cryptography;
using LibraryManagement.Application.Abstractions;

namespace LibraryManagement.Data.Identity;

// Хеширование паролей по алгоритму PBKDF2 (SHA-256, 100 000 итераций).
// Формат хранения: "PBKDF2$<iterations>$<saltBase64>$<hashBase64>"
// Используется встроенный в .NET Rfc2898DeriveBytes - без сторонних NuGet.
public class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16; // 128 бит
    private const int HashSize = 32; // 256 бит
    private const int Iterations = 100_000;
    private const string Marker = "PBKDF2";

    public string Hash(string password)
    {
        if (password is null) throw new ArgumentNullException(nameof(password));

        Span<byte> salt = stackalloc byte[SaltSize];
        RandomNumberGenerator.Fill(salt);

        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, HashSize);

        return $"{Marker}${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    public bool Verify(string password, string hash)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hash)) return false;

        var parts = hash.Split('$');
        if (parts.Length != 4 || parts[0] != Marker) return false;
        if (!int.TryParse(parts[1], out var iterations) || iterations <= 0) return false;

        byte[] salt;
        byte[] expected;
        try
        {
            salt = Convert.FromBase64String(parts[2]);
            expected = Convert.FromBase64String(parts[3]);
        }
        catch (FormatException)
        {
            return false;
        }

        var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expected.Length);
        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }
}
