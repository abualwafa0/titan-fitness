using TitanFitness.Application.Abstractions.Persistence;
using TitanFitness.Application.Common.Exceptions;
using TitanFitness.Domain.Repositories;

namespace TitanFitness.Application.ClassSessions.Commands.CancelClassSession;

public sealed class CancelClassSessionCommandHandler
{
    private readonly IClassSessionRepository _classSessionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CancelClassSessionCommandHandler(
        IClassSessionRepository classSessionRepository,
        IUnitOfWork unitOfWork)
    {
        _classSessionRepository = classSessionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        CancelClassSessionCommand command,
        CancellationToken cancellationToken = default)
    {
        var classSession =
            await _classSessionRepository.GetByIdAsync(
                command.SessionId,
                cancellationToken);

        if (classSession is null)
        {
            throw new NotFoundException(
                $"Class session with id {command.SessionId} was not found.");
        }

        try
        {
            classSession.Cancel();
        }
        catch (InvalidOperationException ex)
        {
            throw new ConflictException(ex.Message);
        }

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);
    }
}