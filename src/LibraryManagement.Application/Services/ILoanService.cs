using LibraryManagement.Application.Common;
using LibraryManagement.Application.Dtos;

namespace LibraryManagement.Application.Services;

public interface ILoanService
{
    Task<IReadOnlyList<LoanDto>> GetAllAsync(string? search = null, int? readerId = null, int? bookId = null, bool onlyActive = false, bool onlyOverdue = false, CancellationToken cancellationToken = default);
    Task<LoanDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    // Оформление выдачи. issuedByUserId - текущий авторизованный библиотекарь
    Task<Result<int>> IssueAsync(IssueLoanRequest request, int issuedByUserId, CancellationToken cancellationToken = default);

    // Возврат книги. fineAmount - штраф за просрочку или порчу (опционально)
    Task<Result> ReturnAsync(int loanId, decimal? fineAmount = null, string? notes = null, CancellationToken cancellationToken = default);

    // Пересчёт статусов всех активных выдач (Active -> Overdue если срок истёк)
    Task RefreshOverdueStatusesAsync(CancellationToken cancellationToken = default);
}
