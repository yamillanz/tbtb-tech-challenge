namespace TbtbChallenge.Api.Entities;

public class Contact
{
    public int Id { get; set; }

    public int PatientId { get; set; }

    public Patient Patient { get; set; } = null!;

    public int GestorId { get; set; }

    public Gestor Gestor { get; set; } = null!;

    public DateOnly ContactDate { get; set; }

    public string Channel { get; set; } = string.Empty;

    public string Result { get; set; } = string.Empty;

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }
}
