using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TitanFitness.Domain.Entities;

namespace TitanFitness.Infrastructure.Persistence.Configurations;

public sealed class StudioConfiguration
    : IEntityTypeConfiguration<Studio>
{
    public void Configure(
        EntityTypeBuilder<Studio> builder)
    {
        builder.ToTable("Studios");

        builder.HasKey(x => x.StudioId);

        builder.Property(x => x.StudioName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Capacity)
            .IsRequired();

        builder.Property(x => x.BranchId)
            .IsRequired();

        builder.HasIndex(x => new
        {
            x.BranchId,
            x.StudioName
        });
    }
}