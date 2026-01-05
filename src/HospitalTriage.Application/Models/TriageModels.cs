namespace HospitalTriage.Application.Models;

public sealed record TriageRequest(
    int VisitId,
    string Symptoms,
    int? DepartmentId,
    int? DoctorId,
    string? ChangedByUserId
);

public sealed record TriageResult(
    int VisitId,
    int DepartmentId,
    int? DoctorId
);
