using HospitalTriage.Domain.Enums;

namespace HospitalTriage.Application.Models;

public sealed record VisitCreateRequest(
    int PatientId,
    DateOnly VisitDate
);

public sealed record VisitCreateResult(
    int VisitId,
    int QueueNumber,
    VisitStatus CurrentStatus
);

public sealed record VisitStatusChangeRequest(
    int VisitId,
    VisitStatus NewStatus,
    string? ChangedByUserId
);
