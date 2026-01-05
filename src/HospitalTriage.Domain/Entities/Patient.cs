using System.ComponentModel.DataAnnotations;
using HospitalTriage.Domain.Enums;

namespace HospitalTriage.Domain.Entities;

public class Patient
{
    public int Id { get; set; }

    [MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    public DateOnly DateOfBirth { get; set; }

    public Gender Gender { get; set; }

    [MaxLength(50)]
    public string? IdentityNumber { get; set; }

    [MaxLength(30)]
    public string? Phone { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    [MaxLength(50)]
    public string? InsuranceCode { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Visit> Visits { get; set; } = new List<Visit>();
}
