using System.ComponentModel.DataAnnotations;

namespace HospitalTriage.Domain.Entities;

public class Doctor
{
    public int Id { get; set; }

    [MaxLength(20)]
    public string Code { get; set; } = string.Empty;

    [MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    public int DepartmentId { get; set; }

    public Department? Department { get; set; }

    public bool IsActive { get; set; } = true;
}
