using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using TitanFitness.Application.Abstractions.Persistence;
using TitanFitness.Application.Abstractions.Storage;

using TitanFitness.Application.Branches.Queries.GetBranches;
using TitanFitness.Application.Branches.Queries.GetStudiosByBranch;

using TitanFitness.Application.ClassSessions.Queries.GetBookingContext;
using TitanFitness.Application.ClassSessions.Queries.GetClassSchedule;

using TitanFitness.Application.Dashboard.Queries.GetDashboard;

using TitanFitness.Application.Members.Queries.GetMemberDirectory;
using TitanFitness.Application.Members.Queries.GetMemberProfile;
using TitanFitness.Application.Members.Queries.SearchMembersLookup;

using TitanFitness.Application.Memberships.Queries.GetChangePlanContext;
using TitanFitness.Application.Memberships.Queries.GetFreezeContext;
using TitanFitness.Application.Memberships.Queries.GetGuestPasses;

using TitanFitness.Application.Plans.Queries.GetPlanCatalogue;
using TitanFitness.Application.Plans.Queries.GetPlanDetails;

using TitanFitness.Application.Trainers.Queries.GetTrainerDetails;
using TitanFitness.Application.Trainers.Queries.GetTrainerDirectory;
using TitanFitness.Application.Trainers.Queries.GetTrainerLookup;

using TitanFitness.Domain.Repositories;

using TitanFitness.Infrastructure.Persistence;
using TitanFitness.Infrastructure.Persistence.ReadServices.Branches;
using TitanFitness.Infrastructure.Persistence.ReadServices.ClassSessions;
using TitanFitness.Infrastructure.Persistence.ReadServices.Dashboard;
using TitanFitness.Infrastructure.Persistence.ReadServices.Members;
using TitanFitness.Infrastructure.Persistence.ReadServices.Memberships;
using TitanFitness.Infrastructure.Persistence.ReadServices.Plans;
using TitanFitness.Infrastructure.Persistence.ReadServices.Trainers;
using TitanFitness.Infrastructure.Persistence.Repositories;
using TitanFitness.Infrastructure.Storage;

namespace TitanFitness.Infrastructure.DependencyInjection;

public static class InfrastructureDependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'DefaultConnection' was not found.");
        }

        services.AddDbContext<TitanFitnessDbContext>(
            options =>
                options.UseSqlServer(connectionString));

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<
            ITransactionManager,
            TransactionManager>();

        services.AddScoped<
            IFileStorageService,
            LocalFileStorageService>();

        services.AddScoped<
            IBranchRepository,
            BranchRepository>();

        services.AddScoped<
            IMemberRepository,
            MemberRepository>();

        services.AddScoped<
            IPlanRepository,
            PlanRepository>();

        services.AddScoped<
            IMembershipRepository,
            MembershipRepository>();

        services.AddScoped<
            ICheckInRepository,
            CheckInRepository>();

        services.AddScoped<
            ITrainerRepository,
            TrainerRepository>();

        services.AddScoped<
            IClassSessionRepository,
            ClassSessionRepository>();

        services.AddScoped<
            IMemberDirectoryReadService,
            MemberDirectoryReadService>();

        services.AddScoped<
            IMemberProfileReadService,
            MemberProfileReadService>();

        services.AddScoped<
            IMemberLookupReadService,
            MemberLookupReadService>();

        services.AddScoped<
            IChangePlanContextReadService,
            ChangePlanContextReadService>();

        services.AddScoped<
            IFreezeContextReadService,
            FreezeContextReadService>();

        services.AddScoped<
            IGuestPassesReadService,
            GuestPassesReadService>();

        services.AddScoped<
            IClassScheduleReadService,
            ClassScheduleReadService>();

        services.AddScoped<
            IBookingContextReadService,
            BookingContextReadService>();

        services.AddScoped<
            IDashboardReadService,
            DashboardReadService>();

        services.AddScoped<
            ITrainerDirectoryReadService,
            TrainerDirectoryReadService>();

        services.AddScoped<
            ITrainerDetailsReadService,
            TrainerDetailsReadService>();

        services.AddScoped<
            ITrainerLookupReadService,
            TrainerLookupReadService>();

        services.AddScoped<
            IPlanCatalogueReadService,
            PlanCatalogueReadService>();

        services.AddScoped<
            IPlanDetailsReadService,
            PlanDetailsReadService>();

        services.AddScoped<
            IBranchesReadService,
            BranchesReadService>();

        services.AddScoped<
            IStudiosByBranchReadService,
            StudiosByBranchReadService>();

        return services;
    }
}