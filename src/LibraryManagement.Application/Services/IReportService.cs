using LibraryManagement.Application.Dtos;

namespace LibraryManagement.Application.Services;

public class PopularBookRow
{
    public int BookId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string AuthorFullName { get; set; } = string.Empty;
    public int LoanCount { get; set; }
}

public class ActiveReaderRow
{
    public int ReaderId { get; set; }
    public string CardNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public int LoanCount { get; set; }
}

public class LibraryStats
{
    public int TotalBooks { get; set; }
    public int TotalCopies { get; set; }
    public int AvailableCopies { get; set; }
    public int TotalReaders { get; set; }
    public int ActiveLoans { get; set; }
    public int OverdueLoans { get; set; }
}

public interface IReportService
{
    Task<LibraryStats> GetStatsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PopularBookRow>> GetPopularBooksAsync(int top = 10, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ActiveReaderRow>> GetActiveReadersAsync(int top = 10, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LoanDto>> GetOverdueLoansAsync(CancellationToken cancellationToken = default);
}
