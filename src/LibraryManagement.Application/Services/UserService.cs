using FluentValidation;
using LibraryManagement.Application.Abstractions;
using LibraryManagement.Application.Common;
using LibraryManagement.Application.Dtos;
using LibraryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Application.Services;

public class UserService : IUserService
{
    private readonly ILibraryDbContext _db;
    private readonly IPasswordHasher _hasher;
    private readonly IValidator<UserDto> _validator;

    public UserService(ILibraryDbContext db, IPasswordHasher hasher, IValidator<UserDto> validator)
    {
        _db = db;
        _hasher = hasher;
        _validator = validator;
    }

    public async Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _db.Users.AsNoTracking()
            .OrderBy(u => u.Username)
            .Select(u => new UserDto
            {
                Id = u.Id,
                Username = u.Username,
                FullName = u.FullName,
                Role = u.Role,
                IsActive = u.IsActive,
                LastLoginAt = u.LastLoginAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<UserDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _db.Users.AsNoTracking()
            .Where(u => u.Id == id)
            .Select(u => new UserDto
            {
                Id = u.Id,
                Username = u.Username,
                FullName = u.FullName,
                Role = u.Role,
                IsActive = u.IsActive,
                LastLoginAt = u.LastLoginAt
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Result<int>> SaveAsync(UserDto dto, CancellationToken cancellationToken = default)
    {
        var validation = await _validator.ValidateAsync(dto, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<int>.ValidationFailure(validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var trimmedUsername = dto.Username.Trim();
        var usernameTaken = await _db.Users.AnyAsync(u => u.Username == trimmedUsername && u.Id != dto.Id, cancellationToken);
        if (usernameTaken)
        {
            return Result<int>.Failure($"Логин \"{trimmedUsername}\" уже используется.");
        }

        User entity;
        if (dto.Id == 0)
        {
            if (string.IsNullOrWhiteSpace(dto.Password))
            {
                return Result<int>.Failure("При создании пользователя необходимо указать пароль.");
            }
            entity = new User { PasswordHash = _hasher.Hash(dto.Password) };
            _db.Users.Add(entity);
        }
        else
        {
            entity = await _db.Users.FirstOrDefaultAsync(u => u.Id == dto.Id, cancellationToken)
                ?? throw new InvalidOperationException($"Пользователь Id={dto.Id} не найден.");
            entity.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                entity.PasswordHash = _hasher.Hash(dto.Password);
            }
        }

        entity.Username = trimmedUsername;
        entity.FullName = dto.FullName.Trim();
        entity.Role = dto.Role;
        entity.IsActive = dto.IsActive;

        await _db.SaveChangesAsync(cancellationToken);
        return Result<int>.Success(entity.Id);
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Users
            .Include(u => u.IssuedLoans)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        if (entity is null) return Result.Failure("Пользователь не найден.");
        if (entity.IssuedLoans.Any())
        {
            // Не удаляем чтобы сохранить аудит выдач, но деактивируем
            entity.IsActive = false;
            entity.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
            return Result.Failure("Пользователь оформлял выдачи и сохранён в системе как неактивный (для аудита). Полное удаление невозможно.");
        }

        _db.Users.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> ChangePasswordAsync(int id, string newPassword, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
        {
            return Result.Failure("Пароль должен содержать минимум 6 символов.");
        }

        var entity = await _db.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        if (entity is null) return Result.Failure("Пользователь не найден.");

        entity.PasswordHash = _hasher.Hash(newPassword);
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
