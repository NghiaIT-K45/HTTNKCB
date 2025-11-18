using System.ComponentModel.DataAnnotations;

namespace HttnKcb.Web.ViewModels
{
  public class DepartmentViewModel
  {
    public int Id { get; set; }

    [Required]
    [StringLength(20)]
    public string Code { get; set; } = string.Empty;  // ví dụ: "CARD"

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;  // ví dụ: "Cardiology"

    public bool IsActive { get; set; } = true;
  }
}
