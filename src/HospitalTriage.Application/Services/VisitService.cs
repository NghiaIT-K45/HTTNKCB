using HospitalTriage.Application.Interfaces.Repositories;
using HospitalTriage.Application.Interfaces.Services;
using HospitalTriage.Application.Models;
using HospitalTriage.Domain.Entities;
using HospitalTriage.Domain.Enums;

namespace HospitalTriage.Application.Services;

public sealed class VisitService : IVisitService
{
    private readonly IVisitRepository _visitRepo;
    private readonly IPatientRepository _patientRepo;

    public VisitService(IVisitRepository visitRepo, IPatientRepository patientRepo)
    {
        _visitRepo = visitRepo;
        _patientRepo = patientRepo;
    }

    public async Task<(bool ok, string? error, VisitCreateResult? result)> CreateVisitAsync(
        VisitCreateRequest request,
        string? changedByUserId,
        CancellationToken ct = default)
    {
        var patient = await _patientRepo.GetByIdAsync(request.PatientId, ct);
        if (patient is null)
            return (false, "Không tìm thấy bệnh nhân.", null);

        if (request.VisitDate == default)
            return (false, "VisitDate không hợp lệ.", null);

        var queueDate = request.VisitDate;
        var maxQueue = await _visitRepo.GetMaxQueueNumberAsync(queueDate, ct);
        var nextQueue = maxQueue + 1;

        var visit = new Visit
        {
            PatientId = request.PatientId,
            VisitDate = request.VisitDate,
            QueueDate = queueDate,
            QueueNumber = nextQueue,
            CurrentStatus = VisitStatus.Registered,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _visitRepo.AddAsync(visit, ct);

        // history: Registered
        await _visitRepo.AddStatusHistoryAsync(new VisitStatusHistory
        {
            Visit = visit,
            Status = VisitStatus.Registered,
            ChangedAt = DateTime.UtcNow,
            ChangedByUserId = changedByUserId
        }, ct);

        // workflow: Registered -> WaitingTriage
        visit.CurrentStatus = VisitStatus.WaitingTriage;
        await _visitRepo.AddStatusHistoryAsync(new VisitStatusHistory
        {
            Visit = visit,
            Status = VisitStatus.WaitingTriage,
            ChangedAt = DateTime.UtcNow,
            ChangedByUserId = changedByUserId
        }, ct);

        await _visitRepo.SaveChangesAsync(ct);

        return (true, null, new VisitCreateResult(visit.Id, visit.QueueNumber, visit.CurrentStatus));
    }

    public Task<Visit?> GetByIdAsync(int id, bool includeDetails = false, CancellationToken ct = default)
        => _visitRepo.GetByIdAsync(id, includeDetails, ct);

    public Task<List<Visit>> GetWaitingListAsync(VisitStatus status, int? departmentId = null, CancellationToken ct = default)
        => _visitRepo.GetByStatusAsync(status, departmentId, ct);

    public async Task<(bool ok, string? error)> ChangeStatusAsync(VisitStatusChangeRequest request, CancellationToken ct = default)
    {
        var visit = await _visitRepo.GetByIdAsync(request.VisitId, includeDetails: false, ct);
        if (visit is null)
            return (false, "Không tìm thấy lượt khám.");

        if (!IsValidTransition(visit.CurrentStatus, request.NewStatus))
            return (false, $"Không thể chuyển trạng thái từ {visit.CurrentStatus} sang {request.NewStatus}.");

        visit.CurrentStatus = request.NewStatus;
        visit.UpdatedAt = DateTime.UtcNow;

        await _visitRepo.UpdateAsync(visit, ct);

        await _visitRepo.AddStatusHistoryAsync(new VisitStatusHistory
        {
            VisitId = visit.Id,
            Status = request.NewStatus,
            ChangedAt = DateTime.UtcNow,
            ChangedByUserId = request.ChangedByUserId
        }, ct);

        await _visitRepo.SaveChangesAsync(ct);

        return (true, null);
    }

    private static bool IsValidTransition(VisitStatus from, VisitStatus to)
    {
        return (from, to) switch
        {
            (VisitStatus.Registered, VisitStatus.WaitingTriage) => true,
            (VisitStatus.WaitingTriage, VisitStatus.Triaged) => true,
            (VisitStatus.Triaged, VisitStatus.WaitingDoctor) => true,
            (VisitStatus.WaitingDoctor, VisitStatus.InExamination) => true,
            (VisitStatus.InExamination, VisitStatus.Done) => true,
            _ => false
        };
    }
    public Task<List<Visit>> GetWaitingTriageByDateAsync(DateOnly date, CancellationToken ct = default)
        => _visitRepo.GetWaitingTriageByDateAsync(date, ct);
}

