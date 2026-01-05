using HospitalTriage.Application.Models;
using HospitalTriage.Domain.Entities;

namespace HospitalTriage.Application.Interfaces.Services;

public interface IDepartmentService
{
    Task<List<Department>> SearchAsync(string? keyword, CancellationToken ct = default);
    Task<Department?> GetByIdAsync(int id, CancellationToken ct = default);

    Task<(bool ok, string? error, int? id)> CreateAsync(DepartmentCreateRequest request, CancellationToken ct = default);
    Task<(bool ok, string? error)> UpdateAsync(DepartmentUpdateRequest request, CancellationToken ct = default);
    Task<(bool ok, string? error)> DeactivateAsync(int id, CancellationToken ct = default);

    Task<Department?> GetGeneralDepartmentAsync(CancellationToken ct = default);
}
