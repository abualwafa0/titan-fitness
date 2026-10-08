using TitanFitness.Domain.Enums;
using TitanFitness.Domain.ValueObjects;

namespace TitanFitness.Domain.Entities;

public class ClassSession
{
    private readonly List<Booking> _bookings = new();

    public int SessionId { get; private set; }

    public string ClassName { get; private set; } = null!;

    public int BranchId { get; private set; }

    public int? StudioId { get; private set; }

    public int? TrainerId { get; private set; }

    public DateOnly SessionDate { get; private set; }

    public TimeOnly StartTime { get; private set; }

    public int DurationInMinutes { get; private set; }

    public int CapacityLimit { get; private set; }

    public SessionStatus Status { get; private set; }

    public string? Description { get; private set; }

    public IReadOnlyCollection<Booking> Bookings =>
        _bookings.AsReadOnly();

    private ClassSession()
    {
    }

    public ClassSession(
        string className,
        int branchId,
        int? studioId,
        int? trainerId,
        DateOnly sessionDate,
        TimeOnly startTime,
        int durationInMinutes,
        int capacityLimit,
        string? description)
    {
        SetClassName(className);
        SetBranch(branchId);
        SetStudio(studioId);
        SetTrainer(trainerId);

        SessionDate = sessionDate;
        StartTime = startTime;

        SetDuration(durationInMinutes);
        SetCapacityLimit(capacityLimit);
        SetDescription(description);

        Status = SessionStatus.Open;
    }

    public void Update(
        string className,
        int branchId,
        int? studioId,
        int? trainerId,
        DateOnly sessionDate,
        TimeOnly startTime,
        int durationInMinutes,
        int capacityLimit,
        string? description)
    {
        if (Status == SessionStatus.Cancelled)
        {
            throw new InvalidOperationException(
                "A cancelled class session cannot be edited.");
        }

        if (branchId != BranchId &&
            _bookings.Any(x =>
                x.Status != BookingStatus.Cancelled))
        {
            throw new InvalidOperationException(
                "The branch cannot be changed while the class has bookings.");
        }

        var occupiedSeats = GetOccupiedSeatsCount();

        if (capacityLimit < occupiedSeats)
        {
            throw new InvalidOperationException(
                $"Capacity cannot be lower than the current enrolment ({occupiedSeats}).");
        }

        SetClassName(className);
        SetBranch(branchId);
        SetStudio(studioId);
        SetTrainer(trainerId);

        SessionDate = sessionDate;
        StartTime = startTime;

        SetDuration(durationInMinutes);
        SetCapacityLimit(capacityLimit);
        SetDescription(description);
    }

    public DateTime GetStartDateTime()
    {
        return SessionDate.ToDateTime(StartTime);
    }

    public DateTime GetEndDateTime()
    {
        return GetStartDateTime()
            .AddMinutes(DurationInMinutes);
    }

    public TimeRange GetTimeRange()
    {
        return new TimeRange(
            GetStartDateTime(),
            GetEndDateTime());
    }

    public SessionStatus GetEffectiveStatus(
        DateTime now)
    {
        if (Status == SessionStatus.Cancelled)
        {
            return SessionStatus.Cancelled;
        }

        var start = GetStartDateTime();
        var end = GetEndDateTime();

        if (now < start)
        {
            return SessionStatus.Open;
        }

        if (now < end)
        {
            return SessionStatus.InProgress;
        }

        return SessionStatus.Completed;
    }

    public bool HasStarted(
        DateTime now)
    {
        return now >= GetStartDateTime();
    }

    public int GetOccupiedSeatsCount()
    {
        return _bookings.Count(ConsumesSeat);
    }

    public int GetRemainingCapacity()
    {
        return Math.Max(
            0,
            CapacityLimit - GetOccupiedSeatsCount());
    }

    public int GetWaitlistCount()
    {
        return _bookings.Count(
            booking =>
                booking.Status ==
                BookingStatus.Waitlisted);
    }

    public bool HasBookingForMember(
        int memberId)
    {
        return _bookings.Any(
            booking =>
                booking.MemberId == memberId &&
                booking.Status !=
                BookingStatus.Cancelled);
    }

    public Booking AddBooking(
        int memberId,
        DateTime bookedOn,
        string? notesForTrainer)
    {
        if (Status == SessionStatus.Cancelled)
        {
            throw new InvalidOperationException(
                "Cannot book a cancelled class session.");
        }

        if (memberId <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(memberId),
                "Member ID must be greater than zero.");
        }

        if (bookedOn >= GetStartDateTime())
        {
            throw new InvalidOperationException(
                "A session that has started cannot accept bookings.");
        }

        if (HasBookingForMember(memberId))
        {
            throw new InvalidOperationException(
                "The member already has a booking for this session.");
        }

        BookingStatus initialStatus;
        int? waitlistPosition = null;

        if (GetRemainingCapacity() > 0)
        {
            initialStatus =
                BookingStatus.Booked;
        }
        else
        {
            initialStatus =
                BookingStatus.Waitlisted;

            waitlistPosition =
                GetNextWaitlistPosition();
        }

