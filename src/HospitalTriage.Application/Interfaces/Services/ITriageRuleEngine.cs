namespace HospitalTriage.Application.Interfaces.Services;

public interface ITriageRuleEngine
{
    /// <summary>
    /// Gợi ý DepartmentId dựa trên keyword trong triệu chứng.
    /// Trả null nếu không match.
    /// </summary>
    Task<int?> SuggestDepartmentIdAsync(string symptoms, CancellationToken ct = default);
}
