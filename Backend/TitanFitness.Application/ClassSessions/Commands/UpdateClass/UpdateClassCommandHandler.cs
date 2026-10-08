using TitanFitness.Application.Abstractions.Persistence;
using TitanFitness.Application.Common.Exceptions;
using TitanFitness.Domain.Repositories;
using TitanFitness.Domain.Services;

namespace TitanFitness.Application.ClassSessions.Commands.UpdateClass;

public sealed class UpdateClassCommandHandler
{
    private readonly IBranchRepository _branchRepository;
    private readonly ITrainerRepository _trainerRepository;
    private readonly IClassSessionRepository _classSessionRepository;
    private readonly SessionConflictChecker _sessionConflictChecker;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateClassCommandHandler(
        IBranchRepository branchRepository,
        ITrainerRepository trainerRepository,
        IClassSessionRepository classSessionRepository,
        SessionConflictChecker sessionConflictChecker,
        IUnitOfWork unitOfWork)
    {
        _branchRepository = branchRepository;
        _trainerRepository = trainerRepository;
        _classSessionRepository = classSessionRepository;
        _sessionConflictChecker = sessionConflictChecker;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        UpdateClassCommand command,
        CancellationToken cancellationToken = default)
    {
        var session =
            await _classSessionRepository.GetByIdAsync(
                command.SessionId,
                cancellationToken);

        if (session is null)
        {
            throw new NotFoundException(
                $"Class session with id {command.SessionId} was not found.");
        }

        var branch =
            await _branchRepository.GetByIdAsync(
                command.BranchId,
                cancellationToken);

        if (branch is null)
        {
            throw new NotFoundException(
                $"Branch with id {command.BranchId} was not found.");
        }

        var startChanged =
            command.SessionDate != session.SessionDate ||
            command.StartTime != session.StartTime;

        if (startChanged &&
            command.SessionDate.ToDateTime(command.StartTime) <=
            DateTime.Now)
        {
            throw new ConflictException(
                "The class must start in the future.");
        }

        var capacityLimit =
            command.CapacityLimit ?? session.CapacityLimit;

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

            if (capacityLimit > studio.Capacity)
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

            if (!trainer.IsActive &&
                session.TrainerId != command.TrainerId)
            {
                throw new ConflictException(
                    "An inactive trainer cannot be assigned to a class session.");
            }

            if (trainer.BranchId != command.BranchId)
            {
                throw new ConflictException(
                    "The trainer does not belong to the selected branch.");
            }
        }

        try
        {
            session.Update(
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
        catch (InvalidOperationException ex)
        {
            throw new ConflictException(
                ex.Message);
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
                    session.TrainerId,
                    session.StudioId,
                    session.GetTimeRange(),
                    session.SessionId,
                    cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            throw new ConflictException(
                ex.Message);
        }

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);
    }
}