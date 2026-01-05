using System.Security.Claims;
using HospitalTriage.Application.Interfaces.Services;
using HospitalTriage.Application.Models;
using HospitalTriage.Web.ViewModels.Dashboard;

namespace HospitalTriage.Web.Services.ApiServices;

public sealed class DashboardApiService
{
    private readonly IDashboardService _dashboardService;
    private readonly IDepartmentService _departmentService;
    private readonly IDoctorService _doctorService;

    public DashboardApiService(
        IDashboardService dashboardService,
        IDepartmentService departmentService,
        IDoctorService doctorService)
    {
        _dashboardService = dashboardService;
        _departmentService = departmentService;
        _doctorService = doctorService;
    }

    public async Task<(bool ok, string? error, DashboardVm? data)> GetDashboardAsync(
    ClaimsPrincipal user,
    int? departmentId,
    CancellationToken ct = default)
    {
        try
        {
            int? effectiveDeptId = departmentId;
            string? deptName = null;

            var isDoctor = user.IsInRole("Doctor");
            if (isDoctor)
            {
                var identityName = user.Identity?.Name;
                if (string.IsNullOrWhiteSpace(identityName))
                    return (false, "Không xác định được tài khoản bác sĩ.", null);

                // ✅ Doctor.Code = "DR001", nhưng Identity.Name có thể là "DR001@hospital.local"
                var code = identityName.Contains('@')
                    ? identityName.Split('@', 2)[0]
                    : identityName;

                var doctor = await _doctorService.GetByCodeAsync(code, ct);
                if (doctor is null)
                    return (false, $"Không tìm thấy bác sĩ theo code '{code}'.", null);

                effectiveDeptId = doctor.DepartmentId;
                deptName = doctor.Department?.Name;
            }
            else if (effectiveDeptId.HasValue)
            {
                var dept = await _departmentService.GetByIdAsync(effectiveDeptId.Value, ct);
                deptName = dept?.Name;
            }

            // ✅ Admin/Manager được chọn khoa, Doctor thì không
            var canSelect = user.IsInRole("Admin") || user.IsInRole("Manager");
            var deptSelectItems = new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>();

            if (canSelect && !isDoctor)
            {
                var depts = await _departmentService.SearchAsync(keyword: null, ct);
                deptSelectItems = depts
                    .Where(d => d.IsActive)
                    .OrderBy(d => d.Name)
                    .Select(d => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                    {
                        Value = d.Id.ToString(),
                        Text = $"{d.Code} - {d.Name}",
                        Selected = effectiveDeptId.HasValue && d.Id == effectiveDeptId.Value
                    })
                    .ToList();
            }

            var (ok, error, result) = await _dashboardService.GetDashboardAsync(
                new DashboardRequest(
                    DepartmentId: effectiveDeptId,
                    UserId: user.FindFirstValue(ClaimTypes.NameIdentifier),
                    IsDoctorView: isDoctor
                ),
                ct);

            if (!ok || result is null)
                return (false, error, null);

            var vm = new DashboardVm
            {
                DepartmentId = effectiveDeptId,
                DepartmentName = deptName,

                CanSelectDepartment = canSelect && !isDoctor,
                Departments = deptSelectItems,

                StatusCounts = result.StatusCounts
                    .Select(x => new DashboardStatusCountVm { Status = x.Status, Count = x.Count })
                    .ToList(),
                WaitingList = result.WaitingList
                    .Select(x => new DashboardWaitingItemVm
                    {
                        VisitId = x.VisitId,
                        QueueNumber = x.QueueNumber,
                        VisitDate = x.VisitDate,
                        PatientName = x.PatientName,
                        DepartmentName = x.DepartmentName,
                        DoctorName = x.DoctorName,
                        Status = x.Status
                    })
                    .ToList()
            };

            return (true, null, vm);
        }
        catch (Exception ex)
        {
            return (false, ex.Message, null);
        }
    }
}