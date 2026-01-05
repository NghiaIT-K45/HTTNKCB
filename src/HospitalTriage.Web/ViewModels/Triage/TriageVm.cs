using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HospitalTriage.Web.ViewModels.Triage;

public sealed class TriageVm
{
    public int VisitId { get; set; }
    public int QueueNumber { get; set; }
    public DateOnly VisitDate { get; set; }

    public string PatientName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Triệu chứng là bắt buộc")]
    [StringLength(2000)]
    public string Symptoms { get; set; } = string.Empty;

    /// <summary>
    /// DepartmentId gợi ý (rule engine)
    /// </summary>
    public int? SuggestedDepartmentId { get; set; }

    /// <summary>
    /// Cho phép để trống => service sẽ tự gợi ý / fallback Khoa Tổng Quát.
    /// </summary>
    public int? DepartmentId { get; set; }

    public int? DoctorId { get; set; }

    public List<SelectListItem> Departments { get; set; } = new();
    public List<SelectListItem> Doctors { get; set; } = new();
}
