using HospitalTriage.Application.Interfaces.Repositories;
using HospitalTriage.Application.Interfaces.Services;
using HospitalTriage.Application.Models;
using HospitalTriage.Domain.Entities;

namespace HospitalTriage.Application.Services;

public sealed class DoctorService : IDoctorService
{
    private readonly IDoctorRepository _repo;
    private readonly IDepartmentRepository _deptRepo;

    public DoctorService(IDoctorRepository repo, IDepartmentRepository deptRepo)
    {
        _repo = repo;
        _deptRepo = deptRepo;
    }

    public Task<List<Doctor>> SearchAsync(string? keyword, int? departmentId = null, CancellationToken ct = default)
        => _repo.SearchAsync(keyword, departmentId, ct);

    public Task<Doctor?> GetByIdAsync(int id, CancellationToken ct = default)
        => _repo.GetByIdAsync(id, ct);

    public Task<Doctor?> GetByCodeAsync(string code, CancellationToken ct = default)
        => _repo.GetByCodeAsync(code, ct);

    public async Task<(bool ok, string? error, int? id)> CreateAsync(DoctorCreateRequest request, CancellationToken ct = default)
    {
        var code = (request.Code ?? string.Empty).Trim();
        var fullName = (request.FullName ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(code))
            return (false, "Mã bác sĩ (Code) là bắt buộc.", null);

        if (string.IsNullOrWhiteSpace(fullName))
            return (false, "Họ tên bác sĩ là bắt buộc.", null);

        if (await _repo.CodeExistsAsync(code, excludingId: null, ct))
            return (false, "Mã bác sĩ đã tồn tại.", null);

        var dept = await _deptRepo.GetByIdAsync(request.DepartmentId, ct);
        if (dept is null)
            return (false, "Khoa khám không tồn tại.", null);

        var entity = new Doctor
        {
            Code = code,
            FullName = fullName,
            DepartmentId = request.DepartmentId,
            IsActive = request.IsActive
        };

        await _repo.AddAsync(entity, ct);
        await _repo.SaveChangesAsync(ct);

        return (true, null, entity.Id);
    }

    public async Task<(bool ok, string? error)> UpdateAsync(DoctorUpdateRequest request, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(request.Id, ct);
        if (entity is null)
            return (false, "Không tìm thấy bác sĩ.");

        var code = (request.Code ?? string.Empty).Trim();
        var fullName = (request.FullName ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(code))
            return (false, "Mã bác sĩ (Code) là bắt buộc.");

        if (string.IsNullOrWhiteSpace(fullName))
            return (false, "Họ tên bác sĩ là bắt buộc.");

        if (await _repo.CodeExistsAsync(code, excludingId: request.Id, ct))
            return (false, "Mã bác sĩ đã tồn tại.");

        var dept = await _deptRepo.GetByIdAsync(request.DepartmentId, ct);
        if (dept is null)
            return (false, "Khoa khám không tồn tại.");

        entity.Code = code;
        entity.FullName = fullName;
        entity.DepartmentId = request.DepartmentId;
        entity.IsActive = request.IsActive;

        await _repo.UpdateAsync(entity, ct);
        await _repo.SaveChangesAsync(ct);

        return (true, null);
    }

    public async Task<(bool ok, string? error)> DeactivateAsync(int id, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity is null)
            return (false, "Không tìm thấy bác sĩ.");

        entity.IsActive = false;

        await _repo.UpdateAsync(entity, ct);
        await _repo.SaveChangesAsync(ct);

        return (true, null);
    }
}
