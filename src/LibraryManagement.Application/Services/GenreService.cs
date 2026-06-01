using FluentValidation;
using LibraryManagement.Application.Abstractions;
using LibraryManagement.Application.Common;
using LibraryManagement.Application.Dtos;
using LibraryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Application.Services;

public class GenreService : IGenreService
{
    private readonly ILibraryDbContext _db;
    private readonly IValidator<GenreDto> _validator;

    public GenreService(ILibraryDbContext db, IValidator<GenreDto> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task<IReadOnlyList<GenreDto>> GetAllAsync(string? search = null, CancellationToken cancellationToken = default)
    {
        var query = _db.Genres.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(g => EF.Functions.Like(g.Name, pattern));
        }

        return await query
            .OrderBy(g => g.Name)
            .Select(g => new GenreDto { Id = g.Id, Name = g.Name, Description = g.Description })
            .ToListAsync(cancellationToken);
    }

    public async Task<GenreDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _db.Genres.AsNoTracking()
            .Where(g => g.Id == id)
            .Select(g => new GenreDto { Id = g.Id, Name = g.Name, Description = g.Description })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Result<int>> SaveAsync(GenreDto dto, CancellationToken cancellationToken = default)
    {
        var validation = await _validator.ValidateAsync(dto, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<int>.ValidationFailure(validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var trimmedName = dto.Name.Trim();
        var nameTaken = await _db.Genres.AnyAsync(g => g.Name == trimmedName && g.Id != dto.Id, cancellationToken);
        if (nameTaken)
        {
            return Result<int>.Failure($"Жанр с названием \"{trimmedName}\" уже существует.");
        }

        Genre entity;
        if (dto.Id == 0)
        {
            entity = new Genre();
            _db.Genres.Add(entity);
        }
        else
        {
            entity = await _db.Genres.FirstOrDefaultAsync(g => g.Id == dto.Id, cancellationToken)
                ?? throw new InvalidOperationException($"Жанр Id={dto.Id} не найден.");
            entity.UpdatedAt = DateTime.UtcNow;
        }

        entity.Name = trimmedName;
        entity.Description = dto.Description;

        await _db.SaveChangesAsync(cancellationToken);
        return Result<int>.Success(entity.Id);
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Genres
            .Include(g => g.Books)
            .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);

        if (entity is null) return Result.Failure("Жанр не найден.");
        if (entity.Books.Any()) return Result.Failure("Нельзя удалить жанр, к которому привязаны книги.");

        _db.Genres.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
