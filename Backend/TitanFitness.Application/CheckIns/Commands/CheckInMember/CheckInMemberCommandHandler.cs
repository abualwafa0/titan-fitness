using TitanFitness.Application.Abstractions.Persistence;
using TitanFitness.Application.Common.Exceptions;
using TitanFitness.Domain.Entities;
using TitanFitness.Domain.Enums;
using TitanFitness.Domain.Repositories;
using TitanFitness.Domain.Services;

namespace TitanFitness.Application.CheckIns.Commands.CheckInMember;

public sealed class CheckInMemberCommandHandler
{
    private readonly IMemberRepository _memberRepository;
    private readonly IBranchRepository _branchRepository;
    private readonly IMembershipRepository _membershipRepository;
    private readonly ICheckInRepository _checkInRepository;
    private readonly CheckInEligibilityService _eligibilityService;
    private readonly IUnitOfWork _unitOfWork;

    public CheckInMemberCommandHandler(
        IMemberRepository memberRepository,
        IBranchRepository branchRepository,
        IMembershipRepository membershipRepository,
        ICheckInRepository checkInRepository,
        CheckInEligibilityService eligibilityService,
        IUnitOfWork unitOfWork)
    {
        _memberRepository = memberRepository;
        _branchRepository = branchRepository;
        _membershipRepository = membershipRepository;
        _checkInRepository = checkInRepository;
        _eligibilityService = eligibilityService;
        _unitOfWork = unitOfWork;
    }

    public async Task<CheckInPreviewResult> Preview(
        CheckInMemberCommand command,
        CancellationToken cancellationToken = default)
    {
        var evaluation =
            await EvaluateAsync(
                command,
                cancellationToken);

        return new CheckInPreviewResult(
            command.MemberId,
            command.BranchId,
            evaluation.EvaluatedAt,
            evaluation.Eligibility.IsAdmitted,
            evaluation.Eligibility.RefusalReason);
    }

    public async Task<CheckInMemberResult> Handle(
        CheckInMemberCommand command,
        CancellationToken cancellationToken = default)
    {
        var evaluation =
            await EvaluateAsync(
                command,
                cancellationToken);

        var result =
            evaluation.Eligibility.IsAdmitted
                ? CheckInResult.Admitted
                : CheckInResult.Refused;

        var checkIn = new CheckIn(
            command.MemberId,
            command.BranchId,
            evaluation.EvaluatedAt,
            result,
            evaluation.Eligibility.RefusalReason,
            command.Notes);

        await _checkInRepository.AddAsync(
            checkIn,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return new CheckInMemberResult(
            checkIn.CheckInId,
            checkIn.MemberId,
            checkIn.BranchId,
            checkIn.CheckInDateTime,
            checkIn.Result,
            checkIn.RefusalReason);
    }

    private async Task<CheckInEvaluation>
        EvaluateAsync(
            CheckInMemberCommand command,
            CancellationToken cancellationToken)
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

        var branchExists =
            await _branchRepository.ExistsAsync(
                command.BranchId,
                cancellationToken);

        if (!branchExists)
        {
            throw new NotFoundException(
                $"Branch with id {command.BranchId} was not found.");
        }

        var evaluatedAt =
            command.CheckInDateTime ?? DateTime.Now;

        var openCheckIn =
            await _checkInRepository
                .GetOpenAdmittedCheckInForMemberAsync(
                    command.MemberId,
                    cancellationToken);

        if (openCheckIn is not null)
        {
            return new CheckInEvaluation(
                evaluatedAt,
                CheckInEligibilityResult.Refused(
                    "Member is already inside"));
        }

        var date =
            DateOnly.FromDateTime(
                evaluatedAt);

        var membership =
            await _membershipRepository
                .GetMostRelevantForMemberAsync(
                    command.MemberId,
                    date,
                    cancellationToken);

        var eligibility =
            _eligibilityService.Evaluate(
                member,
                membership,
                command.BranchId,
                evaluatedAt);

        return new CheckInEvaluation(
            evaluatedAt,
            eligibility);
    }

    private sealed record CheckInEvaluation(
        DateTime EvaluatedAt,
        CheckInEligibilityResult Eligibility);
}