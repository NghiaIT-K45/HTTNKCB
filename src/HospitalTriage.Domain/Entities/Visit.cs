using HospitalTriage.Domain.Enums;

namespace HospitalTriage.Domain.Entities;

public class Visit
{
    public int Id { get; set; }

    public int PatientId { get; set; }
    public Patient? Patient { get; set; }

    /// <summary>
    /// Ngày đến khám (theo local date)
    /// </summary>
    public DateOnly VisitDate { get; set; }

    /// <summary>
    /// Ngày sinh số thứ tự (theo local date)
    /// </summary>
    public DateOnly QueueDate { get; set; }

    /// <summary>
    /// Số thứ tự trong ngày (1..N)
    /// </summary>
    public int QueueNumber { get; set; }

    public int? DepartmentId { get; set; }
    public Department? Department { get; set; }

    public int? DoctorId { get; set; }
    public Doctor? Doctor { get; set; }

    public string? Symptoms { get; set; }

    public VisitStatus CurrentStatus { get; set; } = VisitStatus.Registered;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<VisitStatusHistory> StatusHistories { get; set; } = new List<VisitStatusHistory>();
}
