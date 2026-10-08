using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TitanFitness.Domain.Entities;

namespace TitanFitness.Infrastructure.Persistence.Configurations;

public sealed class MembershipConfiguration
    : IEntityTypeConfiguration<Membership>
{
    public void Configure(EntityTypeBuilder<Membership> builder)
    {
        builder.ToTable("Memberships");

        builder.HasKey(x => x.MembershipId);

        builder.Property(x => x.MemberId)
            .IsRequired();

        builder.Property(x => x.PlanId)
            .IsRequired();

        builder.Property(x => x.PurchaseDate)
            .IsRequired();

        builder.Property(x => x.StartDate)
            .IsRequired();

        builder.Property(x => x.EndDate)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.HasOne<Member>()
            .WithMany()
            .HasForeignKey(x => x.MemberId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Plan>()
            .WithMany()
            .HasForeignKey(x => x.PlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.OwnsOne(
            x => x.AgreedTerms,
            agreedTerms =>
            {
                agreedTerms.Property(x => x.PricePaid)
                    .HasColumnName("AgreedPricePaid")
                    .IsRequired()
                    .HasPrecision(18, 2);

                agreedTerms.Property(x => x.DurationInMonths)
                    .HasColumnName("AgreedDurationInMonths")
                    .IsRequired();

                agreedTerms.Property(x => x.MaximumFreezeDays)
                    .HasColumnName("AgreedMaximumFreezeDays")
                    .IsRequired();

                agreedTerms.Property(x => x.MaximumNumberOfFreezes)
                    .HasColumnName("AgreedMaximumNumberOfFreezes")
                    .IsRequired();

                agreedTerms.Property(x => x.GuestPassQuota)
                    .HasColumnName("AgreedGuestPassQuota")
                    .IsRequired();

                agreedTerms.Property(x => x.AccessScope)
                    .HasColumnName("AgreedAccessScope")
                    .IsRequired()
                    .HasConversion<int>();
            });

        builder.HasMany(x => x.Freezes)
            .WithOne()
            .HasForeignKey(x => x.MembershipId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.GuestPasses)
            .WithOne()
            .HasForeignKey(x => x.MembershipId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(x => x.Freezes)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(x => x.GuestPasses)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(x => new
        {
            x.MemberId,
            x.StartDate,
            x.EndDate
        });
    }
}