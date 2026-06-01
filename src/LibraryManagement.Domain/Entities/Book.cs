using LibraryManagement.Domain.Common;

namespace LibraryManagement.Domain.Entities;

// Книга в фонде библиотеки. TotalCopies - всего экземпляров, AvailableCopies - доступно для выдачи
public class Book : Entity
{
    public string Title { get; set; } = string.Empty;
    public string? Isbn { get; set; }
    public DateTime? PublicationDate { get; set; }
    public int TotalCopies { get; set; }
    public int AvailableCopies { get; set; }
    public string? Description { get; set; }

    public int AuthorId { get; set; }
    public Author Author { get; set; } = null!;

    public int GenreId { get; set; }
    public Genre Genre { get; set; } = null!;

    public int? PublisherId { get; set; }
    public Publisher? Publisher { get; set; }

    public ICollection<Loan> Loans { get; set; } = new List<Loan>();
}
