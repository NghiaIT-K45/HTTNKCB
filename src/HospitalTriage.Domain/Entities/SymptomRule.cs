using System.ComponentModel.DataAnnotations;

namespace HospitalTriage.Domain.Entities;

public class SymptomRule
{
    public int Id { get; set; }

    [MaxLength(100)]
    public string Keyword { get; set; } = string.Empty;

    public int DepartmentId { get; set; }
    public Department? Department { get; set; }

    public bool IsActive { get; set; } = true;
}
