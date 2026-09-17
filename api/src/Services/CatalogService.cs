using Microsoft.EntityFrameworkCore;
using TbtbChallenge.Api.Data;
using TbtbChallenge.Api.Dtos;

namespace TbtbChallenge.Api.Services;

public class CatalogService
{
    private readonly TbtbChallengeDbContext _context;

    public CatalogService(TbtbChallengeDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<PatientOptionDto>> ListPatientsAsync(CancellationToken cancellationToken)
    {
        return await _context.Patients
            .OrderBy(p => p.Name)
            .Select(p => new PatientOptionDto(p.Id, p.Name, p.DocumentNumber, p.City))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<GestorOptionDto>> ListGestorsAsync(CancellationToken cancellationToken)
    {
        return await _context.Gestors
            .OrderBy(g => g.Name)
            .Select(g => new GestorOptionDto(g.Id, g.Name))
            .ToListAsync(cancellationToken);
    }
}
