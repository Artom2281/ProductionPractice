using LibraryManagement.Application.Common;
using LibraryManagement.Application.Dtos;

namespace LibraryManagement.Application.Services;

public interface IUserService
{
    Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<UserDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<int>> SaveAsync(UserDto dto, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<Result> ChangePasswordAsync(int id, string newPassword, CancellationToken cancellationToken = default);
}
