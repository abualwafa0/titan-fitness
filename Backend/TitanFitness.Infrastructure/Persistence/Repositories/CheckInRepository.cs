using Microsoft.EntityFrameworkCore;
using TitanFitness.Domain.Entities;
using TitanFitness.Domain.Enums;
using TitanFitness.Domain.Repositories;

namespace TitanFitness.Infrastructure.Persistence.Repositories;

public sealed class CheckInRepository
    : ICheckInRepository
{
    private readonly TitanFitnessDbContext _dbContext;

    public CheckInRepository(
        TitanFitnessDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<CheckIn?> GetByIdAsync(
        int checkInId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.CheckIns
            .FirstOrDefaultAsync(
                x => x.CheckInId == checkInId,
                cancellationToken);
    }

    public Task<CheckIn?>
        GetOpenAdmittedCheckInForMemberAsync(
            int memberId,
            CancellationToken cancellationToken = default)
    {
        return _dbContext.CheckIns
            .Where(x =>
                x.MemberId == memberId &&
                x.Result == CheckInResult.Admitted &&
                x.CheckOutDateTime == null)
            .OrderByDescending(
                x => x.CheckInDateTime)
            .FirstOrDefaultAsync(
                cancellationToken);
    }

    public async Task AddAsync(
        CheckIn checkIn,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.CheckIns.AddAsync(
            checkIn,
            cancellationToken);
    }
}