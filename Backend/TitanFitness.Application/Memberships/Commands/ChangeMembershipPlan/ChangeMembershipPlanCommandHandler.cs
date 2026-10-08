using TitanFitness.Application.Abstractions.Persistence;
using TitanFitness.Application.Common.Exceptions;
using TitanFitness.Domain.Entities;
using TitanFitness.Domain.Enums;
using TitanFitness.Domain.Repositories;
using TitanFitness.Domain.Services;

namespace TitanFitness.Application.Memberships.Commands.ChangeMembershipPlan;

public sealed class ChangeMembershipPlanCommandHandler
{
    private readonly IMembershipRepository _membershipRepository;
    private readonly IMemberRepository _memberRepository;
    private readonly IPlanRepository _planRepository;
    private readonly MembershipOverlapChecker _membershipOverlapChecker;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITransactionManager _transactionManager;

    public ChangeMembershipPlanCommandHandler(
        IMembershipRepository membershipRepository,
        IMemberRepository memberRepository,
        IPlanRepository planRepository,
        MembershipOverlapChecker membershipOverlapChecker,
        IUnitOfWork unitOfWork,
        ITransactionManager transactionManager)
    {
        _membershipRepository = membershipRepository;
        _memberRepository = memberRepository;
        _planRepository = planRepository;
        _membershipOverlapChecker = membershipOverlapChecker;
        _unitOfWork = unitOfWork;
        _transactionManager = transactionManager;
    }

    public async Task<ChangeMembershipPlanResult> Handle(
        ChangeMembershipPlanCommand command,
        CancellationToken cancellationToken = default)
    {
        var currentMembership =
            await _membershipRepository.GetByIdAsync(
                command.MembershipId,
                cancellationToken);

        if (currentMembership is null)
        {
            throw new NotFoundException(
                $"Membership with id {command.MembershipId} was not found.");
        }

        var member =
            await _memberRepository.GetByIdAsync(
                currentMembership.MemberId,
                cancellationToken);

        if (member is null)
        {
            throw new NotFoundException(
                $"Member with id {currentMembership.MemberId} was not found.");
        }

        var now =
            DateTime.Now;

        var today =
            DateOnly.FromDateTime(now);

        var effectiveStatus =
            currentMembership.GetEffectiveStatus(
                today);

        if (effectiveStatus ==
            MembershipStatus.Cancelled)
        {
            throw new ConflictException(
                "A cancelled membership cannot be changed.");
        }

        var newPlan =
            await _planRepository.GetByIdAsync(
                command.NewPlanId,
                cancellationToken);

        if (newPlan is null)
        {
            throw new NotFoundException(
                $"Plan with id {command.NewPlanId} was not found.");
        }

        if (!newPlan.IsPublished)
        {
            throw new ConflictException(
                "An unpublished plan cannot be selected.");
        }

        if (!newPlan.IsAvailableAtBranch(
                member.HomeBranchId))
        {
            throw new ConflictException(
                "The selected plan is not available at the member's home branch.");
        }

        if (newPlan.PlanId ==
            currentMembership.PlanId)
        {
            throw new ConflictException(
                "The selected plan is already the membership's current plan.");
        }

        return command.Timing switch
        {
            PlanChangeTiming.AtRenewal =>
                await ChangeAtRenewalAsync(
                    currentMembership,
                    newPlan,
                    effectiveStatus,
                    now,
                    today,
                    cancellationToken),

            PlanChangeTiming.Immediately =>
                await ChangeImmediatelyAsync(
                    currentMembership,
                    newPlan,
                    effectiveStatus,
                    now,
                    today,
                    cancellationToken),

            _ =>
                throw new ArgumentOutOfRangeException(
                    nameof(command.Timing),
                    "Invalid plan change timing.")
        };
    }

    private async Task<ChangeMembershipPlanResult>
        ChangeAtRenewalAsync(
            Membership currentMembership,
            Plan newPlan,
            MembershipStatus effectiveStatus,
            DateTime purchaseDate,
            DateOnly today,
            CancellationToken cancellationToken)
    {
        var newStartDate =
            effectiveStatus ==
            MembershipStatus.Expired
                ? today
                : currentMembership.EndDate;

        var newMembership =
            Membership.Purchase(
                currentMembership.MemberId,
                newPlan,
                purchaseDate,
                newStartDate);

        await EnsureNoOverlapAsync(
            newMembership,
            cancellationToken);

        await _membershipRepository.AddAsync(
            newMembership,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return ToResult(
            newMembership);
    }

    private async Task<ChangeMembershipPlanResult>
        ChangeImmediatelyAsync(
            Membership currentMembership,
            Plan newPlan,
            MembershipStatus effectiveStatus,
            DateTime purchaseDate,
            DateOnly today,
            CancellationToken cancellationToken)
    {
        if (effectiveStatus ==
            MembershipStatus.Expired)
        {
            var newMembership =
                Membership.Purchase(
                    currentMembership.MemberId,
                    newPlan,
                    purchaseDate,
                    today);

            await EnsureNoOverlapAsync(
                newMembership,
                cancellationToken);

            await _membershipRepository.AddAsync(
                newMembership,
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(
                cancellationToken);

            return ToResult(
                newMembership);
        }

        ChangeMembershipPlanResult?
            result = null;

        await _transactionManager.ExecuteAsync(
            async ct =>
            {
                currentMembership.Cancel(
                    today);

                var newMembership =
                    Membership.Purchase(
                        currentMembership.MemberId,
                        newPlan,
                        purchaseDate,
                        today);

                await EnsureNoOverlapAsync(
                    newMembership,
                    ct,
                    currentMembership.MembershipId);

                await _membershipRepository.AddAsync(
                    newMembership,
                    ct);

                await _unitOfWork.SaveChangesAsync(
                    ct);

                result =
                    ToResult(
                        newMembership);
            },
            cancellationToken);

        return result
            ?? throw new InvalidOperationException(
                "The plan change transaction did not complete.");
    }

    private async Task EnsureNoOverlapAsync(
        Membership membership,
        CancellationToken cancellationToken,
        int? excludeMembershipId = null)
    {
        try
        {
            await _membershipOverlapChecker
                .EnsureNoOverlapAsync(
                    membership.MemberId,
                    membership.StartDate,
                    membership.EndDate,
                    excludeMembershipId,
                    cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            throw new ConflictException(
                ex.Message);
        }
    }

    private static ChangeMembershipPlanResult ToResult(
        Membership membership)
    {
        return new ChangeMembershipPlanResult(
            membership.MembershipId,
            membership.MemberId,
            membership.PlanId,
            membership.StartDate,
            membership.EndDate);
    }
}