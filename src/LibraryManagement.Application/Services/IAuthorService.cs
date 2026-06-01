using LibraryManagement.Application.Common;
using LibraryManagement.Application.Dtos;

namespace LibraryManagement.Application.Services;

public interface IAuthorService
{
    Task<IReadOnlyList<AuthorDto>> GetAllAsync(string? search = null, CancellationToken cancellationToken = default);
    Task<AuthorDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<int>> SaveAsync(AuthorDto dto, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
