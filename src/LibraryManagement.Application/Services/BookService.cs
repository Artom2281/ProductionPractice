using FluentValidation;
using LibraryManagement.Application.Abstractions;
using LibraryManagement.Application.Common;
using LibraryManagement.Application.Dtos;
using LibraryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Application.Services;

public class BookService : IBookService
{
    private readonly ILibraryDbContext _db;
    private readonly IValidator<BookDto> _validator;

    public BookService(ILibraryDbContext db, IValidator<BookDto> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task<IReadOnlyList<BookDto>> GetAllAsync(string? search = null, int? authorId = null, int? genreId = null, bool onlyAvailable = false, CancellationToken cancellationToken = default)
    {
        var query = _db.Books
            .AsNoTracking()
            .Include(b => b.Author)
            .Include(b => b.Genre)
            .Include(b => b.Publisher)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(b =>
                EF.Functions.Like(b.Title, pattern) ||
                (b.Isbn != null && EF.Functions.Like(b.Isbn, pattern)) ||
                EF.Functions.Like(b.Author.LastName, pattern) ||
                EF.Functions.Like(b.Author.FirstName, pattern));
        }

        if (authorId.HasValue) query = query.Where(b => b.AuthorId == authorId.Value);
        if (genreId.HasValue) query = query.Where(b => b.GenreId == genreId.Value);
        if (onlyAvailable) query = query.Where(b => b.AvailableCopies > 0);

        return await query
            .OrderBy(b => b.Title)
            .Select(b => new BookDto
            {
                Id = b.Id,
                Title = b.Title,
                Isbn = b.Isbn,
                PublicationDate = b.PublicationDate,
                TotalCopies = b.TotalCopies,
                AvailableCopies = b.AvailableCopies,
                Description = b.Description,
                AuthorId = b.AuthorId,
                AuthorFullName = (b.Author.MiddleName == null || b.Author.MiddleName == "")
                    ? b.Author.LastName + " " + b.Author.FirstName
                    : b.Author.LastName + " " + b.Author.FirstName + " " + b.Author.MiddleName,
                GenreId = b.GenreId,
                GenreName = b.Genre.Name,
                PublisherId = b.PublisherId,
                PublisherName = b.Publisher != null ? b.Publisher.Name : null
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<BookDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var b = await _db.Books
            .AsNoTracking()
            .Include(x => x.Author)
            .Include(x => x.Genre)
            .Include(x => x.Publisher)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (b is null) return null;

        return new BookDto
        {
            Id = b.Id,
            Title = b.Title,
            Isbn = b.Isbn,
            PublicationDate = b.PublicationDate,
            TotalCopies = b.TotalCopies,
            AvailableCopies = b.AvailableCopies,
            Description = b.Description,
            AuthorId = b.AuthorId,
            AuthorFullName = b.Author.FullName,
            GenreId = b.GenreId,
            GenreName = b.Genre.Name,
            PublisherId = b.PublisherId,
            PublisherName = b.Publisher?.Name
        };
    }

    public async Task<Result<int>> SaveAsync(BookDto dto, CancellationToken cancellationToken = default)
    {
        var validation = await _validator.ValidateAsync(dto, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<int>.ValidationFailure(validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var authorExists = await _db.Authors.AnyAsync(a => a.Id == dto.AuthorId, cancellationToken);
        if (!authorExists) return Result<int>.Failure("Указанный автор не найден.");

        var genreExists = await _db.Genres.AnyAsync(g => g.Id == dto.GenreId, cancellationToken);
        if (!genreExists) return Result<int>.Failure("Указанный жанр не найден.");

        if (dto.PublisherId.HasValue)
        {
            var publisherExists = await _db.Publishers.AnyAsync(p => p.Id == dto.PublisherId.Value, cancellationToken);
            if (!publisherExists) return Result<int>.Failure("Указанное издательство не найдено.");
        }

        Book entity;
        if (dto.Id == 0)
        {
            entity = new Book
            {
                AvailableCopies = dto.TotalCopies // при создании все экземпляры доступны
            };
            _db.Books.Add(entity);
        }
        else
        {
            entity = await _db.Books.FirstOrDefaultAsync(b => b.Id == dto.Id, cancellationToken)
                ?? throw new InvalidOperationException($"Книга Id={dto.Id} не найдена.");

            // Защита от рассинхрона: на руках сейчас (TotalCopies - AvailableCopies) экземпляров.
            // Уменьшение TotalCopies ниже этого числа создало бы отрицательное "доступно",
            // которое нынешний код прятал через Math.Max(0, ...) - это маскировало баг.
            var inHands = entity.TotalCopies - entity.AvailableCopies;
            if (dto.TotalCopies < inHands)
            {
                return Result<int>.Failure($"Нельзя уменьшить количество экземпляров до {dto.TotalCopies}: на руках у читателей {inHands}.");
            }

            var delta = dto.TotalCopies - entity.TotalCopies;
            entity.AvailableCopies = entity.AvailableCopies + delta;
            entity.UpdatedAt = DateTime.UtcNow;
        }

        entity.Title = dto.Title.Trim();
        entity.Isbn = string.IsNullOrWhiteSpace(dto.Isbn) ? null : dto.Isbn.Trim();
        entity.PublicationDate = dto.PublicationDate;
        entity.TotalCopies = dto.TotalCopies;
        entity.Description = dto.Description;
        entity.AuthorId = dto.AuthorId;
        entity.GenreId = dto.GenreId;
        entity.PublisherId = dto.PublisherId;

        await _db.SaveChangesAsync(cancellationToken);
        return Result<int>.Success(entity.Id);
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Books
            .Include(b => b.Loans)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

        if (entity is null) return Result.Failure("Книга не найдена.");
        if (entity.Loans.Any(l => l.ReturnedAt == null))
        {
            return Result.Failure("Нельзя удалить книгу — есть невозвращённые экземпляры на руках.");
        }

        _db.Books.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
