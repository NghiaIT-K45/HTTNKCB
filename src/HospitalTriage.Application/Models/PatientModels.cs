using HospitalTriage.Domain.Enums;

namespace HospitalTriage.Application.Models;

public sealed record PatientUpsertRequest(
    string FullName,
    DateOnly DateOfBirth,
    Gender Gender,
    string? IdentityNumber,
    string? Phone,
    string? Address,
    string? InsuranceCode
);

public sealed record PatientUpsertResult(
    int PatientId,
    bool IsNew
);
