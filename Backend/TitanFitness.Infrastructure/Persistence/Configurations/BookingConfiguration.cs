using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TitanFitness.Domain.Entities;

namespace TitanFitness.Infrastructure.Persistence.Configurations;

public sealed class BookingConfiguration
    : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("Bookings");

        builder.HasKey(x => x.BookingId);

        builder.Property(x => x.SessionId)
            .IsRequired();

        builder.Property(x => x.MemberId)
            .IsRequired();

        builder.Property(x => x.BookedOn)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.WaitlistPosition);

        builder.Property(x => x.NotesForTrainer)
            .HasMaxLength(500);

        builder.HasOne<Member>()
            .WithMany()
            .HasForeignKey(x => x.MemberId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new
        {
            x.SessionId,
            x.MemberId
        });

        builder.HasIndex(x => new
        {
            x.SessionId,
            x.Status,
            x.WaitlistPosition
        });
    }
}