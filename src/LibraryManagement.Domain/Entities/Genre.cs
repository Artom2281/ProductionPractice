using LibraryManagement.Domain.Common;

namespace LibraryManagement.Domain.Entities;

// Жанр книги (роман, фантастика, учебная литература и т.д.)
public class Genre : Entity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<Book> Books { get; set; } = new List<Book>();
}
