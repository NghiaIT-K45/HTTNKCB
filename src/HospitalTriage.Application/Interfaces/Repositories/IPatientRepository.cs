using HospitalTriage.Domain.Entities;
using HospitalTriage.Domain.Enums;

namespace HospitalTriage.Application.Interfaces.Repositories;

public interface IPatientRepository
{
    Task<Patient?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Patient?> FindByIdentityNumberAsync(string identityNumber, CancellationToken ct = default);
    Task<Patient?> FindByBasicInfoAsync(string fullName, DateOnly dateOfBirth, string? phone, CancellationToken ct = default);

    Task AddAsync(Patient entity, CancellationToken ct = default);
    Task UpdateAsync(Patient entity, CancellationToken ct = default);

    Task SaveChangesAsync(CancellationToken ct = default);
}
