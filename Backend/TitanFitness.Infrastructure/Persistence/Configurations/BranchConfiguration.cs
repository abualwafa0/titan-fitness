using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TitanFitness.Domain.Entities;

namespace TitanFitness.Infrastructure.Persistence.Configurations;

public sealed class BranchConfiguration
    : IEntityTypeConfiguration<Branch>
{
    public void Configure(
        EntityTypeBuilder<Branch> builder)
    {
        builder.ToTable("Branches");

        builder.HasKey(x => x.BranchId);

        builder.Property(x => x.BranchName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Address)
            .HasMaxLength(200);

        builder.Property(x => x.OpeningTime)
            .IsRequired();

        builder.Property(x => x.ClosingTime)
            .IsRequired();

        builder.HasMany(x => x.Studios)
            .WithOne()
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(x => x.Studios)
            .UsePropertyAccessMode(
                PropertyAccessMode.Field);
    }
}