        var booking = new Booking(
            memberId,
            bookedOn,
            initialStatus,
            waitlistPosition,
            notesForTrainer);

        _bookings.Add(booking);

        return booking;
    }

    public void CancelBooking(
        int bookingId)
    {
        var booking =
            GetBooking(bookingId);

        var releasedSeat =
            ConsumesSeat(booking);

        booking.Cancel();

        if (releasedSeat)
        {
            PromoteFirstWaitlistedBooking();
        }

        CompactWaitlistPositions();
    }

    public void MarkBookingAsAttended(
        int bookingId)
    {
        GetBooking(bookingId)
            .MarkAsAttended();
    }

    public void MarkBookingAsNoShow(
        int bookingId)
    {
        GetBooking(bookingId)
            .MarkAsNoShow();
    }

    public void RemoveTrainer()
    {
        TrainerId = null;
    }

    public void Cancel()
    {
        if (Status ==
            SessionStatus.Cancelled)
        {
            throw new InvalidOperationException(
                "The class session is already cancelled.");
        }

        foreach (var booking in
                 _bookings
                     .Where(x =>
                         x.Status is
                             BookingStatus.Booked or
                             BookingStatus.Waitlisted)
                     .ToList())
        {
            booking.Cancel();
        }

        Status =
            SessionStatus.Cancelled;
    }

    private Booking GetBooking(
        int bookingId)
    {
        var booking =
            _bookings.FirstOrDefault(
                x =>
                    x.BookingId ==
                    bookingId);

        if (booking is null)
        {
            throw new InvalidOperationException(
                "Booking was not found in this class session.");
        }

        return booking;
    }

    private int GetNextWaitlistPosition()
    {
        var highestPosition =
            _bookings
                .Where(x =>
                    x.Status ==
                    BookingStatus.Waitlisted)
                .Select(x =>
                    x.WaitlistPosition ?? 0)
                .DefaultIfEmpty(0)
                .Max();

        return highestPosition + 1;
    }

    private void PromoteFirstWaitlistedBooking()
    {
        var firstWaitlisted =
            _bookings
                .Where(x =>
                    x.Status ==
                    BookingStatus.Waitlisted)
                .OrderBy(x =>
                    x.BookedOn)
                .ThenBy(x =>
                    x.BookingId)
                .FirstOrDefault();

        firstWaitlisted?
            .PromoteFromWaitlist();
    }

    private void CompactWaitlistPositions()
    {
        var waitlisted =
            _bookings
                .Where(x =>
                    x.Status ==
                    BookingStatus.Waitlisted)
                .OrderBy(x =>
                    x.BookedOn)
                .ThenBy(x =>
                    x.BookingId)
                .ToList();

        for (var i = 0;
             i < waitlisted.Count;
             i++)
        {
            waitlisted[i]
                .SetWaitlistPosition(i + 1);
        }
    }

    private static bool ConsumesSeat(
        Booking booking)
    {
        return booking.Status is
            BookingStatus.Booked or
            BookingStatus.Attended or
            BookingStatus.NoShow;
    }

    private void SetClassName(
        string className)
    {
        if (string.IsNullOrWhiteSpace(
            className))
        {
            throw new ArgumentException(
                "Class name is required.",
                nameof(className));
        }

        className =
            className.Trim();

        if (className.Length > 100)
        {
            throw new ArgumentException(
                "Class name cannot exceed 100 characters.",
                nameof(className));
        }

        ClassName =
            className;
    }

    private void SetBranch(
        int branchId)
    {
        if (branchId <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(branchId),
                "Branch ID must be greater than zero.");
        }

        BranchId =
            branchId;
    }

    private void SetStudio(
        int? studioId)
    {
        if (studioId.HasValue &&
            studioId.Value <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(studioId),
                "Studio ID must be greater than zero.");
        }

        StudioId =
            studioId;
    }

    private void SetTrainer(
        int? trainerId)
    {
        if (trainerId.HasValue &&
            trainerId.Value <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(trainerId),
                "Trainer ID must be greater than zero.");
        }

        TrainerId =
            trainerId;
    }

    private void SetDuration(
        int durationInMinutes)
    {
        if (durationInMinutes is not
            (30 or 45 or 60))
        {
            throw new ArgumentOutOfRangeException(
                nameof(durationInMinutes),
                "Session duration must be 30, 45, or 60 minutes.");
        }

        DurationInMinutes =
            durationInMinutes;
    }

    private void SetCapacityLimit(
        int capacityLimit)
    {
        if (capacityLimit <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(capacityLimit),
                "Capacity limit must be greater than zero.");
        }

        CapacityLimit =
            capacityLimit;
    }

    private void SetDescription(
        string? description)
    {
        if (string.IsNullOrWhiteSpace(
            description))
        {
            Description = null;
            return;
        }

        description =
            description.Trim();

        if (description.Length > 500)
        {
            throw new ArgumentException(
                "Description cannot exceed 500 characters.",
                nameof(description));
        }

        Description =
            description;
    }
}