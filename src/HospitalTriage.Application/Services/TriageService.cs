using HospitalTriage.Application.Interfaces.Repositories;
using HospitalTriage.Application.Interfaces.Services;
using HospitalTriage.Application.Models;
using HospitalTriage.Domain.Entities;
using HospitalTriage.Domain.Enums;

namespace HospitalTriage.Application.Services;

public sealed class TriageService : ITriageService
{
    private readonly IVisitRepository _visitRepo;
    private readonly IDepartmentRepository _deptRepo;
    private readonly ITriageRuleEngine _ruleEngine;

    public TriageService(IVisitRepository visitRepo, IDepartmentRepository deptRepo, ITriageRuleEngine ruleEngine)
    {
        _visitRepo = visitRepo;
        _deptRepo = deptRepo;
        _ruleEngine = ruleEngine;
    }

    public async Task<(bool ok, string? error, TriageResult? result)> TriageAsync(TriageRequest request, CancellationToken ct = default)
    {
        var visit = await _visitRepo.GetByIdAsync(request.VisitId, includeDetails: false, ct);
        if (visit is null)
            return (false, "Không tìm thấy lượt khám.", null);

        if (string.IsNullOrWhiteSpace(request.Symptoms))
            return (false, "Triệu chứng ban đầu là bắt buộc.", null);

        var deptId = request.DepartmentId;

        if (deptId is null)
        {
            deptId = await _ruleEngine.SuggestDepartmentIdAsync(request.Symptoms, ct);
        }

        if (deptId is null)
        {
            var general = await _deptRepo.GetGeneralDepartmentAsync(ct);
            deptId = general?.Id;
        }

        if (deptId is null)
            return (false, "Không xác định được khoa khám (không có Khoa Tổng Quát).", null);

        // Update visit info
        visit.Symptoms = request.Symptoms.Trim();
        visit.DepartmentId = deptId.Value;
        visit.DoctorId = request.DoctorId;
        visit.UpdatedAt = DateTime.UtcNow;

        await _visitRepo.UpdateAsync(visit, ct);

        // Workflow: WaitingTriage -> Triaged -> WaitingDoctor
        if (visit.CurrentStatus == VisitStatus.WaitingTriage)
        {
            visit.CurrentStatus = VisitStatus.Triaged;

            await _visitRepo.AddStatusHistoryAsync(new VisitStatusHistory
            {
                VisitId = visit.Id,
                Status = VisitStatus.Triaged,
                ChangedAt = DateTime.UtcNow,
                ChangedByUserId = request.ChangedByUserId
            }, ct);

            visit.CurrentStatus = VisitStatus.WaitingDoctor;
            await _visitRepo.AddStatusHistoryAsync(new VisitStatusHistory
            {
                VisitId = visit.Id,
                Status = VisitStatus.WaitingDoctor,
                ChangedAt = DateTime.UtcNow,
                ChangedByUserId = request.ChangedByUserId
            }, ct);
        }

        await _visitRepo.SaveChangesAsync(ct);

        return (true, null, new TriageResult(visit.Id, deptId.Value, request.DoctorId));
    }
}
