using HospitalTriage.Domain.Entities;

namespace HospitalTriage.Application.Interfaces.Repositories;

public interface IDoctorRepository
{
    Task<List<Doctor>> SearchAsync(string? keyword, int? departmentId = null, CancellationToken ct = default);
    Task<Doctor?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Doctor?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<bool> CodeExistsAsync(string code, int? excludingId = null, CancellationToken ct = default);

    Task AddAsync(Doctor entity, CancellationToken ct = default);
    Task UpdateAsync(Doctor entity, CancellationToken ct = default);

    Task SaveChangesAsync(CancellationToken ct = default);
}
