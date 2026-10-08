using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TitanFitness.Domain.Entities;

namespace TitanFitness.Infrastructure.Persistence.Configurations;

public sealed class GuestPassConfiguration
    : IEntityTypeConfiguration<GuestPass>
{
    public void Configure(EntityTypeBuilder<GuestPass> builder)
    {
        builder.ToTable("GuestPasses");

        builder.HasKey(x => x.GuestPassId);

        builder.Property(x => x.MembershipId)
            .IsRequired();

        builder.Property(x => x.IssuedOn)
            .IsRequired();

        builder.Property(x => x.UsedOn);

        builder.Property(x => x.GuestName)
            .HasMaxLength(100);

        builder.HasIndex(x => new
        {
            x.MembershipId,
            x.IssuedOn
        });
    }
}