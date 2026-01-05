namespace HospitalTriage.Application.Models;

public sealed record DoctorCreateRequest(
    string Code,
    string FullName,
    int DepartmentId,
    bool IsActive
);

public sealed record DoctorUpdateRequest(
    int Id,
    string Code,
    string FullName,
    int DepartmentId,
    bool IsActive
);
