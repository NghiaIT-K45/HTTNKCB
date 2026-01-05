using HospitalTriage.Application.Interfaces.Repositories;
using HospitalTriage.Domain.Entities;
using HospitalTriage.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HospitalTriage.Infrastructure.Repositories;

public sealed class SymptomRuleRepository : ISymptomRuleRepository
{
    private readonly ApplicationDbContext _db;

    public SymptomRuleRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public Task<List<SymptomRule>> GetActiveRulesAsync(CancellationToken ct = default)
        => _db.SymptomRules
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Keyword)
            .ToListAsync(ct);

    public Task AddRangeAsync(IEnumerable<SymptomRule> rules, CancellationToken ct = default)
    {
        _db.SymptomRules.AddRange(rules);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}
