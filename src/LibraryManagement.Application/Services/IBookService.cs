using LibraryManagement.Application.Common;
using LibraryManagement.Application.Dtos;

namespace LibraryManagement.Application.Services;

public interface IBookService
{
    Task<IReadOnlyList<BookDto>> GetAllAsync(string? search = null, int? authorId = null, int? genreId = null, bool onlyAvailable = false, CancellationToken cancellationToken = default);
    Task<BookDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<int>> SaveAsync(BookDto dto, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
