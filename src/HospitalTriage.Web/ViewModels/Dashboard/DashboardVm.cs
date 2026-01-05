using HospitalTriage.Domain.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HospitalTriage.Web.ViewModels.Dashboard;

public sealed class DashboardStatusCountVm
{
    public VisitStatus Status { get; set; }
    public int Count { get; set; }
}

public sealed class DashboardWaitingItemVm
{
    public int VisitId { get; set; }
    public int QueueNumber { get; set; }
    public DateOnly VisitDate { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string? DepartmentName { get; set; }
    public string? DoctorName { get; set; }
    public VisitStatus Status { get; set; }
}

public sealed class DashboardVm
{
    public int? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }

    // ✅ Cho Admin/Manager chọn khoa
    public bool CanSelectDepartment { get; set; }
    public List<SelectListItem> Departments { get; set; } = new();

    public List<DashboardStatusCountVm> StatusCounts { get; set; } = new();
    public List<DashboardWaitingItemVm> WaitingList { get; set; } = new();
}
