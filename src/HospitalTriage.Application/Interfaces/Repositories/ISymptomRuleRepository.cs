using HospitalTriage.Domain.Entities;

namespace HospitalTriage.Application.Interfaces.Repositories;

public interface ISymptomRuleRepository
{
    Task<List<SymptomRule>> GetActiveRulesAsync(CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<SymptomRule> rules, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
