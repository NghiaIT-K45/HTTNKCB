using HospitalTriage.Application.Interfaces.Repositories;
using HospitalTriage.Application.Interfaces.Services;
using HospitalTriage.Application.Models;
using HospitalTriage.Domain.Entities;

namespace HospitalTriage.Application.Services;

public sealed class DepartmentService : IDepartmentService
{
    private readonly IDepartmentRepository _repo;

    public DepartmentService(IDepartmentRepository repo)
    {
        _repo = repo;
    }

    public Task<List<Department>> SearchAsync(string? keyword, CancellationToken ct = default)
        => _repo.SearchAsync(keyword, ct);

    public Task<Department?> GetByIdAsync(int id, CancellationToken ct = default)
        => _repo.GetByIdAsync(id, ct);

    public Task<Department?> GetGeneralDepartmentAsync(CancellationToken ct = default)
        => _repo.GetGeneralDepartmentAsync(ct);

    public async Task<(bool ok, string? error, int? id)> CreateAsync(DepartmentCreateRequest request, CancellationToken ct = default)
    {
        var code = (request.Code ?? string.Empty).Trim();
        var name = (request.Name ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(code))
            return (false, "Mã khoa (Code) là bắt buộc.", null);

        if (string.IsNullOrWhiteSpace(name))
            return (false, "Tên khoa (Name) là bắt buộc.", null);

        if (await _repo.CodeExistsAsync(code, excludingId: null, ct))
            return (false, "Mã khoa đã tồn tại.", null);

        // Nếu tạo khoa General => đảm bảo chỉ có 1 khoa general
        if (request.IsGeneral)
        {
            var currentGeneral = await _repo.GetGeneralDepartmentAsync(ct);
            if (currentGeneral is not null)
            {
                currentGeneral.IsGeneral = false;
                await _repo.UpdateAsync(currentGeneral, ct);
            }
        }

        var entity = new Department
        {
            Code = code,
            Name = name,
            IsActive = request.IsActive,
            IsGeneral = request.IsGeneral
        };

        await _repo.AddAsync(entity, ct);
        await _repo.SaveChangesAsync(ct);

        return (true, null, entity.Id);
    }

    public async Task<(bool ok, string? error)> UpdateAsync(DepartmentUpdateRequest request, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(request.Id, ct);
        if (entity is null)
            return (false, "Không tìm thấy khoa.");

        var code = (request.Code ?? string.Empty).Trim();
        var name = (request.Name ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(code))
            return (false, "Mã khoa (Code) là bắt buộc.");

        if (string.IsNullOrWhiteSpace(name))
            return (false, "Tên khoa (Name) là bắt buộc.");

        if (await _repo.CodeExistsAsync(code, excludingId: request.Id, ct))
            return (false, "Mã khoa đã tồn tại.");

        if (request.IsGeneral)
        {
            var currentGeneral = await _repo.GetGeneralDepartmentAsync(ct);
            if (currentGeneral is not null && currentGeneral.Id != entity.Id)
            {
                currentGeneral.IsGeneral = false;
                await _repo.UpdateAsync(currentGeneral, ct);
            }
        }

        entity.Code = code;
        entity.Name = name;
        entity.IsActive = request.IsActive;
        entity.IsGeneral = request.IsGeneral;

        await _repo.UpdateAsync(entity, ct);
        await _repo.SaveChangesAsync(ct);

        return (true, null);
    }

    public async Task<(bool ok, string? error)> DeactivateAsync(int id, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity is null)
            return (false, "Không tìm thấy khoa.");

        entity.IsActive = false;
        entity.IsGeneral = entity.IsGeneral; // giữ nguyên

        await _repo.UpdateAsync(entity, ct);
        await _repo.SaveChangesAsync(ct);

        return (true, null);
    }
}
