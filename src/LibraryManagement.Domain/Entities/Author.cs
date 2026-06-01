using LibraryManagement.Domain.Common;

namespace LibraryManagement.Domain.Entities;

// Автор книги. Один автор может иметь много книг
public class Author : Entity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? Biography { get; set; }

    public ICollection<Book> Books { get; set; } = new List<Book>();

    // Удобное представление полного имени для отображения в UI и отчётах
    public string FullName => string.IsNullOrWhiteSpace(MiddleName)
        ? $"{LastName} {FirstName}".Trim()
        : $"{LastName} {FirstName} {MiddleName}".Trim();
}
