using Microsoft.EntityFrameworkCore;
using TitanFitness.Domain.Entities;

namespace TitanFitness.Infrastructure.Persistence;

public sealed class TitanFitnessDbContext
    : DbContext
{
    public TitanFitnessDbContext(
        DbContextOptions<TitanFitnessDbContext> options)
        : base(options)
    {
    }

    public DbSet<Branch> Branches =>
        Set<Branch>();

    public DbSet<Studio> Studios =>
        Set<Studio>();

    public DbSet<Member> Members =>
        Set<Member>();

    public DbSet<Plan> Plans =>
        Set<Plan>();

    public DbSet<PlanBranch> PlanBranches =>
        Set<PlanBranch>();

    public DbSet<Membership> Memberships =>
        Set<Membership>();

    public DbSet<Freeze> Freezes =>
        Set<Freeze>();

    public DbSet<GuestPass> GuestPasses =>
        Set<GuestPass>();

    public DbSet<CheckIn> CheckIns =>
        Set<CheckIn>();

    public DbSet<Trainer> Trainers =>
        Set<Trainer>();

    public DbSet<ClassSession> ClassSessions =>
        Set<ClassSession>();

    public DbSet<Booking> Bookings =>
        Set<Booking>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(TitanFitnessDbContext).Assembly);
    }
}