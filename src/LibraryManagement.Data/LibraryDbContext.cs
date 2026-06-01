using LibraryManagement.Application.Abstractions;
using LibraryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace LibraryManagement.Data;

// Реализация ILibraryDbContext поверх EF Core SQLite. Все настройки таблиц - в Configurations/.
public class LibraryDbContext : DbContext, ILibraryDbContext
{
    public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options)
    {
    }

    public DbSet<Author> Authors => Set<Author>();
    public DbSet<Genre> Genres => Set<Genre>();
    public DbSet<Publisher> Publishers => Set<Publisher>();
    public DbSet<Book> Books => Set<Book>();
    public DbSet<Reader> Readers => Set<Reader>();
    public DbSet<Loan> Loans => Set<Loan>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
