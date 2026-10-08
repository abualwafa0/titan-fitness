using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TitanFitness.Domain.Entities;

namespace TitanFitness.Infrastructure.Persistence.Configurations;

public sealed class TrainerConfiguration
    : IEntityTypeConfiguration<Trainer>
{
    public void Configure(
        EntityTypeBuilder<Trainer> builder)
    {
        builder.ToTable("Trainers");

        builder.HasKey(x =>
            x.TrainerId);

        builder.Property(x =>
                x.TrainerNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(x =>
                x.TrainerNumber)
            .IsUnique();

        builder.Property(x =>
                x.TrainerName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x =>
            x.Specialty);

        builder.Property(x =>
                x.Email)
            .HasMaxLength(100);

        builder.Property(x =>
                x.Phone)
            .HasMaxLength(20);

        builder.Property(x =>
                x.BranchId)
            .IsRequired();

        builder.Property(x =>
                x.IsActive)
            .IsRequired();

        builder.HasOne<Branch>()
            .WithMany()
            .HasForeignKey(x =>
                x.BranchId)
            .OnDelete(
                DeleteBehavior.Restrict);

        builder.HasIndex(x => new
        {
            x.BranchId,
            x.IsActive
        });
    }
}