using LibraryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryManagement.Data.Configurations;

public class AuthorConfiguration : IEntityTypeConfiguration<Author>
{
    public void Configure(EntityTypeBuilder<Author> builder)
    {
        builder.ToTable("Authors");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(a => a.LastName).HasMaxLength(100).IsRequired();
        builder.Property(a => a.MiddleName).HasMaxLength(100);
        builder.Property(a => a.Biography).HasMaxLength(4000);

        builder.HasIndex(a => new { a.LastName, a.FirstName });

        // FullName рассчитывается в C#, не маппим в БД
        builder.Ignore(a => a.FullName);
    }
}
