using HospitalTriage.Application.Models;
using HospitalTriage.Domain.Entities;

namespace HospitalTriage.Application.Interfaces.Services;

public interface IDoctorService
{
    Task<List<Doctor>> SearchAsync(string? keyword, int? departmentId = null, CancellationToken ct = default);
    Task<Doctor?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Doctor?> GetByCodeAsync(string code, CancellationToken ct = default);

    Task<(bool ok, string? error, int? id)> CreateAsync(DoctorCreateRequest request, CancellationToken ct = default);
    Task<(bool ok, string? error)> UpdateAsync(DoctorUpdateRequest request, CancellationToken ct = default);
    Task<(bool ok, string? error)> DeactivateAsync(int id, CancellationToken ct = default);
}
