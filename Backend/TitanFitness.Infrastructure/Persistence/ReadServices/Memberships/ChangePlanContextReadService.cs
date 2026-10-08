using Microsoft.EntityFrameworkCore;
using TitanFitness.Application.Memberships.Queries.GetChangePlanContext;
using TitanFitness.Domain.Enums;

namespace TitanFitness.Infrastructure.Persistence.ReadServices.Memberships;

public sealed class ChangePlanContextReadService
    : IChangePlanContextReadService
{
    private readonly TitanFitnessDbContext _dbContext;

    public ChangePlanContextReadService(
        TitanFitnessDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ChangePlanContextDto?> GetAsync(
        int membershipId,
        DateOnly asOfDate,
        CancellationToken cancellationToken = default)
    {
        var membership =
            await _dbContext.Memberships
                .AsNoTracking()
                .Include(x => x.Freezes)
                .FirstOrDefaultAsync(
                    x =>
                        x.MembershipId ==
                        membershipId,
                    cancellationToken);

        if (membership is null)
        {
            return null;
        }

        var member =
            await _dbContext.Members
                .AsNoTracking()
                .FirstAsync(
                    x =>
                        x.MemberId ==
                        membership.MemberId,
                    cancellationToken);

        var currentPlan =
            await _dbContext.Plans
                .AsNoTracking()
                .FirstAsync(
                    x =>
                        x.PlanId ==
                        membership.PlanId,
                    cancellationToken);

        var currentStatus =
            membership.GetEffectiveStatus(
                asOfDate);

        var atRenewalStartDate =
            currentStatus ==
            MembershipStatus.Expired
                ? asOfDate
                : membership.EndDate;

        var immediateStartDate =
            asOfDate;

        var homeBranchId =
            member.HomeBranchId;

        var plans =
            await _dbContext.Plans
                .AsNoTracking()
                .Where(plan =>
                    plan.IsPublished &&
                    plan.PlanId !=
                        membership.PlanId &&
                    (
                        !plan.AvailableBranches.Any() ||
                        plan.AvailableBranches.Any(
                            branch =>
                                branch.BranchId ==
                                homeBranchId)
                    ))
                .OrderBy(plan =>
                    plan.PlanName)
                .ToListAsync(
                    cancellationToken);

        var options =
            plans
                .Select(plan =>
                    new ChangePlanOptionDto(
                        plan.PlanId,
                        plan.PlanName,
                        plan.Price,
                        plan.DurationInMonths,
                        plan.MaximumFreezeDays,
                        plan.MaximumNumberOfFreezes,
                        plan.GuestPassQuota,
                        plan.AccessScope,
                        atRenewalStartDate.AddMonths(
                            plan.DurationInMonths),
                        immediateStartDate.AddMonths(
                            plan.DurationInMonths)))
                .ToList();

        return new ChangePlanContextDto(
            membership.MembershipId,
            member.MemberId,
            member.FullName,
            member.MembershipNumber,
            currentPlan.PlanId,
            currentPlan.PlanName,
            membership.AgreedTerms.PricePaid,
            membership.AgreedTerms.DurationInMonths,
            membership.AgreedTerms.MaximumFreezeDays,
            membership.AgreedTerms.MaximumNumberOfFreezes,
            membership.AgreedTerms.GuestPassQuota,
            membership.AgreedTerms.AccessScope,
            membership.StartDate,
            membership.EndDate,
            currentStatus,
            atRenewalStartDate,
            immediateStartDate,
            options);
    }
}