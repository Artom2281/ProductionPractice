using LibraryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryManagement.Data.Configurations;

public class ReaderConfiguration : IEntityTypeConfiguration<Reader>
{
    public void Configure(EntityTypeBuilder<Reader> builder)
    {
        builder.ToTable("Readers");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.CardNumber).HasMaxLength(20).IsRequired();
        builder.Property(r => r.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(r => r.LastName).HasMaxLength(100).IsRequired();
        builder.Property(r => r.MiddleName).HasMaxLength(100);
        builder.Property(r => r.Phone).HasMaxLength(30);
        builder.Property(r => r.Email).HasMaxLength(150);
        builder.Property(r => r.Address).HasMaxLength(300);

        builder.HasIndex(r => r.CardNumber).IsUnique();
        builder.HasIndex(r => new { r.LastName, r.FirstName });

        builder.Ignore(r => r.FullName);
    }
}
