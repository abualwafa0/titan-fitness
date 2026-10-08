using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TitanFitness.Domain.Entities;

namespace TitanFitness.Infrastructure.Persistence.Configurations;

public sealed class MemberConfiguration
    : IEntityTypeConfiguration<Member>
{
    public void Configure(
        EntityTypeBuilder<Member> builder)
    {
        builder.ToTable("Members");

        builder.HasKey(x =>
            x.MemberId);

        builder.Property(x =>
                x.MembershipNumber)
            .IsRequired()
            .HasMaxLength(10);

        builder.HasIndex(x =>
                x.MembershipNumber)
            .IsUnique();

        builder.Property(x =>
                x.FullName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x =>
                x.Email)
            .HasMaxLength(100);

        builder.Property(x =>
                x.Phone)
            .HasMaxLength(20);

        builder.Property(x =>
                x.Address)
            .HasMaxLength(200);

        builder.Property(x =>
                x.JoinedDate)
            .IsRequired();

        builder.Property(x =>
            x.Photo);

        builder.Property(x =>
                x.HomeBranchId)
            .IsRequired();

        builder.HasOne<Branch>()
            .WithMany()
            .HasForeignKey(x =>
                x.HomeBranchId)
            .OnDelete(
                DeleteBehavior.Restrict);
    }
}