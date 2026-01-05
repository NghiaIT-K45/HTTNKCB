using HospitalTriage.Application.Interfaces.Repositories;
using HospitalTriage.Domain.Entities;
using HospitalTriage.Domain.Enums;
using HospitalTriage.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HospitalTriage.Infrastructure.Repositories;

public sealed class VisitRepository : IVisitRepository
{
    private readonly ApplicationDbContext _db;

    public VisitRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public Task<Visit?> GetByIdAsync(int id, bool includeDetails = false, CancellationToken ct = default)
    {
        var query = _db.Visits.AsQueryable();

        if (includeDetails)
        {
            query = query
                .Include(x => x.Patient)
                .Include(x => x.Department)
                .Include(x => x.Doctor)
                .Include(x => x.StatusHistories);
        }

        return query.FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<int> GetMaxQueueNumberAsync(DateOnly queueDate, CancellationToken ct = default)
    {
        var max = await _db.Visits
            .Where(x => x.QueueDate == queueDate)
            .MaxAsync(x => (int?)x.QueueNumber, ct);

        return max ?? 0;
    }

    public async Task<List<Visit>> GetByStatusAsync(VisitStatus status, int? departmentId = null, CancellationToken ct = default)
    {
        var query = _db.Visits
            .Include(x => x.Patient)
            .Include(x => x.Department)
            .Include(x => x.Doctor)
            .AsQueryable()
            .Where(x => x.CurrentStatus == status);

        if (departmentId.HasValue)
            query = query.Where(x => x.DepartmentId == departmentId.Value);

        return await query
            .OrderBy(x => x.QueueNumber)
            .ToListAsync(ct);
    }

    public async Task<List<Visit>> GetByDateRangeAsync(DateOnly from, DateOnly to, int? departmentId = null, CancellationToken ct = default)
    {
        var query = _db.Visits
            .Include(x => x.StatusHistories)
            .AsQueryable()
            .Where(x => x.VisitDate >= from && x.VisitDate <= to);

        if (departmentId.HasValue)
            query = query.Where(x => x.DepartmentId == departmentId.Value);

        return await query
            .OrderBy(x => x.VisitDate)
            .ThenBy(x => x.QueueNumber)
            .ToListAsync(ct);
    }

    public Task AddAsync(Visit entity, CancellationToken ct = default)
    {
        _db.Visits.Add(entity);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Visit entity, CancellationToken ct = default)
    {
        _db.Visits.Update(entity);
        return Task.CompletedTask;
    }

    public Task AddStatusHistoryAsync(VisitStatusHistory history, CancellationToken ct = default)
    {
        _db.VisitStatusHistories.Add(history);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
    public async Task<IReadOnlyList<HospitalTriage.Domain.Entities.Visit>> GetWaitingTriageAsync(DateOnly date, CancellationToken ct = default)
    {
        return await _db.Visits
            .AsNoTracking()
            .Include(v => v.Patient)
            .Where(v => v.VisitDate == date && v.CurrentStatus == HospitalTriage.Domain.Enums.VisitStatus.WaitingTriage)
            .OrderBy(v => v.QueueNumber)
            .ToListAsync(ct);
    }
    public async Task<List<Visit>> GetWaitingTriageByDateAsync(DateOnly date, CancellationToken ct = default)
    {
        return await _db.Visits
            .AsNoTracking()
            .Include(v => v.Patient)
            .Include(v => v.Department)
            .Include(v => v.Doctor)
            .Where(v => v.VisitDate == date && v.CurrentStatus == VisitStatus.WaitingTriage)
            .OrderBy(v => v.QueueNumber)
            .ToListAsync(ct);
    }

}

