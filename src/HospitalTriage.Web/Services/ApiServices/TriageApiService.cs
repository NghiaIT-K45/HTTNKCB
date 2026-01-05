using HospitalTriage.Application.Interfaces.Services;
using HospitalTriage.Application.Models;
using HospitalTriage.Domain.Enums;
using HospitalTriage.Web.ViewModels.Triage;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HospitalTriage.Web.Services.ApiServices;

public sealed class TriageApiService
{
    private readonly IVisitService _visitService;
    private readonly IDepartmentService _departmentService;
    private readonly IDoctorService _doctorService;
    private readonly ITriageService _triageService;

    public TriageApiService(
        IVisitService visitService,
        IDepartmentService departmentService,
        IDoctorService doctorService,
        ITriageService triageService)
    {
        _visitService = visitService;
        _departmentService = departmentService;
        _doctorService = doctorService;
        _triageService = triageService;
    }

    // ✅ Danh sách lượt khám chờ phân luồng theo ngày
    public async Task<(bool ok, string? error, TriageWaitingListVm? data)> GetWaitingListAsync(DateOnly date, CancellationToken ct = default)
    {
        // IVisitService hiện có GetWaitingListAsync(status, departmentId)
        // -> Lấy tất cả WaitingTriage rồi lọc theo ngày tại Web layer (nhanh, ít sửa code).
        var visitsAll = await _visitService.GetWaitingListAsync(VisitStatus.WaitingTriage, departmentId: null, ct);

        var visits = visitsAll
            .Where(v => v.VisitDate == date)
            .OrderBy(v => v.QueueNumber)
            .ToList();

        var vm = new TriageWaitingListVm
        {
            Date = date,
            Items = visits
                .Select(x => new TriageWaitingItemVm
                {
                    VisitId = x.Id,
                    QueueNumber = x.QueueNumber,
                    PatientName = x.Patient?.FullName ?? string.Empty,
                    VisitDate = x.VisitDate,
                    CurrentStatus = x.CurrentStatus.ToString()
                })
                .ToList()
        };

        return (true, null, vm);
    }

    public async Task<(bool ok, string? error, TriageVm? data)> GetTriageVmAsync(int visitId, CancellationToken ct = default)
    {
        var visit = await _visitService.GetByIdAsync(visitId, includeDetails: true, ct);
        if (visit is null)
            return (false, "Không tìm thấy lượt khám.", null);

        var depts = await _departmentService.SearchAsync(keyword: null, ct);
        var doctors = await _doctorService.SearchAsync(keyword: null, departmentId: null, ct);

        var vm = new TriageVm
        {
            VisitId = visit.Id,
            VisitDate = visit.VisitDate,
            QueueNumber = visit.QueueNumber,
            PatientName = visit.Patient?.FullName ?? string.Empty,
            Symptoms = visit.Symptoms ?? string.Empty,
            DepartmentId = visit.DepartmentId,
            DoctorId = visit.DoctorId,
            Departments = BuildDepartmentSelectList(depts, visit.DepartmentId),
            Doctors = BuildDoctorSelectList(doctors, visit.DoctorId)
        };

        return (true, null, vm);
    }

    public async Task<(bool ok, string? error)> SubmitAsync(TriageVm vm, string? changedByUserId, CancellationToken ct = default)
    {
        var (ok, error, _) = await _triageService.TriageAsync(
            new TriageRequest(
                vm.VisitId,
                vm.Symptoms,
                vm.DepartmentId,
                vm.DoctorId,
                changedByUserId
            ),
            ct);

        return (ok, error);
    }

    private static List<SelectListItem> BuildDepartmentSelectList(IEnumerable<HospitalTriage.Domain.Entities.Department> depts, int? selectedId)
        => depts
            .Where(d => d.IsActive)
            .OrderBy(d => d.Name)
            .Select(d => new SelectListItem
            {
                Value = d.Id.ToString(),
                Text = $"{d.Code} - {d.Name}",
                Selected = selectedId.HasValue && d.Id == selectedId.Value
            })
            .ToList();

    private static List<SelectListItem> BuildDoctorSelectList(IEnumerable<HospitalTriage.Domain.Entities.Doctor> doctors, int? selectedId)
        => doctors
            .Where(d => d.IsActive)
            .OrderBy(d => d.FullName)
            .Select(d => new SelectListItem
            {
                Value = d.Id.ToString(),
                Text = $"{d.Code} - {d.FullName}",
                Selected = selectedId.HasValue && d.Id == selectedId.Value
            })
            .ToList();
}
