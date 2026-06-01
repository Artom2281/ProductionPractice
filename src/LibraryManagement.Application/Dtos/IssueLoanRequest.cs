namespace LibraryManagement.Application.Dtos;

// Запрос на выдачу книги читателю
public class IssueLoanRequest
{
    public int BookId { get; set; }
    public int ReaderId { get; set; }
    public DateTime LoanDate { get; set; } = DateTime.UtcNow;
    public DateTime DueDate { get; set; } = DateTime.UtcNow.AddDays(14);
    public string? Notes { get; set; }
}
