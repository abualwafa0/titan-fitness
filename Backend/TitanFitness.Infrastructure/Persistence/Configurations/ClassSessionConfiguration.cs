using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TitanFitness.Domain.Entities;

namespace TitanFitness.Infrastructure.Persistence.Configurations;

public sealed class ClassSessionConfiguration
    : IEntityTypeConfiguration<ClassSession>
{
    public void Configure(
        EntityTypeBuilder<ClassSession> builder)
    {
        builder.ToTable("ClassSessions");

        builder.HasKey(x =>
            x.SessionId);

        builder.Property(x =>
                x.ClassName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x =>
                x.BranchId)
            .IsRequired();

        builder.Property(x =>
                x.StudioId)
            .IsRequired(false);

        builder.Property(x =>
                x.TrainerId)
            .IsRequired(false);

        builder.Property(x =>
                x.SessionDate)
            .IsRequired();

        builder.Property(x =>
                x.StartTime)
            .IsRequired();

        builder.Property(x =>
                x.DurationInMinutes)
            .IsRequired();

        builder.Property(x =>
                x.CapacityLimit)
            .IsRequired();

        builder.Property(x =>
                x.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x =>
                x.Description)
            .HasMaxLength(500);

        builder.HasOne<Branch>()
            .WithMany()
            .HasForeignKey(x =>
                x.BranchId)
            .OnDelete(
                DeleteBehavior.Restrict);

        builder.HasOne<Studio>()
            .WithMany()
            .HasForeignKey(x =>
                x.StudioId)
            .OnDelete(
                DeleteBehavior.Restrict);

        builder.HasOne<Trainer>()
            .WithMany()
            .HasForeignKey(x =>
                x.TrainerId)
            .IsRequired(false)
            .OnDelete(
                DeleteBehavior.SetNull);

        builder.HasMany(x =>
                x.Bookings)
            .WithOne()
            .HasForeignKey(x =>
                x.SessionId)
            .OnDelete(
                DeleteBehavior.Restrict);

        builder.Navigation(x =>
                x.Bookings)
            .UsePropertyAccessMode(
                PropertyAccessMode.Field);

        builder.HasIndex(x =>
            new
            {
                x.BranchId,
                x.SessionDate
            });

        builder.HasIndex(x =>
            new
            {
                x.TrainerId,
                x.SessionDate,
                x.StartTime
            });

        builder.HasIndex(x =>
            new
            {
                x.StudioId,
                x.SessionDate,
                x.StartTime
            });
    }
}