using FluentValidation;

using TitanFitness.API.Middleware;

using TitanFitness.Application.Bookings.Commands.BookSession;
using TitanFitness.Application.Bookings.Commands.CancelBooking;
using TitanFitness.Application.Bookings.Commands.MarkBookingAttended;
using TitanFitness.Application.Bookings.Commands.MarkBookingNoShow;
using System.Text.Json.Serialization;
using TitanFitness.Application.Branches.Queries.GetBranches;
using TitanFitness.Application.Branches.Queries.GetStudiosByBranch;

using TitanFitness.Application.CheckIns.Commands.CheckInMember;
using TitanFitness.Application.CheckIns.Commands.CheckOutMember;

using TitanFitness.Application.ClassSessions.Commands.CancelClassSession;
using TitanFitness.Application.ClassSessions.Commands.ScheduleClass;
using TitanFitness.Application.ClassSessions.Commands.UpdateClass;
using TitanFitness.Application.ClassSessions.Queries.GetBookingContext;
using TitanFitness.Application.ClassSessions.Queries.GetClassSchedule;

using TitanFitness.Application.Dashboard.Queries.GetDashboard;

using TitanFitness.Application.Members.Commands.CreateMember;
using TitanFitness.Application.Members.Commands.UpdateMember;
using TitanFitness.Application.Members.Queries.GetMemberDirectory;
using TitanFitness.Application.Members.Queries.GetMemberProfile;
using TitanFitness.Application.Members.Queries.SearchMembersLookup;

using TitanFitness.Application.Memberships.Commands.CancelMembership;
using TitanFitness.Application.Memberships.Commands.ChangeMembershipPlan;
using TitanFitness.Application.Memberships.Commands.FreezeMembership;
using TitanFitness.Application.Memberships.Commands.IssueGuestPass;
using TitanFitness.Application.Memberships.Commands.PurchaseMembership;
using TitanFitness.Application.Memberships.Commands.RenewMembership;
using TitanFitness.Application.Memberships.Commands.UseGuestPass;
using TitanFitness.Application.Memberships.Queries.GetChangePlanContext;
using TitanFitness.Application.Memberships.Queries.GetFreezeContext;
using TitanFitness.Application.Memberships.Queries.GetGuestPasses;

using TitanFitness.Application.Plans.Commands.CreatePlan;
using TitanFitness.Application.Plans.Commands.UpdatePlan;
using TitanFitness.Application.Plans.Queries.GetPlanCatalogue;
using TitanFitness.Application.Plans.Queries.GetPlanDetails;

using TitanFitness.Application.Trainers.Commands.CreateTrainer;
using TitanFitness.Application.Trainers.Commands.UpdateTrainer;
using TitanFitness.Application.Trainers.Queries.GetTrainerDetails;
using TitanFitness.Application.Trainers.Queries.GetTrainerDirectory;
using TitanFitness.Application.Trainers.Queries.GetTrainerLookup;

using TitanFitness.Domain.Services;

using TitanFitness.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter());
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

builder.Services.AddInfrastructure(
    builder.Configuration);


// ============================================================
// DOMAIN SERVICES
// ============================================================

builder.Services.AddScoped<MembershipOverlapChecker>();

builder.Services.AddScoped<CheckInEligibilityService>();

builder.Services.AddScoped<SessionConflictChecker>();

builder.Services.AddScoped<BookingEligibilityService>();


// ============================================================
// MEMBERS
// ============================================================

builder.Services.AddScoped<CreateMemberCommandHandler>();

builder.Services.AddScoped<UpdateMemberCommandHandler>();

builder.Services.AddScoped<GetMemberDirectoryQueryHandler>();

builder.Services.AddScoped<GetMemberProfileQueryHandler>();

builder.Services.AddScoped<SearchMembersLookupQueryHandler>();

builder.Services.AddScoped<
    IValidator<CreateMemberCommand>,
    CreateMemberCommandValidator>();

builder.Services.AddScoped<
    IValidator<UpdateMemberCommand>,
    UpdateMemberCommandValidator>();


// ============================================================
// MEMBERSHIPS
// ============================================================

builder.Services.AddScoped<PurchaseMembershipCommandHandler>();

builder.Services.AddScoped<RenewMembershipCommandHandler>();

builder.Services.AddScoped<ChangeMembershipPlanCommandHandler>();

builder.Services.AddScoped<CancelMembershipCommandHandler>();

builder.Services.AddScoped<FreezeMembershipCommandHandler>();

builder.Services.AddScoped<IssueGuestPassCommandHandler>();

builder.Services.AddScoped<UseGuestPassCommandHandler>();

builder.Services.AddScoped<GetChangePlanContextQueryHandler>();

builder.Services.AddScoped<GetFreezeContextQueryHandler>();

