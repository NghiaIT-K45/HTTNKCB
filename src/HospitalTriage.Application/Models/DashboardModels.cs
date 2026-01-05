using HospitalTriage.Domain.Enums;

namespace HospitalTriage.Application.Models;

public sealed record DashboardRequest(
    int? DepartmentId,
    string? UserId,
    bool IsDoctorView
);

public sealed record StatusCountItem(VisitStatus Status, int Count);

public sealed record WaitingVisitItem(
    int VisitId,
    int QueueNumber,
    DateOnly VisitDate,
    string PatientName,
    int? DepartmentId,
    string? DepartmentName,
    int? DoctorId,
    string? DoctorName,
    VisitStatus Status
);

public sealed record DashboardResult(
    List<StatusCountItem> StatusCounts,
    List<WaitingVisitItem> WaitingList
);
