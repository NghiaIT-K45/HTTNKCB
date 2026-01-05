using HospitalTriage.Application.Models;
using HospitalTriage.Domain.Entities;

namespace HospitalTriage.Application.Interfaces.Services;

public interface IPatientService
{
    Task<Patient?> GetByIdAsync(int id, CancellationToken ct = default);

    Task<(bool ok, string? error, PatientUpsertResult? result)> UpsertAsync(PatientUpsertRequest request, CancellationToken ct = default);
}
