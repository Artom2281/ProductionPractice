using LibraryManagement.Application.Common;
using LibraryManagement.Application.Dtos;

namespace LibraryManagement.Application.Services;

public interface IGenreService
{
    Task<IReadOnlyList<GenreDto>> GetAllAsync(string? search = null, CancellationToken cancellationToken = default);
    Task<GenreDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<int>> SaveAsync(GenreDto dto, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
