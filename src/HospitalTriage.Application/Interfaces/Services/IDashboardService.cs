using HospitalTriage.Application.Models;

namespace HospitalTriage.Application.Interfaces.Services;

public interface IDashboardService
{
    Task<(bool ok, string? error, DashboardResult? result)> GetDashboardAsync(DashboardRequest request, CancellationToken ct = default);
}
