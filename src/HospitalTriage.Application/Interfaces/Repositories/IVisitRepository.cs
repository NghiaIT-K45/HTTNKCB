using HospitalTriage.Domain.Entities;
using HospitalTriage.Domain.Enums;

namespace HospitalTriage.Application.Interfaces.Repositories;

public interface IVisitRepository
{
    Task<Visit?> GetByIdAsync(int id, bool includeDetails = false, CancellationToken ct = default);
    Task<int> GetMaxQueueNumberAsync(DateOnly queueDate, CancellationToken ct = default);

    Task<List<Visit>> GetByStatusAsync(VisitStatus status, int? departmentId = null, CancellationToken ct = default);
    Task<List<Visit>> GetByDateRangeAsync(DateOnly from, DateOnly to, int? departmentId = null, CancellationToken ct = default);

    Task AddAsync(Visit entity, CancellationToken ct = default);
    Task UpdateAsync(Visit entity, CancellationToken ct = default);

    Task AddStatusHistoryAsync(VisitStatusHistory history, CancellationToken ct = default);

    Task SaveChangesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<HospitalTriage.Domain.Entities.Visit>> GetWaitingTriageAsync(DateOnly date, CancellationToken ct = default);
    Task<List<Visit>> GetWaitingTriageByDateAsync(DateOnly date, CancellationToken ct = default);


}
