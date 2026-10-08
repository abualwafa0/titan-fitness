using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TitanFitness.Domain.Entities;

namespace TitanFitness.Infrastructure.Persistence.Configurations;

public sealed class PlanBranchConfiguration
    : IEntityTypeConfiguration<PlanBranch>
{
    public void Configure(
        EntityTypeBuilder<PlanBranch> builder)
    {
        builder.ToTable("PlanBranches");

        builder.HasKey(x => new
        {
            x.PlanId,
            x.BranchId
        });

        builder.Property(x => x.PlanId)
            .IsRequired();

        builder.Property(x => x.BranchId)
            .IsRequired();

        builder.HasOne<Branch>()
            .WithMany()
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.BranchId);
    }
}