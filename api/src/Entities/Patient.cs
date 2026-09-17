namespace TbtbChallenge.Api.Entities;

public class Patient
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string DocumentType { get; set; } = string.Empty;

    public string DocumentNumber { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string City { get; set; } = string.Empty;

    public DateOnly TreatmentStartDate { get; set; }

    public string Status { get; set; } = "activo";

    public DateTime? StatusChangedAt { get; set; }

    public DateTime CreatedAt { get; set; }
}
