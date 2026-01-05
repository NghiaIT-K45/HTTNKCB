using HospitalTriage.Application.Interfaces.Repositories;
using HospitalTriage.Domain.Entities;
using HospitalTriage.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HospitalTriage.Infrastructure.Repositories;

public sealed class DepartmentRepository : IDepartmentRepository
{
    private readonly ApplicationDbContext _db;

    public DepartmentRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public Task<Department?> GetByIdAsync(int id, CancellationToken ct = default)
        => _db.Departments.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<Department?> GetByCodeAsync(string code, CancellationToken ct = default)
        => _db.Departments.FirstOrDefaultAsync(x => x.Code == code, ct);

    public Task<Department?> GetGeneralDepartmentAsync(CancellationToken ct = default)
        => _db.Departments.FirstOrDefaultAsync(x => x.IsGeneral && x.IsActive, ct);

    public async Task<bool> CodeExistsAsync(string code, int? excludingId = null, CancellationToken ct = default)
    {
        code = (code ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(code))
            return false;

        var query = _db.Departments.AsQueryable().Where(x => x.Code == code);
        if (excludingId.HasValue)
            query = query.Where(x => x.Id != excludingId.Value);

        return await query.AnyAsync(ct);
    }

    public async Task<List<Department>> SearchAsync(string? keyword, CancellationToken ct = default)
    {
        keyword = keyword?.Trim();

        var query = _db.Departments.AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(x =>
                x.Code.Contains(keyword) ||
                x.Name.Contains(keyword));
        }

        return await query
            .OrderByDescending(x => x.IsActive)
            .ThenBy(x => x.Name)
            .ToListAsync(ct);
    }

    public Task AddAsync(Department entity, CancellationToken ct = default)
    {
        _db.Departments.Add(entity);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Department entity, CancellationToken ct = default)
    {
        _db.Departments.Update(entity);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}
