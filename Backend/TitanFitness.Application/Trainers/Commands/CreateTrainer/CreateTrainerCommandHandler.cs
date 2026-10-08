using TitanFitness.Application.Abstractions.Persistence;
using TitanFitness.Application.Common.Exceptions;
using TitanFitness.Domain.Entities;
using TitanFitness.Domain.Repositories;

namespace TitanFitness.Application.Trainers.Commands.CreateTrainer;

public sealed class CreateTrainerCommandHandler
{
    private readonly ITrainerRepository _trainerRepository;
    private readonly IBranchRepository _branchRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateTrainerCommandHandler(
        ITrainerRepository trainerRepository,
        IBranchRepository branchRepository,
        IUnitOfWork unitOfWork)
    {
        _trainerRepository = trainerRepository;
        _branchRepository = branchRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateTrainerResult> Handle(
        CreateTrainerCommand command,
        CancellationToken cancellationToken = default)
    {
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
                null,
                cancellationToken);

            if (emailTaken)
            {
                throw new ConflictException(
                    "A trainer with this email already exists.");
            }
        }

        var trainerNumber = await _trainerRepository.GetNextTrainerNumberAsync(
            cancellationToken);

        Trainer trainer;

        try
        {
            trainer = new Trainer(
                trainerNumber,
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

        await _trainerRepository.AddAsync(
            trainer,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return new CreateTrainerResult(
            trainer.TrainerId,
            trainer.TrainerNumber);
    }
}