namespace TbtbChallenge.Api.Entities;

public class ContactAmendment
{
    public int Id { get; set; }

    public int ContactId { get; set; }

    public Contact Contact { get; set; } = null!;

    public string Reason { get; set; } = string.Empty;

    public int AmendedByGestorId { get; set; }

    public Gestor AmendedByGestor { get; set; } = null!;

    public string? OldChannel { get; set; }

    public string? NewChannel { get; set; }

    public string? OldResult { get; set; }

    public string? NewResult { get; set; }

    public DateOnly? OldContactDate { get; set; }

    public DateOnly? NewContactDate { get; set; }

    public string? OldNotes { get; set; }

    public string? NewNotes { get; set; }

    public DateTime AmendedAt { get; set; }
}
