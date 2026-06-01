using LibraryManagement.Application.Common;
using LibraryManagement.Application.Dtos;

namespace LibraryManagement.Application.Services;

public interface IAuthService
{
    Task<Result<UserDto>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}
