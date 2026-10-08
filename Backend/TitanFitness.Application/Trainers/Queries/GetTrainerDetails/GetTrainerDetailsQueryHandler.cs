using TitanFitness.Application.Common.Exceptions;

namespace TitanFitness.Application.Trainers.Queries.GetTrainerDetails;

public sealed class GetTrainerDetailsQueryHandler
{
    private readonly ITrainerDetailsReadService _readService;

    public GetTrainerDetailsQueryHandler(
        ITrainerDetailsReadService readService)
    {
        _readService = readService;
    }

    public async Task<TrainerDetailsDto> Handle(
        GetTrainerDetailsQuery query,
        CancellationToken cancellationToken = default)
    {
        if (query.TrainerId <= 0)
        {
            throw new ArgumentException(
                "Trainer id must be greater than zero.");
        }

        var trainer = await _readService.GetByIdAsync(
            query.TrainerId,
            cancellationToken);

        if (trainer is null)
        {
            throw new NotFoundException(
                $"Trainer with id {query.TrainerId} was not found.");
        }

        return trainer;
    }
}