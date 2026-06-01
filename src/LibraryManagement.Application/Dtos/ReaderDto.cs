namespace LibraryManagement.Application.Dtos;

public class ReaderDto
{
    public int Id { get; set; }
    public string CardNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public DateTime RegistrationDate { get; set; }
    public bool IsBlocked { get; set; }
    public string FullName => string.IsNullOrWhiteSpace(MiddleName)
        ? $"{LastName} {FirstName}".Trim()
        : $"{LastName} {FirstName} {MiddleName}".Trim();
}
