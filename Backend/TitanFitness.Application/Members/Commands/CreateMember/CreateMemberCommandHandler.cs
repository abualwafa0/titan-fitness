using TitanFitness.Application.Abstractions.Persistence;
using TitanFitness.Application.Abstractions.Storage;
using TitanFitness.Application.Common.Exceptions;
using TitanFitness.Domain.Entities;
using TitanFitness.Domain.Repositories;

namespace TitanFitness.Application.Members.Commands.CreateMember;

public sealed class CreateMemberCommandHandler
{
    private readonly IMemberRepository _memberRepository;
    private readonly IBranchRepository _branchRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IUnitOfWork _unitOfWork;

    public CreateMemberCommandHandler(
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

    public async Task<CreateMemberResult> Handle(
        CreateMemberCommand command,
        CancellationToken cancellationToken = default)
    {
        var branchExists = await _branchRepository.ExistsAsync(
            command.HomeBranchId,
            cancellationToken);

        if (!branchExists)
        {
            throw new NotFoundException(
                $"Branch with id {command.HomeBranchId} was not found.");
        }

        var membershipNumber = string.IsNullOrWhiteSpace(command.MembershipNumber)
            ? await _memberRepository.GetNextMembershipNumberAsync(
                cancellationToken)
            : command.MembershipNumber.Trim().ToUpperInvariant();

        if (!string.IsNullOrWhiteSpace(command.MembershipNumber))
        {
            var membershipNumberExists =
                await _memberRepository.MembershipNumberExistsAsync(
                    membershipNumber,
                    cancellationToken);

            if (membershipNumberExists)
            {
                throw new ConflictException(
                    $"Membership number '{membershipNumber}' already exists.");
            }
        }

        string? photoPath = null;

        if (command.PhotoStream is not null &&
            !string.IsNullOrWhiteSpace(command.PhotoFileName))
        {
            photoPath = await _fileStorageService.SaveMemberPhotoAsync(
                command.PhotoStream,
                command.PhotoFileName,
                cancellationToken);
        }

        try
        {
            var member = new Member(
                membershipNumber,
                command.FullName,
                command.Email,
                command.Phone,
                command.Address,
                command.JoinedDate,
                photoPath,
                command.HomeBranchId);

            await _memberRepository.AddAsync(
                member,
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(
                cancellationToken);

            return new CreateMemberResult(
                member.MemberId,
                member.MembershipNumber);
        }
        catch
        {
            if (!string.IsNullOrWhiteSpace(photoPath))
            {
                await _fileStorageService.DeleteAsync(
                    photoPath,
                    cancellationToken);
            }

            throw;
        }
    }
}