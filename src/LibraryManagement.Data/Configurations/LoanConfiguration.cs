using LibraryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryManagement.Data.Configurations;

public class LoanConfiguration : IEntityTypeConfiguration<Loan>
{
    public void Configure(EntityTypeBuilder<Loan> builder)
    {
        builder.ToTable("Loans");
        builder.HasKey(l => l.Id);

        builder.Property(l => l.LoanDate).IsRequired();
        builder.Property(l => l.DueDate).IsRequired();
        builder.Property(l => l.Status).IsRequired();
        builder.Property(l => l.FineAmount).HasColumnType("decimal(10,2)");
        builder.Property(l => l.Notes).HasMaxLength(1000);

        builder.HasOne(l => l.Book)
            .WithMany(b => b.Loans)
            .HasForeignKey(l => l.BookId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.Reader)
            .WithMany(r => r.Loans)
            .HasForeignKey(l => l.ReaderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.IssuedByUser)
            .WithMany(u => u.IssuedLoans)
            .HasForeignKey(l => l.IssuedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(l => l.LoanDate);
        builder.HasIndex(l => l.DueDate);
        builder.HasIndex(l => new { l.ReaderId, l.ReturnedAt });
    }
}
