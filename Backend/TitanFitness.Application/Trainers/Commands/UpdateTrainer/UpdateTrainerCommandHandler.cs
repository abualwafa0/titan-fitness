using TitanFitness.Application.Abstractions.Persistence;
using TitanFitness.Application.Common.Exceptions;
using TitanFitness.Domain.Repositories;

namespace TitanFitness.Application.Trainers.Commands.UpdateTrainer;

public sealed class UpdateTrainerCommandHandler
{
    private readonly ITrainerRepository _trainerRepository;
    private readonly IBranchRepository _branchRepository;
    private readonly IClassSessionRepository _classSessionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateTrainerCommandHandler(
        ITrainerRepository trainerRepository,
        IBranchRepository branchRepository,
        IClassSessionRepository classSessionRepository,
        IUnitOfWork unitOfWork)
    {
        _trainerRepository = trainerRepository;
        _branchRepository = branchRepository;
        _classSessionRepository = classSessionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        UpdateTrainerCommand command,
        CancellationToken cancellationToken = default)
    {
        var trainer = await _trainerRepository.GetByIdAsync(
            command.TrainerId,
            cancellationToken);

        if (trainer is null)
        {
            throw new NotFoundException(
                $"Trainer with id {command.TrainerId} was not found.");
        }

        var branchExists = await _branchRepository.ExistsAsync(
            command.BranchId,
            cancellationToken);

        if (!branchExists)
        {
            throw new NotFoundException(
                $"Branch with id {command.BranchId} was not found.");
        }

        if (!string.IsNullOrWhiteSpace(command.Email))
        {
            var emailTaken = await _trainerRepository.EmailExistsAsync(
                command.Email,
                command.TrainerId,
                cancellationToken);

            if (emailTaken)
            {
                throw new ConflictException(
                    "A trainer with this email already exists.");
            }
        }

        var wasActive = trainer.IsActive;

        try
        {
            trainer.Update(
                command.TrainerName,
                command.Specialty,
                command.Email,
                command.Phone,
                command.BranchId,
                command.IsActive);
        }
        catch (ArgumentException ex)
        {
            throw new ConflictException(ex.Message);
        }

        if (wasActive && !command.IsActive)
        {
            var now = DateTime.Now;

            var futureSessions =
                await _classSessionRepository
                    .GetFutureSessionsByTrainerAsync(
                        trainer.TrainerId,
                        now,
                        cancellationToken);

            foreach (var session in futureSessions)
            {
                session.RemoveTrainer();
            }
        }

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);
    }
}