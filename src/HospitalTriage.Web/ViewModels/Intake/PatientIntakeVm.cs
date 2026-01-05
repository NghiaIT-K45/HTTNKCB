using System.ComponentModel.DataAnnotations;
using HospitalTriage.Domain.Enums;

namespace HospitalTriage.Web.ViewModels.Intake;

public sealed class PatientIntakeVm
{
    [Required(ErrorMessage = "Họ tên là bắt buộc")]
    [StringLength(200)]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ngày sinh là bắt buộc")]
    [DataType(DataType.Date)]
    public DateOnly DateOfBirth { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Required(ErrorMessage = "Giới tính là bắt buộc")]
    public Gender Gender { get; set; } = Gender.Unknown;

    [StringLength(50)]
    public string? IdentityNumber { get; set; }

    [StringLength(30)]
    public string? Phone { get; set; }

    [StringLength(500)]
    public string? Address { get; set; }

    [StringLength(50)]
    public string? InsuranceCode { get; set; }
}

public sealed class IntakeResultVm
{
    public int PatientId { get; set; }
    public bool IsNewPatient { get; set; }

    public int VisitId { get; set; }
    public int QueueNumber { get; set; }

    public DateOnly VisitDate { get; set; }
}
