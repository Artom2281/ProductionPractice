using LibraryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Application.Abstractions;

// Контракт доступа к данным. Реализуется в слое Data классом LibraryDbContext.
// Объявление в Application позволяет сервисам не зависеть от конкретного DbContext.
public interface ILibraryDbContext
{
    DbSet<Author> Authors { get; }
    DbSet<Genre> Genres { get; }
    DbSet<Publisher> Publishers { get; }
    DbSet<Book> Books { get; }
    DbSet<Reader> Readers { get; }
    DbSet<Loan> Loans { get; }
    DbSet<User> Users { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
