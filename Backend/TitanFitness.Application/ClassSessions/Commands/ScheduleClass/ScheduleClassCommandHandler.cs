using TitanFitness.Application.Abstractions.Persistence;
using TitanFitness.Application.Common.Exceptions;
using TitanFitness.Domain.Entities;
using TitanFitness.Domain.Repositories;
using TitanFitness.Domain.Services;

namespace TitanFitness.Application.ClassSessions.Commands.ScheduleClass;

public sealed class ScheduleClassCommandHandler
{
    private const int DefaultCapacity = 20;

    private readonly IBranchRepository _branchRepository;
    private readonly ITrainerRepository _trainerRepository;
    private readonly IClassSessionRepository _classSessionRepository;
    private readonly SessionConflictChecker _sessionConflictChecker;
    private readonly IUnitOfWork _unitOfWork;

    public ScheduleClassCommandHandler(
        IBranchRepository branchRepository,
        ITrainerRepository trainerRepository,
        IClassSessionRepository classSessionRepository,
        SessionConflictChecker sessionConflictChecker,
        IUnitOfWork unitOfWork)
    {
        _branchRepository =
            branchRepository;

        _trainerRepository =
            trainerRepository;

        _classSessionRepository =
            classSessionRepository;

        _sessionConflictChecker =
            sessionConflictChecker;

        _unitOfWork =
            unitOfWork;
    }

    public async Task<ScheduleClassResult> Handle(
        ScheduleClassCommand command,
        CancellationToken cancellationToken = default)
    {
        var branch =
            await _branchRepository.GetByIdAsync(
                command.BranchId,
                cancellationToken);

        if (branch is null)
        {
            throw new NotFoundException(
                $"Branch with id {command.BranchId} was not found.");
        }

        var capacityLimit =
            command.CapacityLimit ?? DefaultCapacity;

        if (command.StudioId.HasValue)
        {
            var studio =
                branch.Studios.FirstOrDefault(
                    x =>
                        x.StudioId ==
                        command.StudioId.Value);

            if (studio is null)
            {
                throw new NotFoundException(
                    $"Studio with id {command.StudioId.Value} was not found in branch {command.BranchId}.");
            }

            if (!command.CapacityLimit.HasValue)
            {
                capacityLimit =
                    Math.Min(
                        DefaultCapacity,
                        studio.Capacity);
            }

            if (capacityLimit >
                studio.Capacity)
            {
                throw new ConflictException(
                    "Session capacity cannot exceed studio capacity.");
            }
        }

        if (command.TrainerId.HasValue)
        {
            var trainer =
                await _trainerRepository.GetByIdAsync(
                    command.TrainerId.Value,
                    cancellationToken);

            if (trainer is null)
            {
                throw new NotFoundException(
                    $"Trainer with id {command.TrainerId.Value} was not found.");
            }

            if (!trainer.IsActive)
            {
                throw new ConflictException(
                    "An inactive trainer cannot be assigned to a class session.");
            }

            if (trainer.BranchId !=
                command.BranchId)
            {
                throw new ConflictException(
                    "The trainer does not belong to the selected branch.");
            }
        }

        ClassSession classSession;

        try
        {
            classSession =
                new ClassSession(
                    command.ClassName,
                    command.BranchId,
                    command.StudioId,
                    command.TrainerId,
                    command.SessionDate,
                    command.StartTime,
                    command.DurationInMinutes,
                    capacityLimit,
                    command.Description);
        }
        catch (ArgumentException ex)
        {
            throw new ConflictException(
                ex.Message);
        }

        try
        {
            await _sessionConflictChecker
                .EnsureNoConflictsAsync(
                    classSession.TrainerId,
                    classSession.StudioId,
                    classSession.GetTimeRange(),
                    cancellationToken:
                        cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            throw new ConflictException(
                ex.Message);
        }

        await _classSessionRepository.AddAsync(
            classSession,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return new ScheduleClassResult(
            classSession.SessionId,
            classSession.ClassName,
            classSession.BranchId,
            classSession.StudioId,
            classSession.TrainerId,
            classSession.SessionDate,
            classSession.StartTime,
            classSession.DurationInMinutes,
            classSession.CapacityLimit);
    }
}