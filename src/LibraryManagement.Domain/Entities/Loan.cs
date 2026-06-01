using LibraryManagement.Domain.Common;
using LibraryManagement.Domain.Enums;

namespace LibraryManagement.Domain.Entities;

// Запись о выдаче книги читателю. ReturnedAt = null пока книга на руках
public class Loan : Entity
{
    public int BookId { get; set; }
    public Book Book { get; set; } = null!;

    public int ReaderId { get; set; }
    public Reader Reader { get; set; } = null!;

    public DateTime LoanDate { get; set; } = DateTime.UtcNow;
    public DateTime DueDate { get; set; }
    public DateTime? ReturnedAt { get; set; }
    public LoanStatus Status { get; set; } = LoanStatus.Active;
    public decimal? FineAmount { get; set; }
    public string? Notes { get; set; }

    // Кто оформил выдачу (для аудита)
    public int IssuedByUserId { get; set; }
    public User IssuedByUser { get; set; } = null!;
}
