using HospitalTriage.Application.Models;

namespace HospitalTriage.Application.Interfaces.Services;

public interface ITriageService
{
    Task<(bool ok, string? error, TriageResult? result)> TriageAsync(TriageRequest request, CancellationToken ct = default);
}
