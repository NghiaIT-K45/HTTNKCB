using HospitalTriage.Application.Interfaces.Repositories;
using HospitalTriage.Domain.Entities;
using HospitalTriage.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HospitalTriage.Infrastructure.Repositories;

public sealed class DoctorRepository : IDoctorRepository
{
    private readonly ApplicationDbContext _db;

    public DoctorRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public Task<Doctor?> GetByIdAsync(int id, CancellationToken ct = default)
        => _db.Doctors.Include(x => x.Department).FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<Doctor?> GetByCodeAsync(string code, CancellationToken ct = default)
        => _db.Doctors.Include(x => x.Department).FirstOrDefaultAsync(x => x.Code == code, ct);

    public async Task<bool> CodeExistsAsync(string code, int? excludingId = null, CancellationToken ct = default)
    {
        code = (code ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(code))
            return false;

        var query = _db.Doctors.AsQueryable().Where(x => x.Code == code);
        if (excludingId.HasValue)
            query = query.Where(x => x.Id != excludingId.Value);

        return await query.AnyAsync(ct);
    }

    public async Task<List<Doctor>> SearchAsync(string? keyword, int? departmentId = null, CancellationToken ct = default)
    {
        keyword = keyword?.Trim();

        var query = _db.Doctors.Include(x => x.Department).AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(x =>
                x.Code.Contains(keyword) ||
                x.FullName.Contains(keyword));
        }

        if (departmentId.HasValue)
            query = query.Where(x => x.DepartmentId == departmentId.Value);

        return await query
            .OrderByDescending(x => x.IsActive)
            .ThenBy(x => x.FullName)
            .ToListAsync(ct);
    }

    public Task AddAsync(Doctor entity, CancellationToken ct = default)
    {
        _db.Doctors.Add(entity);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Doctor entity, CancellationToken ct = default)
    {
        _db.Doctors.Update(entity);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}
