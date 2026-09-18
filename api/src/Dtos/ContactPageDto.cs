namespace TbtbChallenge.Api.Dtos;

public record ContactPageDto(
    IReadOnlyList<ContactListItemDto> Items,
    int Total,
    int Page,
    int PageSize,
    int TotalPages,
    IReadOnlyDictionary<string, int> DayCounts);
