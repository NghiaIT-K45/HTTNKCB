using HospitalTriage.Application.Interfaces.Services;
using HospitalTriage.Application.Models;
using HospitalTriage.Web.ViewModels.Doctors;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HospitalTriage.Web.Services.ApiServices;

public sealed class DoctorApiService
{
    private readonly IDoctorService _service;
    private readonly IDepartmentService _departmentService;

    public DoctorApiService(IDoctorService service, IDepartmentService departmentService)
    {
        _service = service;
        _departmentService = departmentService;
    }

    public async Task<(bool ok, string? error, DoctorIndexVm? data)> SearchAsync(string? keyword, int? departmentId, CancellationToken ct = default)
    {
        try
        {
            var doctors = await _service.SearchAsync(keyword, departmentId, ct);
            var depts = await _departmentService.SearchAsync(keyword: null, ct);

            var vm = new DoctorIndexVm
            {
                Search = keyword,
                DepartmentId = departmentId,
                Departments = BuildDepartmentSelectList(depts, departmentId),
                Items = doctors.Select(x => new DoctorListItemVm
                {
                    Id = x.Id,
                    Code = x.Code,
                    FullName = x.FullName,
                    DepartmentId = x.DepartmentId,
                    DepartmentName = x.Department?.Name ?? string.Empty,
                    IsActive = x.IsActive
                }).ToList()
            };

            return (true, null, vm);
        }
        catch (Exception ex)
        {
            return (false, ex.Message, null);
        }
    }

    public async Task<(bool ok, string? error, DoctorCreateVm? data)> GetCreateVmAsync(CancellationToken ct = default)
    {
        var depts = await _departmentService.SearchAsync(keyword: null, ct);

        var vm = new DoctorCreateVm
        {
            IsActive = true,
            Departments = BuildDepartmentSelectList(depts, selectedId: null)
        };

        return (true, null, vm);
    }

    public async Task<(bool ok, string? error, DoctorEditVm? data)> GetEditVmAsync(int id, CancellationToken ct = default)
    {
        var doctor = await _service.GetByIdAsync(id, ct);
        if (doctor is null)
            return (false, "Không tìm thấy bác sĩ.", null);

        var depts = await _departmentService.SearchAsync(keyword: null, ct);

        var vm = new DoctorEditVm
        {
            Id = doctor.Id,
            Code = doctor.Code,
            FullName = doctor.FullName,
            DepartmentId = doctor.DepartmentId,
            IsActive = doctor.IsActive,
            Departments = BuildDepartmentSelectList(depts, doctor.DepartmentId)
        };

        return (true, null, vm);
    }

    public Task<(bool ok, string? error, int? id)> CreateAsync(DoctorCreateVm vm, CancellationToken ct = default)
        => _service.CreateAsync(new DoctorCreateRequest(vm.Code, vm.FullName, vm.DepartmentId, vm.IsActive), ct);

    public Task<(bool ok, string? error)> UpdateAsync(DoctorEditVm vm, CancellationToken ct = default)
        => _service.UpdateAsync(new DoctorUpdateRequest(vm.Id, vm.Code, vm.FullName, vm.DepartmentId, vm.IsActive), ct);

    public Task<(bool ok, string? error)> DeleteAsync(int id, CancellationToken ct = default)
        => _service.DeactivateAsync(id, ct);

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
}