builder.Services.AddScoped<GetGuestPassesQueryHandler>();

builder.Services.AddScoped<
    IValidator<PurchaseMembershipCommand>,
    PurchaseMembershipCommandValidator>();

builder.Services.AddScoped<
    IValidator<RenewMembershipCommand>,
    RenewMembershipCommandValidator>();

builder.Services.AddScoped<
    IValidator<ChangeMembershipPlanCommand>,
    ChangeMembershipPlanCommandValidator>();

builder.Services.AddScoped<
    IValidator<CancelMembershipCommand>,
    CancelMembershipCommandValidator>();

builder.Services.AddScoped<
    IValidator<FreezeMembershipCommand>,
    FreezeMembershipCommandValidator>();

builder.Services.AddScoped<
    IValidator<IssueGuestPassCommand>,
    IssueGuestPassCommandValidator>();

builder.Services.AddScoped<
    IValidator<UseGuestPassCommand>,
    UseGuestPassCommandValidator>();


// ============================================================
// CHECK-INS
// ============================================================

builder.Services.AddScoped<CheckInMemberCommandHandler>();

builder.Services.AddScoped<CheckOutMemberCommandHandler>();

builder.Services.AddScoped<
    IValidator<CheckInMemberCommand>,
    CheckInMemberCommandValidator>();

builder.Services.AddScoped<
    IValidator<CheckOutMemberCommand>,
    CheckOutMemberCommandValidator>();


// ============================================================
// CLASS SESSIONS
// ============================================================

builder.Services.AddScoped<ScheduleClassCommandHandler>();

builder.Services.AddScoped<CancelClassSessionCommandHandler>();

builder.Services.AddScoped<UpdateClassCommandHandler>();

builder.Services.AddScoped<GetClassScheduleQueryHandler>();

builder.Services.AddScoped<GetBookingContextQueryHandler>();

builder.Services.AddScoped<
    IValidator<ScheduleClassCommand>,
    ScheduleClassCommandValidator>();

builder.Services.AddScoped<
    IValidator<CancelClassSessionCommand>,
    CancelClassSessionCommandValidator>();

builder.Services.AddScoped<
    IValidator<UpdateClassCommand>,
    UpdateClassCommandValidator>();


// ============================================================
// BOOKINGS
// ============================================================

builder.Services.AddScoped<BookSessionCommandHandler>();

builder.Services.AddScoped<CancelBookingCommandHandler>();

builder.Services.AddScoped<MarkBookingAttendedCommandHandler>();

builder.Services.AddScoped<MarkBookingNoShowCommandHandler>();

builder.Services.AddScoped<
    IValidator<BookSessionCommand>,
    BookSessionCommandValidator>();

builder.Services.AddScoped<
    IValidator<CancelBookingCommand>,
    CancelBookingCommandValidator>();

builder.Services.AddScoped<
    IValidator<MarkBookingAttendedCommand>,
    MarkBookingAttendedCommandValidator>();

builder.Services.AddScoped<
    IValidator<MarkBookingNoShowCommand>,
    MarkBookingNoShowCommandValidator>();


// ============================================================
// TRAINERS
// ============================================================

builder.Services.AddScoped<CreateTrainerCommandHandler>();

builder.Services.AddScoped<UpdateTrainerCommandHandler>();

builder.Services.AddScoped<GetTrainerDirectoryQueryHandler>();

builder.Services.AddScoped<GetTrainerDetailsQueryHandler>();

builder.Services.AddScoped<GetTrainerLookupQueryHandler>();

builder.Services.AddScoped<
    IValidator<CreateTrainerCommand>,
    CreateTrainerCommandValidator>();

builder.Services.AddScoped<
    IValidator<UpdateTrainerCommand>,
    UpdateTrainerCommandValidator>();


// ============================================================
// PLANS
// ============================================================

builder.Services.AddScoped<CreatePlanCommandHandler>();

builder.Services.AddScoped<UpdatePlanCommandHandler>();

builder.Services.AddScoped<GetPlanCatalogueQueryHandler>();

builder.Services.AddScoped<GetPlanDetailsQueryHandler>();

builder.Services.AddScoped<
    IValidator<CreatePlanCommand>,
    CreatePlanCommandValidator>();

builder.Services.AddScoped<
    IValidator<UpdatePlanCommand>,
    UpdatePlanCommandValidator>();


// ============================================================
// BRANCHES
// ============================================================

builder.Services.AddScoped<GetBranchesQueryHandler>();

builder.Services.AddScoped<GetStudiosByBranchQueryHandler>();


// ============================================================
// DASHBOARD
// ============================================================

builder.Services.AddScoped<GetDashboardQueryHandler>();


// ============================================================
// BUILD APPLICATION
// ============================================================

var app = builder.Build();

app.UseCors("Frontend");

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();

app.Run();