using LibraryManagement.Application.Common;
using LibraryManagement.Application.Dtos;

namespace LibraryManagement.Application.Services;

public interface IReaderService
{
    Task<IReadOnlyList<ReaderDto>> GetAllAsync(string? search = null, bool includeBlocked = true, CancellationToken cancellationToken = default);
    Task<ReaderDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<int>> SaveAsync(ReaderDto dto, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<Result> SetBlockedAsync(int id, bool isBlocked, CancellationToken cancellationToken = default);
}
