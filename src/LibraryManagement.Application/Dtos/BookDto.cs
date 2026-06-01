namespace LibraryManagement.Application.Dtos;

public class BookDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Isbn { get; set; }
    public DateTime? PublicationDate { get; set; }
    public int TotalCopies { get; set; }
    public int AvailableCopies { get; set; }
    public string? Description { get; set; }

    public int AuthorId { get; set; }
    public string AuthorFullName { get; set; } = string.Empty;

    public int GenreId { get; set; }
    public string GenreName { get; set; } = string.Empty;

    public int? PublisherId { get; set; }
    public string? PublisherName { get; set; }

    // Вычисляемое поле для отображения в гриде. Read-only с точки зрения DataGridView -
    // bound колонка не пытается записать обратно (нет setter'а).
    public string AvailabilityText => $"{AvailableCopies} / {TotalCopies}";
}
