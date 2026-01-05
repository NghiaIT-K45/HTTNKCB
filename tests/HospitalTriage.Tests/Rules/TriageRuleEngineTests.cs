using HospitalTriage.Application.Rules;
using HospitalTriage.Domain.Entities;
using HospitalTriage.Infrastructure.Repositories;
using HospitalTriage.Tests.Helpers;
using Xunit;

namespace HospitalTriage.Tests.Rules;

public class TriageRuleEngineTests
{
    [Fact]
    public async Task SuggestDepartmentIdAsync_Should_Prefer_Longer_Keyword()
    {
        using var db = TestDbContextFactory.CreateInMemoryDbContext();

        // Seed departments
        var d1 = new Department { Code = "D1", Name = "Dept 1", IsActive = true };
        var d2 = new Department { Code = "D2", Name = "Dept 2", IsActive = true };
        db.Departments.AddRange(d1, d2);
        await db.SaveChangesAsync();

        // Seed rules
        db.SymptomRules.AddRange(
            new SymptomRule { Keyword = "đau", DepartmentId = d1.Id, IsActive = true },
            new SymptomRule { Keyword = "đau bụng", DepartmentId = d2.Id, IsActive = true }
        );
        await db.SaveChangesAsync();

        var repo = new SymptomRuleRepository(db);
        var engine = new TriageRuleEngine(repo);

        var deptId = await engine.SuggestDepartmentIdAsync("Bệnh nhân đau bụng dữ dội");

        Assert.Equal(d2.Id, deptId);
    }

    [Fact]
    public async Task SuggestDepartmentIdAsync_Should_Return_Null_When_No_Match()
    {
        using var db = TestDbContextFactory.CreateInMemoryDbContext();

        var d1 = new Department { Code = "D1", Name = "Dept 1", IsActive = true };
        db.Departments.Add(d1);
        await db.SaveChangesAsync();

        db.SymptomRules.Add(new SymptomRule { Keyword = "sốt", DepartmentId = d1.Id, IsActive = true });
        await db.SaveChangesAsync();

        var repo = new SymptomRuleRepository(db);
        var engine = new TriageRuleEngine(repo);

        var deptId = await engine.SuggestDepartmentIdAsync("không liên quan");

        Assert.Null(deptId);
    }
}
