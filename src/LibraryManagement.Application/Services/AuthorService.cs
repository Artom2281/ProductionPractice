using FluentValidation;
using LibraryManagement.Application.Abstractions;
using LibraryManagement.Application.Common;
using LibraryManagement.Application.Dtos;
using LibraryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Application.Services;

public class AuthorService : IAuthorService
{
    private readonly ILibraryDbContext _db;
    private readonly IValidator<AuthorDto> _validator;

    public AuthorService(ILibraryDbContext db, IValidator<AuthorDto> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task<IReadOnlyList<AuthorDto>> GetAllAsync(string? search = null, CancellationToken cancellationToken = default)
    {
        var query = _db.Authors.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(a =>
                EF.Functions.Like(a.LastName, pattern) ||
                EF.Functions.Like(a.FirstName, pattern) ||
                (a.MiddleName != null && EF.Functions.Like(a.MiddleName, pattern)));
        }

        return await query
            .OrderBy(a => a.LastName).ThenBy(a => a.FirstName)
            .Select(a => new AuthorDto
            {
                Id = a.Id,
                FirstName = a.FirstName,
                LastName = a.LastName,
                MiddleName = a.MiddleName,
                BirthDate = a.BirthDate,
                Biography = a.Biography
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<AuthorDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var a = await _db.Authors.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (a is null) return null;
        return new AuthorDto
        {
            Id = a.Id,
            FirstName = a.FirstName,
            LastName = a.LastName,
            MiddleName = a.MiddleName,
            BirthDate = a.BirthDate,
            Biography = a.Biography
        };
    }

    public async Task<Result<int>> SaveAsync(AuthorDto dto, CancellationToken cancellationToken = default)
    {
        var validation = await _validator.ValidateAsync(dto, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<int>.ValidationFailure(validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        Author entity;
        if (dto.Id == 0)
        {
            entity = new Author();
            _db.Authors.Add(entity);
        }
        else
        {
            entity = await _db.Authors.FirstOrDefaultAsync(x => x.Id == dto.Id, cancellationToken)
                ?? throw new InvalidOperationException($"Автор Id={dto.Id} не найден.");
            entity.UpdatedAt = DateTime.UtcNow;
        }

        entity.FirstName = dto.FirstName.Trim();
        entity.LastName = dto.LastName.Trim();
        entity.MiddleName = string.IsNullOrWhiteSpace(dto.MiddleName) ? null : dto.MiddleName.Trim();
        entity.BirthDate = dto.BirthDate;
        entity.Biography = dto.Biography;

        await _db.SaveChangesAsync(cancellationToken);
        return Result<int>.Success(entity.Id);
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Authors
            .Include(a => a.Books)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (entity is null)
        {
            return Result.Failure("Автор не найден.");
        }

        if (entity.Books.Any())
        {
            return Result.Failure("Нельзя удалить автора, у которого есть книги. Сначала переназначьте или удалите книги.");
        }

        _db.Authors.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
