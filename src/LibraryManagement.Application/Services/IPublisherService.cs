using LibraryManagement.Application.Common;
using LibraryManagement.Application.Dtos;

namespace LibraryManagement.Application.Services;

public interface IPublisherService
{
    Task<IReadOnlyList<PublisherDto>> GetAllAsync(string? search = null, CancellationToken cancellationToken = default);
    Task<PublisherDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<int>> SaveAsync(PublisherDto dto, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
