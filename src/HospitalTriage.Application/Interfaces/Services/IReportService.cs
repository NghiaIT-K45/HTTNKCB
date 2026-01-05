using HospitalTriage.Application.Models;

namespace HospitalTriage.Application.Interfaces.Services;

public interface IReportService
{
    Task<(bool ok, string? error, ReportResult? result)> GetReportAsync(ReportRequest request, CancellationToken ct = default);
    string ExportVisitsPerDayCsv(ReportResult report);
}
