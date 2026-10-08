using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TitanFitness.Domain.Entities;

namespace TitanFitness.Infrastructure.Persistence.Configurations;

public sealed class FreezeConfiguration
    : IEntityTypeConfiguration<Freeze>
{
    public void Configure(EntityTypeBuilder<Freeze> builder)
    {
        builder.ToTable("Freezes");

        builder.HasKey(x => x.FreezeId);

        builder.Property(x => x.MembershipId)
            .IsRequired();

        builder.Property(x => x.StartDate)
            .IsRequired();

        builder.Property(x => x.EndDate)
            .IsRequired();

        builder.Property(x => x.DurationInMonths)
            .IsRequired();

        builder.Property(x => x.Reason)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.AdditionalNotes)
            .HasMaxLength(500);

        builder.Property(x => x.RequestedOn)
            .IsRequired();

        builder.HasIndex(x => new
        {
            x.MembershipId,
            x.StartDate
        });
    }
}