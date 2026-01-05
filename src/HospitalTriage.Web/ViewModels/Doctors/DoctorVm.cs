using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HospitalTriage.Web.ViewModels.Doctors;

public sealed class DoctorListItemVm
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public sealed class DoctorIndexVm
{
    public string? Search { get; set; }
    public int? DepartmentId { get; set; }
    public List<SelectListItem> Departments { get; set; } = new();
    public List<DoctorListItemVm> Items { get; set; } = new();
}

public sealed class DoctorCreateVm
{
    [Required(ErrorMessage = "Code là bắt buộc")]
    [StringLength(20)]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Họ tên là bắt buộc")]
    [StringLength(200)]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Khoa là bắt buộc")]
    public int DepartmentId { get; set; }

    public bool IsActive { get; set; } = true;

    public List<SelectListItem> Departments { get; set; } = new();
}

public sealed class DoctorEditVm
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Code là bắt buộc")]
    [StringLength(20)]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Họ tên là bắt buộc")]
    [StringLength(200)]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Khoa là bắt buộc")]
    public int DepartmentId { get; set; }

    public bool IsActive { get; set; } = true;

    public List<SelectListItem> Departments { get; set; } = new();
}
