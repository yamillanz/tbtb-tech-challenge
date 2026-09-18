namespace TbtbChallenge.Api.Dtos;

public record AmendmentDto(
    int Id,
    int ContactId,
    string Reason,
    string AmendedBy,
    DateTime AmendedAt,
    string? OldChannel,
    string? NewChannel,
    string? OldResult,
    string? NewResult,
    DateOnly? OldContactDate,
    DateOnly? NewContactDate,
    string? OldNotes,
    string? NewNotes);
