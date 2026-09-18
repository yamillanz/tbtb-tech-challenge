namespace TbtbChallenge.Api.Dtos;

public record CreateAmendmentRequest(
    int GestorId,
    string Reason,
    string? Channel,
    string? Result,
    DateOnly? ContactDate,
    string? Notes);
