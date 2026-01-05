using System.ComponentModel.DataAnnotations;

namespace HospitalTriage.Web.ViewModels.Departments;

public sealed class DepartmentListItemVm
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsGeneral { get; set; }
}

public sealed class DepartmentIndexVm
{
    public string? Search { get; set; }
    public List<DepartmentListItemVm> Items { get; set; } = new();
}

public sealed class DepartmentCreateVm
{
    [Required(ErrorMessage = "Code là bắt buộc")]
    [StringLength(20)]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Name là bắt buộc")]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public bool IsGeneral { get; set; } = false;
}

public sealed class DepartmentEditVm
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Code là bắt buộc")]
    [StringLength(20)]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Name là bắt buộc")]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public bool IsGeneral { get; set; } = false;
}
