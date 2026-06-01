using LibraryManagement.Domain.Enums;

namespace LibraryManagement.Application.Dtos;

public class LoanDto
{
    public int Id { get; set; }

    public int BookId { get; set; }
    public string BookTitle { get; set; } = string.Empty;

    public int ReaderId { get; set; }
    public string ReaderFullName { get; set; } = string.Empty;
    public string ReaderCardNumber { get; set; } = string.Empty;

    public DateTime LoanDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? ReturnedAt { get; set; }
    public LoanStatus Status { get; set; }
    public decimal? FineAmount { get; set; }
    public string? Notes { get; set; }

    public int IssuedByUserId { get; set; }
    public string IssuedByUserName { get; set; } = string.Empty;

    // Утилитарное свойство для UI: остаток дней до срока возврата (отрицательное число = просрочка)
    public int DaysRemaining => ReturnedAt.HasValue
        ? 0
        : (int)Math.Floor((DueDate - DateTime.UtcNow).TotalDays);

    // Дней просрочки - всегда положительное (для отображения в отчёте просроченных)
    public int OverdueDays => Math.Max(0, -DaysRemaining);

    // Русское представление статуса для DataGridView (вместо enum.ToString)
    public string StatusDisplay => Status switch
    {
        LoanStatus.Active => "Выдана",
        LoanStatus.Returned => "Возвращена",
        LoanStatus.Overdue => "Просрочена",
        _ => Status.ToString()
    };
}
