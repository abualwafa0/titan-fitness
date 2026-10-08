using TitanFitness.Application.Abstractions.Persistence;
using TitanFitness.Application.Common.Exceptions;
using TitanFitness.Domain.Entities;
using TitanFitness.Domain.Repositories;
using TitanFitness.Domain.Services;

namespace TitanFitness.Application.Memberships.Commands.PurchaseMembership;

public sealed class PurchaseMembershipCommandHandler
{
    private readonly IMemberRepository _memberRepository;
    private readonly IPlanRepository _planRepository;
    private readonly IMembershipRepository _membershipRepository;
    private readonly MembershipOverlapChecker _membershipOverlapChecker;
    private readonly IUnitOfWork _unitOfWork;

    public PurchaseMembershipCommandHandler(
        IMemberRepository memberRepository,
        IPlanRepository planRepository,
        IMembershipRepository membershipRepository,
        MembershipOverlapChecker membershipOverlapChecker,
        IUnitOfWork unitOfWork)
    {
        _memberRepository = memberRepository;
        _planRepository = planRepository;
        _membershipRepository = membershipRepository;
        _membershipOverlapChecker = membershipOverlapChecker;
        _unitOfWork = unitOfWork;
    }

    public async Task<PurchaseMembershipResult> Handle(
        PurchaseMembershipCommand command,
        CancellationToken cancellationToken = default)
    {
        var member =
            await _memberRepository.GetByIdAsync(
                command.MemberId,
                cancellationToken);

        if (member is null)
        {
            throw new NotFoundException(
                $"Member with id {command.MemberId} was not found.");
        }

        var plan =
            await _planRepository.GetByIdAsync(
                command.PlanId,
                cancellationToken);

        if (plan is null)
        {
            throw new NotFoundException(
                $"Plan with id {command.PlanId} was not found.");
        }

        if (!plan.IsPublished)
        {
            throw new ConflictException(
                "An unpublished plan cannot be purchased.");
        }

        if (!plan.IsAvailableAtBranch(
                member.HomeBranchId))
        {
            throw new ConflictException(
                "The selected plan is not available at the member's home branch.");
        }

        var purchaseDate =
            DateTime.Now;

        var membership =
            Membership.Purchase(
                command.MemberId,
                plan,
                purchaseDate,
                command.StartDate);

        try
        {
            await _membershipOverlapChecker
                .EnsureNoOverlapAsync(
                    membership.MemberId,
                    membership.StartDate,
                    membership.EndDate,
                    cancellationToken:
                        cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            throw new ConflictException(
                ex.Message);
        }

        await _membershipRepository.AddAsync(
            membership,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return new PurchaseMembershipResult(
            membership.MembershipId,
            membership.MemberId,
            membership.PlanId,
            membership.StartDate,
            membership.EndDate);
    }
}