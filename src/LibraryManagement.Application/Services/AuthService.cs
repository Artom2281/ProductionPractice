using FluentValidation;
using LibraryManagement.Application.Abstractions;
using LibraryManagement.Application.Common;
using LibraryManagement.Application.Dtos;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Application.Services;

public class AuthService : IAuthService
{
    private readonly ILibraryDbContext _db;
    private readonly IPasswordHasher _hasher;
    private readonly IValidator<LoginRequest> _validator;

    public AuthService(ILibraryDbContext db, IPasswordHasher hasher, IValidator<LoginRequest> validator)
    {
        _db = db;
        _hasher = hasher;
        _validator = validator;
    }

    public async Task<Result<UserDto>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var validation = await _validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<UserDto>.ValidationFailure(validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Username == request.Username, cancellationToken);

        if (user is null || !user.IsActive)
        {
            return Result<UserDto>.Failure("Пользователь не найден или отключён.");
        }

        if (!_hasher.Verify(request.Password, user.PasswordHash))
        {
            return Result<UserDto>.Failure("Неверный логин или пароль.");
        }

        user.LastLoginAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        return Result<UserDto>.Success(new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            FullName = user.FullName,
            Role = user.Role,
            IsActive = user.IsActive,
            LastLoginAt = user.LastLoginAt
        });
    }
}
