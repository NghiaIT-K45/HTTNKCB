using HospitalTriage.Domain.Enums;

namespace HospitalTriage.Domain.Entities;

public class VisitStatusHistory
{
    public long Id { get; set; }

    public int VisitId { get; set; }
    public Visit? Visit { get; set; }

    public VisitStatus Status { get; set; }

    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Identity UserId thực hiện thay đổi (optional)
    /// </summary>
    public string? ChangedByUserId { get; set; }
}
