using HospitalTriage.Application.Models;
using HospitalTriage.Domain.Entities;
using HospitalTriage.Domain.Enums;

namespace HospitalTriage.Application.Interfaces.Services;

public interface IVisitService
{
    Task<Visit?> GetByIdAsync(int id, bool includeDetails = false, CancellationToken ct = default);

    Task<(bool ok, string? error, VisitCreateResult? result)> CreateVisitAsync(
        VisitCreateRequest request,
        string? changedByUserId,
        CancellationToken ct = default);

    Task<(bool ok, string? error)> ChangeStatusAsync(VisitStatusChangeRequest request, CancellationToken ct = default);

    Task<List<Visit>> GetWaitingListAsync(VisitStatus status, int? departmentId = null, CancellationToken ct = default);

    // ✅ thêm: lấy danh sách chờ triage theo ngày
    Task<List<Visit>> GetWaitingTriageByDateAsync(DateOnly date, CancellationToken ct = default);
}
