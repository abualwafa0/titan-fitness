using TitanFitness.Application.Abstractions.Persistence;
using TitanFitness.Application.Common.Exceptions;
using TitanFitness.Domain.Entities;
using TitanFitness.Domain.Enums;
using TitanFitness.Domain.Repositories;
using TitanFitness.Domain.Services;

namespace TitanFitness.Application.Memberships.Commands.RenewMembership;

public sealed class RenewMembershipCommandHandler
{
    private readonly IMembershipRepository _membershipRepository;
    private readonly IPlanRepository _planRepository;
    private readonly MembershipOverlapChecker _membershipOverlapChecker;
    private readonly IUnitOfWork _unitOfWork;

    public RenewMembershipCommandHandler(
        IMembershipRepository membershipRepository,
        IPlanRepository planRepository,
        MembershipOverlapChecker membershipOverlapChecker,
        IUnitOfWork unitOfWork)
    {
        _membershipRepository = membershipRepository;
        _planRepository = planRepository;
        _membershipOverlapChecker = membershipOverlapChecker;
        _unitOfWork = unitOfWork;
    }

    public async Task<RenewMembershipResult> Handle(
        RenewMembershipCommand command,
        CancellationToken cancellationToken = default)
    {
        var sourceMembership =
            await _membershipRepository.GetByIdAsync(
                command.MembershipId,
                cancellationToken);

        if (sourceMembership is null)
        {
            throw new NotFoundException(
                $"Membership with id {command.MembershipId} was not found.");
        }

        var today =
            DateOnly.FromDateTime(
                DateTime.Now);

        var sourceEffectiveStatus =
            sourceMembership.GetEffectiveStatus(
                today);

        if (sourceEffectiveStatus ==
            MembershipStatus.Cancelled)
        {
            throw new ConflictException(
                "A cancelled membership cannot be renewed.");
        }

        var plan =
            await _planRepository.GetByIdAsync(
                sourceMembership.PlanId,
                cancellationToken);

        if (plan is null)
        {
            throw new NotFoundException(
                $"Plan with id {sourceMembership.PlanId} was not found.");
        }

        if (!plan.IsPublished)
        {
            throw new ConflictException(
                "The membership cannot be renewed because its plan is no longer published.");
        }

        var latestMembership =
            await _membershipRepository
                .GetLatestForMemberAsync(
                    sourceMembership.MemberId,
                    cancellationToken);

        DateOnly newStartDate;

        if (latestMembership is null)
        {
            newStartDate =
                sourceEffectiveStatus ==
                MembershipStatus.Expired
                    ? today
                    : sourceMembership.EndDate;
        }
        else
        {
            var latestEffectiveStatus =
                latestMembership.GetEffectiveStatus(
                    today);

            if (latestEffectiveStatus ==
                    MembershipStatus.Expired &&
                latestMembership.MembershipId ==
                    sourceMembership.MembershipId)
            {
                newStartDate =
                    today;
            }
            else
            {
                newStartDate =
                    latestMembership.EndDate;
            }
        }

        var purchaseDate =
            DateTime.Now;

        var newMembership =
            Membership.Purchase(
                sourceMembership.MemberId,
                plan,
                purchaseDate,
                newStartDate);

        try
        {
            await _membershipOverlapChecker
                .EnsureNoOverlapAsync(
                    newMembership.MemberId,
                    newMembership.StartDate,
                    newMembership.EndDate,
                    cancellationToken:
                        cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            throw new ConflictException(
                ex.Message);
        }

        await _membershipRepository.AddAsync(
            newMembership,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return new RenewMembershipResult(
            newMembership.MembershipId,
            newMembership.MemberId,
            newMembership.PlanId,
            newMembership.StartDate,
            newMembership.EndDate);
    }
}