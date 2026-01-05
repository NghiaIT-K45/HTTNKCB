using HospitalTriage.Application.Interfaces.Repositories;
using HospitalTriage.Application.Interfaces.Services;
using HospitalTriage.Application.Models;
using HospitalTriage.Domain.Enums;

namespace HospitalTriage.Application.Services;

public sealed class DashboardService : IDashboardService
{
    private readonly IVisitRepository _visitRepo;

    public DashboardService(IVisitRepository visitRepo)
    {
        _visitRepo = visitRepo;
    }

    public async Task<(bool ok, string? error, DashboardResult? result)> GetDashboardAsync(DashboardRequest request, CancellationToken ct = default)
    {
        try
        {
            var statusList = new[]
            {
                VisitStatus.WaitingTriage,
                VisitStatus.WaitingDoctor,
                VisitStatus.InExamination,
                VisitStatus.Done
            };

            var counts = new List<StatusCountItem>();
            var waitingItems = new List<WaitingVisitItem>();

            foreach (var status in statusList)
            {
                var visits = await _visitRepo.GetByStatusAsync(status, request.DepartmentId, ct);
                counts.Add(new StatusCountItem(status, visits.Count));

                if (status is VisitStatus.WaitingTriage or VisitStatus.WaitingDoctor)
                {
                    waitingItems.AddRange(visits.Select(v => new WaitingVisitItem(
                        v.Id,
                        v.QueueNumber,
                        v.VisitDate,
                        v.Patient?.FullName ?? "(N/A)",
                        v.DepartmentId,
                        v.Department?.Name,
                        v.DoctorId,
                        v.Doctor?.FullName,
                        v.CurrentStatus
                    )));
                }
            }

            // sort waiting list by queue number asc
            waitingItems = waitingItems
                .OrderBy(x => x.VisitDate)
                .ThenBy(x => x.QueueNumber)
                .ToList();

            return (true, null, new DashboardResult(counts, waitingItems));
        }
        catch (Exception ex)
        {
            return (false, ex.Message, null);
        }
    }
}
