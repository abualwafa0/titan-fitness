using TitanFitness.Application.Abstractions.Persistence;
using TitanFitness.Application.Common.Exceptions;
using TitanFitness.Domain.Entities;
using TitanFitness.Domain.Repositories;

namespace TitanFitness.Application.Plans.Commands.CreatePlan;

public sealed class CreatePlanCommandHandler
{
    private readonly IPlanRepository _planRepository;
    private readonly IBranchRepository _branchRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreatePlanCommandHandler(
        IPlanRepository planRepository,
        IBranchRepository branchRepository,
        IUnitOfWork unitOfWork)
    {
        _planRepository = planRepository;
        _branchRepository = branchRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CreatePlanResult> Handle(
        CreatePlanCommand command,
        CancellationToken cancellationToken = default)
    {
        var nameTaken =
            await _planRepository.NameExistsAsync(
                command.PlanName,
                null,
                cancellationToken);

        if (nameTaken)
        {
            throw new ConflictException(
                "A plan with this name already exists.");
        }
        var branchIds =
            await ResolveBranchIdsAsync(
                command.BranchIds,
                cancellationToken);

        Plan plan;

        try
        {
            plan = new Plan(
                command.PlanName,
                command.Price,
                command.DurationInMonths,
                command.MaximumFreezeDays,
                command.MaximumNumberOfFreezes,
                command.GuestPassQuota,
                command.AccessScope,
                command.IsPublished);

            plan.SetAvailableBranches(
                branchIds);
        }
        catch (ArgumentException ex)
        {
            throw new ConflictException(
                ex.Message);
        }

        await _planRepository.AddAsync(
            plan,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return new CreatePlanResult(
            plan.PlanId);
    }

    private async Task<IReadOnlyCollection<int>>
        ResolveBranchIdsAsync(
            IReadOnlyCollection<int>? requestedBranchIds,
            CancellationToken cancellationToken)
    {
        var existingBranchIds =
            await _branchRepository.GetAllIdsAsync(
                cancellationToken);

        if (requestedBranchIds is null ||
            requestedBranchIds.Count == 0)
        {
            return existingBranchIds;
        }

        var requested =
            requestedBranchIds
                .Distinct()
                .ToList();

        var existingSet =
            existingBranchIds.ToHashSet();

        var invalidBranchId =
            requested.FirstOrDefault(
                x => !existingSet.Contains(x));

        if (invalidBranchId != 0)
        {
            throw new NotFoundException(
                $"Branch with id {invalidBranchId} was not found.");
        }

        return requested;
    }
}