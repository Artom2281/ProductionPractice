using FluentValidation;
using LibraryManagement.Application.Abstractions;
using LibraryManagement.Application.Common;
using LibraryManagement.Application.Dtos;
using LibraryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Application.Services;

public class PublisherService : IPublisherService
{
    private readonly ILibraryDbContext _db;
    private readonly IValidator<PublisherDto> _validator;

    public PublisherService(ILibraryDbContext db, IValidator<PublisherDto> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task<IReadOnlyList<PublisherDto>> GetAllAsync(string? search = null, CancellationToken cancellationToken = default)
    {
        var query = _db.Publishers.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(p =>
                EF.Functions.Like(p.Name, pattern) ||
                (p.City != null && EF.Functions.Like(p.City, pattern)));
        }

        return await query
            .OrderBy(p => p.Name)
            .Select(p => new PublisherDto { Id = p.Id, Name = p.Name, City = p.City, Description = p.Description })
            .ToListAsync(cancellationToken);
    }

    public async Task<PublisherDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _db.Publishers.AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new PublisherDto { Id = p.Id, Name = p.Name, City = p.City, Description = p.Description })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Result<int>> SaveAsync(PublisherDto dto, CancellationToken cancellationToken = default)
    {
        var validation = await _validator.ValidateAsync(dto, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<int>.ValidationFailure(validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var trimmedName = dto.Name.Trim();
        var nameTaken = await _db.Publishers.AnyAsync(p => p.Name == trimmedName && p.Id != dto.Id, cancellationToken);
        if (nameTaken)
        {
            return Result<int>.Failure($"Издательство «{trimmedName}» уже существует.");
        }

        Publisher entity;
        if (dto.Id == 0)
        {
            entity = new Publisher();
            _db.Publishers.Add(entity);
        }
        else
        {
            entity = await _db.Publishers.FirstOrDefaultAsync(p => p.Id == dto.Id, cancellationToken)
                ?? throw new InvalidOperationException($"Издательство Id={dto.Id} не найдено.");
            entity.UpdatedAt = DateTime.UtcNow;
        }

        entity.Name = trimmedName;
        entity.City = string.IsNullOrWhiteSpace(dto.City) ? null : dto.City.Trim();
        entity.Description = dto.Description;

        await _db.SaveChangesAsync(cancellationToken);
        return Result<int>.Success(entity.Id);
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Publishers.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (entity is null) return Result.Failure("Издательство не найдено.");

        // Книги останутся - PublisherId станет null благодаря OnDelete(SetNull) в конфигурации
        _db.Publishers.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
