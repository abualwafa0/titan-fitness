using TitanFitness.Domain.Enums;

namespace TitanFitness.Application.ClassSessions.Queries.GetBookingContext;

public sealed record BookingContextDto(
    int SessionId,
    string ClassName,
    int BranchId,
    string BranchName,
    int? StudioId,
    string? StudioName,
    int? TrainerId,
    string? TrainerName,
    DateOnly SessionDate,
    TimeOnly StartTime,
    int DurationInMinutes,
    int CapacityLimit,
    int BookedPlaces,
    int RemainingPlaces,
    int WaitlistCount,
    SessionStatus Status,
    string? Description);