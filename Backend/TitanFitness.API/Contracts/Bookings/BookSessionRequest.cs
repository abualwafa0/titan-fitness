namespace TitanFitness.API.Contracts.Bookings;

public sealed class BookSessionRequest
{
    public int MemberId { get; set; }

    public string? NotesForTrainer { get; set; }
}