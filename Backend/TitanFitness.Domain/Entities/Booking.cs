using TitanFitness.Domain.Enums;

namespace TitanFitness.Domain.Entities;

public class Booking
{
    public int BookingId { get; private set; }

    public int SessionId { get; private set; }

    public int MemberId { get; private set; }

    public DateTime BookedOn { get; private set; }

    public BookingStatus Status { get; private set; }

    public int? WaitlistPosition { get; private set; }

    public string? NotesForTrainer { get; private set; }

    private Booking()
    {
    }

    internal Booking(
        int memberId,
        DateTime bookedOn,
        BookingStatus status,
        int? waitlistPosition,
        string? notesForTrainer)
    {
        if (memberId <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(memberId),
                "Member ID must be greater than zero.");

        if (status is not
            (BookingStatus.Booked or
             BookingStatus.Waitlisted))
        {
            throw new ArgumentException(
                "A new booking must be booked or waitlisted.",
                nameof(status));
        }

        if (status == BookingStatus.Booked &&
            waitlistPosition.HasValue)
        {
            throw new ArgumentException(
                "A booked reservation cannot have a waitlist position.",
                nameof(waitlistPosition));
        }

        if (status == BookingStatus.Waitlisted &&
            (!waitlistPosition.HasValue ||
             waitlistPosition.Value <= 0))
        {
            throw new ArgumentException(
                "A waitlisted booking must have a valid waitlist position.",
                nameof(waitlistPosition));
        }

        MemberId = memberId;
        BookedOn = bookedOn;
        Status = status;
        WaitlistPosition = waitlistPosition;

        SetNotesForTrainer(notesForTrainer);
    }

    internal void Cancel()
    {
        if (Status == BookingStatus.Cancelled)
            throw new InvalidOperationException(
                "The booking is already cancelled.");

        if (Status is
            BookingStatus.Attended or
            BookingStatus.NoShow)
        {
            throw new InvalidOperationException(
                "A completed booking cannot be cancelled.");
        }

        Status = BookingStatus.Cancelled;
        WaitlistPosition = null;
    }

    internal void PromoteFromWaitlist()
    {
        if (Status != BookingStatus.Waitlisted)
            throw new InvalidOperationException(
                "Only a waitlisted booking can be promoted.");

        Status = BookingStatus.Booked;
        WaitlistPosition = null;
    }

    internal void SetWaitlistPosition(int position)
    {
        if (Status != BookingStatus.Waitlisted)
            throw new InvalidOperationException(
                "Only a waitlisted booking can have a waitlist position.");

        if (position <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(position),
                "Waitlist position must be greater than zero.");

        WaitlistPosition = position;
    }

    internal void MarkAsAttended()
    {
        if (Status != BookingStatus.Booked)
            throw new InvalidOperationException(
                "Only a booked reservation can be marked as attended.");

        Status = BookingStatus.Attended;
    }

    internal void MarkAsNoShow()
    {
        if (Status != BookingStatus.Booked)
            throw new InvalidOperationException(
                "Only a booked reservation can be marked as no-show.");

        Status = BookingStatus.NoShow;
    }

    private void SetNotesForTrainer(
        string? notesForTrainer)
    {
        if (string.IsNullOrWhiteSpace(notesForTrainer))
        {
            NotesForTrainer = null;
            return;
        }

        notesForTrainer = notesForTrainer.Trim();

        if (notesForTrainer.Length > 500)
            throw new ArgumentException(
                "Notes for trainer cannot exceed 500 characters.",
                nameof(notesForTrainer));

        NotesForTrainer = notesForTrainer;
    }
}