using LibraryManagement.Application.Abstractions;
using LibraryManagement.Application.Dtos;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Application.Services;

public class ReportService : IReportService
{
    private readonly ILibraryDbContext _db;

    public ReportService(ILibraryDbContext db)
    {
        _db = db;
    }

    public async Task<LibraryStats> GetStatsAsync(CancellationToken cancellationToken = default)
    {
        var todayDate = DateTime.UtcNow.Date;
        return new LibraryStats
        {
            TotalBooks = await _db.Books.CountAsync(cancellationToken),
            TotalCopies = await _db.Books.SumAsync(b => (int?)b.TotalCopies, cancellationToken) ?? 0,
            AvailableCopies = await _db.Books.SumAsync(b => (int?)b.AvailableCopies, cancellationToken) ?? 0,
            TotalReaders = await _db.Readers.CountAsync(cancellationToken),
            ActiveLoans = await _db.Loans.CountAsync(l => l.ReturnedAt == null, cancellationToken),
            OverdueLoans = await _db.Loans.CountAsync(l => l.ReturnedAt == null && l.DueDate.Date < todayDate, cancellationToken)
        };
    }

    public async Task<IReadOnlyList<PopularBookRow>> GetPopularBooksAsync(int top = 10, CancellationToken cancellationToken = default)
    {
        return await _db.Loans
            .AsNoTracking()
            .GroupBy(l => l.BookId)
            .Select(g => new
            {
                BookId = g.Key,
                LoanCount = g.Count()
            })
            .OrderByDescending(x => x.LoanCount)
            .Take(top)
            .Join(_db.Books.Include(b => b.Author),
                  agg => agg.BookId,
                  b => b.Id,
                  (agg, b) => new PopularBookRow
                  {
                      BookId = b.Id,
                      Title = b.Title,
                      AuthorFullName = (b.Author.MiddleName == null || b.Author.MiddleName == "")
                          ? b.Author.LastName + " " + b.Author.FirstName
                          : b.Author.LastName + " " + b.Author.FirstName + " " + b.Author.MiddleName,
                      LoanCount = agg.LoanCount
                  })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ActiveReaderRow>> GetActiveReadersAsync(int top = 10, CancellationToken cancellationToken = default)
    {
        return await _db.Loans
            .AsNoTracking()
            .GroupBy(l => l.ReaderId)
            .Select(g => new
            {
                ReaderId = g.Key,
                LoanCount = g.Count()
            })
            .OrderByDescending(x => x.LoanCount)
            .Take(top)
            .Join(_db.Readers,
                  agg => agg.ReaderId,
                  r => r.Id,
                  (agg, r) => new ActiveReaderRow
                  {
                      ReaderId = r.Id,
                      CardNumber = r.CardNumber,
                      FullName = (r.MiddleName == null || r.MiddleName == "")
                          ? r.LastName + " " + r.FirstName
                          : r.LastName + " " + r.FirstName + " " + r.MiddleName,
                      LoanCount = agg.LoanCount
                  })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<LoanDto>> GetOverdueLoansAsync(CancellationToken cancellationToken = default)
    {
        var todayDate = DateTime.UtcNow.Date;
        return await _db.Loans
            .AsNoTracking()
            .Include(l => l.Book)
            .Include(l => l.Reader)
            .Include(l => l.IssuedByUser)
            .Where(l => l.ReturnedAt == null && l.DueDate.Date < todayDate)
            .OrderBy(l => l.DueDate)
            .Select(l => new LoanDto
            {
                Id = l.Id,
                BookId = l.BookId,
                BookTitle = l.Book.Title,
                ReaderId = l.ReaderId,
                ReaderFullName = (l.Reader.MiddleName == null || l.Reader.MiddleName == "")
                    ? l.Reader.LastName + " " + l.Reader.FirstName
                    : l.Reader.LastName + " " + l.Reader.FirstName + " " + l.Reader.MiddleName,
                ReaderCardNumber = l.Reader.CardNumber,
                LoanDate = l.LoanDate,
                DueDate = l.DueDate,
                ReturnedAt = l.ReturnedAt,
                Status = l.Status,
                FineAmount = l.FineAmount,
                Notes = l.Notes,
                IssuedByUserId = l.IssuedByUserId,
                IssuedByUserName = l.IssuedByUser.FullName
            })
            .ToListAsync(cancellationToken);
    }
}
