namespace HospitalTriage.Application.Models;

public sealed record DepartmentCreateRequest(
    string Code,
    string Name,
    bool IsActive,
    bool IsGeneral
);

public sealed record DepartmentUpdateRequest(
    int Id,
    string Code,
    string Name,
    bool IsActive,
    bool IsGeneral
);
