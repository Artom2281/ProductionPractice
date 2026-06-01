using FluentValidation;
using LibraryManagement.Application.Abstractions;
using LibraryManagement.Application.Common;
using LibraryManagement.Application.Dtos;
using LibraryManagement.Domain.Entities;
using LibraryManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Application.Services;

public class LoanService : ILoanService
{
    // Фиксированный штраф за просрочку, выставляется автоматически при переводе выдачи в Overdue
    public const decimal OverdueFineAmount = 500m;

    private readonly ILibraryDbContext _db;
    private readonly IValidator<IssueLoanRequest> _validator;

    public LoanService(ILibraryDbContext db, IValidator<IssueLoanRequest> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task<IReadOnlyList<LoanDto>> GetAllAsync(string? search = null, int? readerId = null, int? bookId = null, bool onlyActive = false, bool onlyOverdue = false, CancellationToken cancellationToken = default)
    {
        var query = _db.Loans
            .AsNoTracking()
            .Include(l => l.Book)
            .Include(l => l.Reader)
            .Include(l => l.IssuedByUser)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(l =>
                EF.Functions.Like(l.Book.Title, pattern) ||
                EF.Functions.Like(l.Reader.LastName, pattern) ||
                EF.Functions.Like(l.Reader.FirstName, pattern) ||
                (l.Reader.MiddleName != null && EF.Functions.Like(l.Reader.MiddleName, pattern)) ||
                EF.Functions.Like(l.Reader.CardNumber, pattern));
        }

        if (readerId.HasValue) query = query.Where(l => l.ReaderId == readerId.Value);
        if (bookId.HasValue) query = query.Where(l => l.BookId == bookId.Value);
        if (onlyActive) query = query.Where(l => l.ReturnedAt == null);
        if (onlyOverdue)
        {
            var todayDate = DateTime.UtcNow.Date;
            query = query.Where(l => l.ReturnedAt == null && l.DueDate.Date < todayDate);
        }

        return await query
            .OrderByDescending(l => l.LoanDate)
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

    public async Task<LoanDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var l = await _db.Loans
            .AsNoTracking()
            .Include(x => x.Book)
            .Include(x => x.Reader)
            .Include(x => x.IssuedByUser)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (l is null) return null;

        return new LoanDto
        {
            Id = l.Id,
            BookId = l.BookId,
            BookTitle = l.Book.Title,
            ReaderId = l.ReaderId,
            ReaderFullName = l.Reader.FullName,
            ReaderCardNumber = l.Reader.CardNumber,
            LoanDate = l.LoanDate,
            DueDate = l.DueDate,
            ReturnedAt = l.ReturnedAt,
            Status = l.Status,
            FineAmount = l.FineAmount,
            Notes = l.Notes,
            IssuedByUserId = l.IssuedByUserId,
            IssuedByUserName = l.IssuedByUser.FullName
        };
    }

    public async Task<Result<int>> IssueAsync(IssueLoanRequest request, int issuedByUserId, CancellationToken cancellationToken = default)
    {
        var validation = await _validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<int>.ValidationFailure(validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var book = await _db.Books.FirstOrDefaultAsync(b => b.Id == request.BookId, cancellationToken);
        if (book is null) return Result<int>.Failure("Книга не найдена.");
        if (book.AvailableCopies <= 0) return Result<int>.Failure("Нет свободных экземпляров для выдачи.");

        var reader = await _db.Readers.FirstOrDefaultAsync(r => r.Id == request.ReaderId, cancellationToken);
        if (reader is null) return Result<int>.Failure("Читатель не найден.");
        if (reader.IsBlocked) return Result<int>.Failure("Читатель заблокирован — выдача невозможна.");

        var todayDate = DateTime.UtcNow.Date;
        var hasOverdue = await _db.Loans.AnyAsync(
            l => l.ReaderId == reader.Id && l.ReturnedAt == null && l.DueDate.Date < todayDate,
            cancellationToken);
        if (hasOverdue)
        {
            return Result<int>.Failure("У читателя есть просроченные выдачи. Сначала верните их.");
        }

        var loan = new Loan
        {
            BookId = book.Id,
            ReaderId = reader.Id,
            LoanDate = request.LoanDate,
            DueDate = request.DueDate,
            Status = LoanStatus.Active,
            Notes = request.Notes,
            IssuedByUserId = issuedByUserId
        };

        book.AvailableCopies -= 1;
        book.UpdatedAt = DateTime.UtcNow;

        _db.Loans.Add(loan);
        await _db.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(loan.Id);
    }

    public async Task<Result> ReturnAsync(int loanId, decimal? fineAmount = null, string? notes = null, CancellationToken cancellationToken = default)
    {
        var loan = await _db.Loans
            .Include(l => l.Book)
            .FirstOrDefaultAsync(l => l.Id == loanId, cancellationToken);

        if (loan is null) return Result.Failure("Выдача не найдена.");
        if (loan.ReturnedAt is not null) return Result.Failure("Книга уже возвращена.");

        loan.ReturnedAt = DateTime.UtcNow;
        loan.Status = LoanStatus.Returned;
        // null от UI означает "штраф не указан, оставить как было".
        // Иначе - перезаписать (включая 0 = "штраф снят вручную").
        // Без этой проверки авто-штраф 500р из RefreshOverdueStatusesAsync терялся бы при возврате.
        if (fineAmount.HasValue) loan.FineAmount = fineAmount;
        loan.UpdatedAt = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(notes))
        {
            loan.Notes = string.IsNullOrWhiteSpace(loan.Notes) ? notes : $"{loan.Notes}\n{notes}";
        }

        loan.Book.AvailableCopies = Math.Min(loan.Book.AvailableCopies + 1, loan.Book.TotalCopies);
        loan.Book.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task RefreshOverdueStatusesAsync(CancellationToken cancellationToken = default)
    {
        // Просрочка считается со СЛЕДУЮЩЕГО дня после DueDate.
        // То есть весь день срока возврата ещё считается нормальным.
        // Пример: DueDate = 19.05, сегодня 19.05 - не просрочено; завтра 20.05 - уже да.
        var today = DateTime.UtcNow.Date;
        var toUpdate = await _db.Loans
            .Where(l => l.ReturnedAt == null && l.DueDate.Date < today && l.Status != LoanStatus.Overdue)
            .ToListAsync(cancellationToken);

        foreach (var l in toUpdate)
        {
            l.Status = LoanStatus.Overdue;
            l.UpdatedAt = DateTime.UtcNow;
            // Авто-штраф 500 рублей при первом обнаружении просрочки.
            // Если ранее уже был выставлен штраф вручную - не перезаписываем.
            if (!l.FineAmount.HasValue)
            {
                l.FineAmount = OverdueFineAmount;
            }
        }

        if (toUpdate.Count > 0) await _db.SaveChangesAsync(cancellationToken);
    }
}
