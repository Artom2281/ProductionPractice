using FluentValidation;
using LibraryManagement.Application.Abstractions;
using LibraryManagement.Application.Common;
using LibraryManagement.Application.Dtos;
using LibraryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Application.Services;

public class ReaderService : IReaderService
{
    private readonly ILibraryDbContext _db;
    private readonly IValidator<ReaderDto> _validator;

    public ReaderService(ILibraryDbContext db, IValidator<ReaderDto> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task<IReadOnlyList<ReaderDto>> GetAllAsync(string? search = null, bool includeBlocked = true, CancellationToken cancellationToken = default)
    {
        var query = _db.Readers.AsNoTracking();

        if (!includeBlocked) query = query.Where(r => !r.IsBlocked);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(r =>
                EF.Functions.Like(r.CardNumber, pattern) ||
                EF.Functions.Like(r.LastName, pattern) ||
                EF.Functions.Like(r.FirstName, pattern) ||
                (r.Phone != null && EF.Functions.Like(r.Phone, pattern)) ||
                (r.Email != null && EF.Functions.Like(r.Email, pattern)));
        }

        return await query
            .OrderBy(r => r.LastName).ThenBy(r => r.FirstName)
            .Select(r => new ReaderDto
            {
                Id = r.Id,
                CardNumber = r.CardNumber,
                FirstName = r.FirstName,
                LastName = r.LastName,
                MiddleName = r.MiddleName,
                BirthDate = r.BirthDate,
                Phone = r.Phone,
                Email = r.Email,
                Address = r.Address,
                RegistrationDate = r.RegistrationDate,
                IsBlocked = r.IsBlocked
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<ReaderDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var r = await _db.Readers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (r is null) return null;
        return new ReaderDto
        {
            Id = r.Id,
            CardNumber = r.CardNumber,
            FirstName = r.FirstName,
            LastName = r.LastName,
            MiddleName = r.MiddleName,
            BirthDate = r.BirthDate,
            Phone = r.Phone,
            Email = r.Email,
            Address = r.Address,
            RegistrationDate = r.RegistrationDate,
            IsBlocked = r.IsBlocked
        };
    }

    public async Task<Result<int>> SaveAsync(ReaderDto dto, CancellationToken cancellationToken = default)
    {
        var validation = await _validator.ValidateAsync(dto, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<int>.ValidationFailure(validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var trimmedCard = dto.CardNumber.Trim();
        var cardTaken = await _db.Readers.AnyAsync(r => r.CardNumber == trimmedCard && r.Id != dto.Id, cancellationToken);
        if (cardTaken)
        {
            return Result<int>.Failure($"Читатель с номером билета \"{trimmedCard}\" уже зарегистрирован.");
        }

        Reader entity;
        if (dto.Id == 0)
        {
            entity = new Reader { RegistrationDate = DateTime.UtcNow };
            _db.Readers.Add(entity);
        }
        else
        {
            entity = await _db.Readers.FirstOrDefaultAsync(r => r.Id == dto.Id, cancellationToken)
                ?? throw new InvalidOperationException($"Читатель Id={dto.Id} не найден.");
            entity.UpdatedAt = DateTime.UtcNow;
        }

        entity.CardNumber = trimmedCard;
        entity.FirstName = dto.FirstName.Trim();
        entity.LastName = dto.LastName.Trim();
        entity.MiddleName = string.IsNullOrWhiteSpace(dto.MiddleName) ? null : dto.MiddleName.Trim();
        entity.BirthDate = dto.BirthDate;
        entity.Phone = string.IsNullOrWhiteSpace(dto.Phone) ? null : dto.Phone.Trim();
        entity.Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim();
        entity.Address = dto.Address;
        entity.IsBlocked = dto.IsBlocked;

        await _db.SaveChangesAsync(cancellationToken);
        return Result<int>.Success(entity.Id);
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Readers
            .Include(r => r.Loans)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (entity is null) return Result.Failure("Читатель не найден.");

        if (entity.Loans.Any(l => l.ReturnedAt == null))
        {
            return Result.Failure("Нельзя удалить читателя — у него есть невозвращённые книги.");
        }

        _db.Readers.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> SetBlockedAsync(int id, bool isBlocked, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Readers.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        if (entity is null) return Result.Failure("Читатель не найден.");

        entity.IsBlocked = isBlocked;
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
