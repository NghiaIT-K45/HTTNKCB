using System.Text;
using HospitalTriage.Application.Interfaces.Repositories;
using HospitalTriage.Application.Interfaces.Services;
using HospitalTriage.Application.Models;
using HospitalTriage.Domain.Enums;

namespace HospitalTriage.Application.Services;

public sealed class ReportService : IReportService
{
    private readonly IVisitRepository _visitRepo;

    public ReportService(IVisitRepository visitRepo)
    {
        _visitRepo = visitRepo;
    }

    public async Task<(bool ok, string? error, ReportResult? result)> GetReportAsync(ReportRequest request, CancellationToken ct = default)
    {
        if (request.FromDate == default || request.ToDate == default || request.ToDate < request.FromDate)
            return (false, "Khoảng ngày không hợp lệ.", null);

        var visits = await _visitRepo.GetByDateRangeAsync(request.FromDate, request.ToDate, request.DepartmentId, ct);

        // Visits per day
        var perDay = visits
            .GroupBy(v => v.VisitDate)
            .Select(g => new VisitsPerDayItem(g.Key, g.Count()))
            .OrderBy(x => x.Date)
            .ToList();

        // Avg waiting time: from WaitingTriage -> InExamination (fallback Done)
        var waitingMinutes = new List<double>();

        foreach (var v in visits)
        {
            var histories = v.StatusHistories?.OrderBy(h => h.ChangedAt).ToList();
            if (histories is null || histories.Count == 0)
                continue;

            var t0 = histories.FirstOrDefault(h => h.Status == VisitStatus.WaitingTriage)?.ChangedAt
                     ?? histories.FirstOrDefault(h => h.Status == VisitStatus.Registered)?.ChangedAt;

            var t1 = histories.FirstOrDefault(h => h.Status == VisitStatus.InExamination)?.ChangedAt
                     ?? histories.FirstOrDefault(h => h.Status == VisitStatus.Done)?.ChangedAt;

            if (t0 is null || t1 is null)
                continue;

            if (t1 <= t0)
                continue;

            waitingMinutes.Add((t1.Value - t0.Value).TotalMinutes);
        }

        var avg = waitingMinutes.Count == 0 ? 0 : waitingMinutes.Average();
        var avgResult = new AvgWaitingTimeResult(avg, waitingMinutes.Count);

        return (true, null, new ReportResult(perDay, avgResult));
    }

    public string ExportVisitsPerDayCsv(ReportResult report)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Date,Count");

        foreach (var item in report.VisitsPerDay)
        {
            sb.AppendLine($"{item.Date:yyyy-MM-dd},{item.Count}");
        }

        return sb.ToString();
    }
}
