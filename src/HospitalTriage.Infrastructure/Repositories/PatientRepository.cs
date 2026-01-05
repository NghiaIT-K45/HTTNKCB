using HospitalTriage.Application.Interfaces.Repositories;
using HospitalTriage.Domain.Entities;
using HospitalTriage.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HospitalTriage.Infrastructure.Repositories;

public sealed class PatientRepository : IPatientRepository
{
    private readonly ApplicationDbContext _db;

    public PatientRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public Task<Patient?> GetByIdAsync(int id, CancellationToken ct = default)
        => _db.Patients.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<Patient?> FindByIdentityNumberAsync(string identityNumber, CancellationToken ct = default)
        => _db.Patients.FirstOrDefaultAsync(x => x.IdentityNumber == identityNumber, ct);

    public Task<Patient?> FindByBasicInfoAsync(string fullName, DateOnly dateOfBirth, string? phone, CancellationToken ct = default)
    {
        fullName = (fullName ?? string.Empty).Trim();
        phone = phone?.Trim();

        var query = _db.Patients.AsQueryable().Where(x => x.FullName == fullName && x.DateOfBirth == dateOfBirth);
        if (!string.IsNullOrWhiteSpace(phone))
            query = query.Where(x => x.Phone == phone);

        return query.FirstOrDefaultAsync(ct);
    }

    public Task AddAsync(Patient entity, CancellationToken ct = default)
    {
        _db.Patients.Add(entity);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Patient entity, CancellationToken ct = default)
    {
        _db.Patients.Update(entity);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}
