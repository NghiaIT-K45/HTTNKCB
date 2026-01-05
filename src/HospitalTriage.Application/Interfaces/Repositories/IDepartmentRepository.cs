using HospitalTriage.Domain.Entities;

namespace HospitalTriage.Application.Interfaces.Repositories;

public interface IDepartmentRepository
{
    Task<List<Department>> SearchAsync(string? keyword, CancellationToken ct = default);
    Task<Department?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Department?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<Department?> GetGeneralDepartmentAsync(CancellationToken ct = default);
    Task<bool> CodeExistsAsync(string code, int? excludingId = null, CancellationToken ct = default);

    Task AddAsync(Department entity, CancellationToken ct = default);
    Task UpdateAsync(Department entity, CancellationToken ct = default);

    Task SaveChangesAsync(CancellationToken ct = default);
}
