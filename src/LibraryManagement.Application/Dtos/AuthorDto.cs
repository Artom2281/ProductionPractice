namespace LibraryManagement.Application.Dtos;

// DTO автора. Id = 0 для создания, > 0 для обновления
public class AuthorDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? Biography { get; set; }
    public string FullName => string.IsNullOrWhiteSpace(MiddleName)
        ? $"{LastName} {FirstName}".Trim()
        : $"{LastName} {FirstName} {MiddleName}".Trim();
}
