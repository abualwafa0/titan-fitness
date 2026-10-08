using TitanFitness.Application.Abstractions.Persistence;
using TitanFitness.Application.Abstractions.Storage;
using TitanFitness.Application.Common.Exceptions;
using TitanFitness.Domain.Repositories;

namespace TitanFitness.Application.Members.Commands.UpdateMember;

public sealed class UpdateMemberCommandHandler
{
    private readonly IMemberRepository _memberRepository;
    private readonly IBranchRepository _branchRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateMemberCommandHandler(
        IMemberRepository memberRepository,
        IBranchRepository branchRepository,
        IFileStorageService fileStorageService,
        IUnitOfWork unitOfWork)
    {
        _memberRepository = memberRepository;
        _branchRepository = branchRepository;
        _fileStorageService = fileStorageService;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        UpdateMemberCommand command,
        CancellationToken cancellationToken = default)
    {
        var member = await _memberRepository.GetByIdAsync(
            command.MemberId,
            cancellationToken);

        if (member is null)
        {
            throw new NotFoundException(
                $"Member with id {command.MemberId} was not found.");
        }

        var branchExists = await _branchRepository.ExistsAsync(
            command.HomeBranchId,
            cancellationToken);

        if (!branchExists)
        {
            throw new NotFoundException(
                $"Branch with id {command.HomeBranchId} was not found.");
        }

        var oldPhotoPath = member.Photo;
        string? newPhotoPath = null;

        if (command.PhotoStream is not null &&
            !string.IsNullOrWhiteSpace(command.PhotoFileName))
        {
            newPhotoPath = await _fileStorageService.SaveMemberPhotoAsync(
                command.PhotoStream,
                command.PhotoFileName,
                cancellationToken);
        }

        try
        {
            var photoPath = newPhotoPath ?? oldPhotoPath;

            member.UpdateProfile(
                command.FullName,
                command.Email ?? member.Email,
                command.Phone ?? member.Phone,
                command.Address ?? member.Address,
                command.JoinedDate == default
                    ? member.JoinedDate
                    : command.JoinedDate,
                photoPath,
                command.HomeBranchId);

            await _unitOfWork.SaveChangesAsync(
                cancellationToken);
        }
        catch
        {
            if (!string.IsNullOrWhiteSpace(newPhotoPath))
            {
                await _fileStorageService.DeleteAsync(
                    newPhotoPath,
                    cancellationToken);
            }

            throw;
        }

        if (!string.IsNullOrWhiteSpace(newPhotoPath) &&
            !string.IsNullOrWhiteSpace(oldPhotoPath) &&
            !string.Equals(
                newPhotoPath,
                oldPhotoPath,
                StringComparison.OrdinalIgnoreCase))
        {
            await _fileStorageService.DeleteAsync(
                oldPhotoPath,
                cancellationToken);
        }
    }
}