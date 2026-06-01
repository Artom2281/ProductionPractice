using LibraryManagement.Domain.Common;

namespace LibraryManagement.Domain.Entities;

// Издательство. Книга ссылается на издательство по PublisherId.
public class Publisher : Entity
{
    public string Name { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? Description { get; set; }

    public ICollection<Book> Books { get; set; } = new List<Book>();
}
