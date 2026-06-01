using LibraryManagement.Domain.Common;

namespace LibraryManagement.Domain.Entities;

// Читатель библиотеки. CardNumber - номер читательского билета (уникальный)
public class Reader : Entity
{
    public string CardNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;
    public bool IsBlocked { get; set; }

    public ICollection<Loan> Loans { get; set; } = new List<Loan>();

    public string FullName => string.IsNullOrWhiteSpace(MiddleName)
        ? $"{LastName} {FirstName}".Trim()
        : $"{LastName} {FirstName} {MiddleName}".Trim();
}
