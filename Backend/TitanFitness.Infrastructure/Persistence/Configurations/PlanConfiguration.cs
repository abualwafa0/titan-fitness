using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TitanFitness.Domain.Entities;

namespace TitanFitness.Infrastructure.Persistence.Configurations;

public sealed class PlanConfiguration
    : IEntityTypeConfiguration<Plan>
{
    public void Configure(
        EntityTypeBuilder<Plan> builder)
    {
        builder.ToTable("Plans");

        builder.HasKey(x => x.PlanId);

        builder.Property(x => x.PlanName)
            .IsRequired()
            .HasMaxLength(60);

        builder.Property(x => x.Price)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(x => x.DurationInMonths)
            .IsRequired();

        builder.Property(x => x.MaximumFreezeDays)
            .IsRequired();

        builder.Property(x => x.MaximumNumberOfFreezes)
            .IsRequired();

        builder.Property(x => x.GuestPassQuota)
            .IsRequired();

        builder.Property(x => x.AccessScope)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.IsPublished)
            .IsRequired();

        builder.HasMany(x => x.AvailableBranches)
            .WithOne()
            .HasForeignKey(x => x.PlanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.AvailableBranches)
            .UsePropertyAccessMode(
                PropertyAccessMode.Field);
    }
}