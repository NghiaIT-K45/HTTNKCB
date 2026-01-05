using HospitalTriage.Application.Interfaces.Repositories;
using HospitalTriage.Application.Interfaces.Services;

namespace HospitalTriage.Application.Rules;

public sealed class TriageRuleEngine : ITriageRuleEngine
{
    private readonly ISymptomRuleRepository _repo;

    public TriageRuleEngine(ISymptomRuleRepository repo)
    {
        _repo = repo;
    }

    public async Task<int?> SuggestDepartmentIdAsync(string symptoms, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(symptoms))
            return null;

        var rules = await _repo.GetActiveRulesAsync(ct);

        // Prefer keyword dài hơn để match "đau bụng" trước "đau"
        foreach (var rule in rules.OrderByDescending(r => r.Keyword.Length))
        {
            if (string.IsNullOrWhiteSpace(rule.Keyword))
                continue;

            if (symptoms.Contains(rule.Keyword, StringComparison.OrdinalIgnoreCase))
                return rule.DepartmentId;
        }

        return null;
    }
}
