using LibraryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryManagement.Data.Configurations;

public class BookConfiguration : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        builder.ToTable("Books");
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Title).HasMaxLength(300).IsRequired();
        builder.Property(b => b.Isbn).HasMaxLength(20);
        builder.Property(b => b.Description).HasMaxLength(2000);
        builder.Property(b => b.TotalCopies).IsRequired();
        builder.Property(b => b.AvailableCopies).IsRequired();

        builder.HasIndex(b => b.Title);
        builder.HasIndex(b => b.Isbn);

        builder.HasOne(b => b.Author)
            .WithMany(a => a.Books)
            .HasForeignKey(b => b.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.Genre)
            .WithMany(g => g.Books)
            .HasForeignKey(b => b.GenreId)
            .OnDelete(DeleteBehavior.Restrict);

        // Издательство опционально - не каждая книга его имеет.
        // SetNull при удалении издательства, чтобы не каскадно тереть книги.
        builder.HasOne(b => b.Publisher)
            .WithMany(p => p.Books)
            .HasForeignKey(b => b.PublisherId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
