using HospitalTriage.Application.Models;
using HospitalTriage.Application.Rules;
using HospitalTriage.Application.Services;
using HospitalTriage.Domain.Entities;
using HospitalTriage.Domain.Enums;
using HospitalTriage.Infrastructure.Repositories;
using HospitalTriage.Tests.Helpers;
using Xunit;

namespace HospitalTriage.Tests.Services;

public class TriageServiceTests
{
    [Fact]
    public async Task TriageAsync_Should_Set_Department_By_Rule_And_Advance_Workflow()
    {
        using var db = TestDbContextFactory.CreateInMemoryDbContext();

        // Seed departments
        var gen = new Department { Code = "GEN", Name = "General", IsActive = true, IsGeneral = true };
        var noi = new Department { Code = "NOI", Name = "Internal", IsActive = true };
        db.Departments.AddRange(gen, noi);
        await db.SaveChangesAsync();

        // Seed rule: "sốt" -> noi
        db.SymptomRules.Add(new SymptomRule { Keyword = "sốt", DepartmentId = noi.Id, IsActive = true });
        await db.SaveChangesAsync();

        // Seed patient + visit
        var patient = new Patient { FullName = "P1", DateOfBirth = new DateOnly(2000, 1, 1), Gender = Gender.Male };
        db.Patients.Add(patient);
        await db.SaveChangesAsync();

        var visit = new Visit
        {
            PatientId = patient.Id,
            VisitDate = new DateOnly(2025, 12, 24),
            QueueDate = new DateOnly(2025, 12, 24),
            QueueNumber = 1,
            CurrentStatus = VisitStatus.WaitingTriage
        };
        db.Visits.Add(visit);
        await db.SaveChangesAsync();

        // Setup services
        var visitRepo = new VisitRepository(db);
        var deptRepo = new DepartmentRepository(db);
        var ruleRepo = new SymptomRuleRepository(db);
        var engine = new TriageRuleEngine(ruleRepo);

        var svc = new TriageService(visitRepo, deptRepo, engine);

        // Act
        var (ok, error, result) = await svc.TriageAsync(new TriageRequest(
            VisitId: visit.Id,
            Symptoms: "Bệnh nhân sốt cao",
            DepartmentId: null,
            DoctorId: null,
            ChangedByUserId: "u1"
        ));

        Assert.True(ok);
        Assert.Null(error);
        Assert.NotNull(result);
        Assert.Equal(noi.Id, result!.DepartmentId);

        var updated = await visitRepo.GetByIdAsync(visit.Id, includeDetails: true);
        Assert.NotNull(updated);
        Assert.Equal(noi.Id, updated!.DepartmentId);
        Assert.Equal(VisitStatus.WaitingDoctor, updated.CurrentStatus);

        var statuses = updated.StatusHistories.Select(h => h.Status).ToList();
        Assert.Contains(VisitStatus.Triaged, statuses);
        Assert.Contains(VisitStatus.WaitingDoctor, statuses);
    }

    [Fact]
    public async Task TriageAsync_Should_Fallback_To_General_When_No_Rule_Match()
    {
        using var db = TestDbContextFactory.CreateInMemoryDbContext();

        var gen = new Department { Code = "GEN", Name = "General", IsActive = true, IsGeneral = true };
        db.Departments.Add(gen);
        await db.SaveChangesAsync();

        var patient = new Patient { FullName = "P1", DateOfBirth = new DateOnly(2000, 1, 1), Gender = Gender.Male };
        db.Patients.Add(patient);
        await db.SaveChangesAsync();

        var visit = new Visit
        {
            PatientId = patient.Id,
            VisitDate = new DateOnly(2025, 12, 24),
            QueueDate = new DateOnly(2025, 12, 24),
            QueueNumber = 1,
            CurrentStatus = VisitStatus.WaitingTriage
        };
        db.Visits.Add(visit);
        await db.SaveChangesAsync();

        var visitRepo = new VisitRepository(db);
        var deptRepo = new DepartmentRepository(db);
        var ruleRepo = new SymptomRuleRepository(db);
        var engine = new TriageRuleEngine(ruleRepo);

        var svc = new TriageService(visitRepo, deptRepo, engine);

        var (ok, error, result) = await svc.TriageAsync(new TriageRequest(
            visit.Id,
            "triệu chứng không match",
            null,
            null,
            "u1"
        ));

        Assert.True(ok);
        Assert.Null(error);
        Assert.NotNull(result);
        Assert.Equal(gen.Id, result!.DepartmentId);
    }

    [Fact]
    public async Task TriageAsync_Should_Use_Manual_DepartmentId_When_Provided()
    {
        using var db = TestDbContextFactory.CreateInMemoryDbContext();

        // Seed departments
        var gen = new Department { Code = "GEN", Name = "General", IsActive = true, IsGeneral = true };
        var noi = new Department { Code = "NOI", Name = "Internal", IsActive = true };
        db.Departments.AddRange(gen, noi);
        await db.SaveChangesAsync();

        // Seed rule would match -> noi, but we will manually choose gen
        db.SymptomRules.Add(new SymptomRule { Keyword = "sốt", DepartmentId = noi.Id, IsActive = true });
        await db.SaveChangesAsync();

        // Seed patient + visit
        var patient = new Patient { FullName = "P1", DateOfBirth = new DateOnly(2000, 1, 1), Gender = Gender.Male };
        db.Patients.Add(patient);
        await db.SaveChangesAsync();

        var visit = new Visit
        {
            PatientId = patient.Id,
            VisitDate = new DateOnly(2025, 12, 24),
            QueueDate = new DateOnly(2025, 12, 24),
            QueueNumber = 1,
            CurrentStatus = VisitStatus.WaitingTriage
        };
        db.Visits.Add(visit);
        await db.SaveChangesAsync();

        var visitRepo = new VisitRepository(db);
        var deptRepo = new DepartmentRepository(db);
        var ruleRepo = new SymptomRuleRepository(db);
        var engine = new TriageRuleEngine(ruleRepo);

        var svc = new TriageService(visitRepo, deptRepo, engine);

        var (ok, error, result) = await svc.TriageAsync(new TriageRequest(
            VisitId: visit.Id,
            Symptoms: "Bệnh nhân sốt cao",
            DepartmentId: gen.Id,     // manual override
            DoctorId: null,
            ChangedByUserId: "u1"
        ));

        Assert.True(ok);
        Assert.Null(error);
        Assert.NotNull(result);
        Assert.Equal(gen.Id, result!.DepartmentId);

        var updated = await visitRepo.GetByIdAsync(visit.Id, includeDetails: true);
        Assert.NotNull(updated);
        Assert.Equal(gen.Id, updated!.DepartmentId);
        Assert.Equal(VisitStatus.WaitingDoctor, updated.CurrentStatus);
    }

    [Fact]
    public async Task TriageAsync_Should_Not_Match_Inactive_Rule_And_Fallback_To_General()
    {
        using var db = TestDbContextFactory.CreateInMemoryDbContext();

        var gen = new Department { Code = "GEN", Name = "General", IsActive = true, IsGeneral = true };
        var noi = new Department { Code = "NOI", Name = "Internal", IsActive = true };
        db.Departments.AddRange(gen, noi);
        await db.SaveChangesAsync();

        // Inactive rule: should be ignored
        db.SymptomRules.Add(new SymptomRule { Keyword = "sốt", DepartmentId = noi.Id, IsActive = false });
        await db.SaveChangesAsync();

        var patient = new Patient { FullName = "P1", DateOfBirth = new DateOnly(2000, 1, 1), Gender = Gender.Male };
        db.Patients.Add(patient);
        await db.SaveChangesAsync();

        var visit = new Visit
        {
            PatientId = patient.Id,
            VisitDate = new DateOnly(2025, 12, 24),
            QueueDate = new DateOnly(2025, 12, 24),
            QueueNumber = 1,
            CurrentStatus = VisitStatus.WaitingTriage
        };
        db.Visits.Add(visit);
        await db.SaveChangesAsync();

        var visitRepo = new VisitRepository(db);
        var deptRepo = new DepartmentRepository(db);
        var ruleRepo = new SymptomRuleRepository(db);
        var engine = new TriageRuleEngine(ruleRepo);

        var svc = new TriageService(visitRepo, deptRepo, engine);

        var (ok, error, result) = await svc.TriageAsync(new TriageRequest(
            visit.Id,
            "Bệnh nhân sốt cao",
            null,
            null,
            "u1"
        ));

        Assert.True(ok);
        Assert.Null(error);
        Assert.NotNull(result);
        Assert.Equal(gen.Id, result!.DepartmentId);
    }

    [Fact]
    public async Task TriageAsync_Should_Match_CaseInsensitive_Keyword()
    {
        using var db = TestDbContextFactory.CreateInMemoryDbContext();

        var gen = new Department { Code = "GEN", Name = "General", IsActive = true, IsGeneral = true };
        var noi = new Department { Code = "NOI", Name = "Internal", IsActive = true };
        db.Departments.AddRange(gen, noi);
        await db.SaveChangesAsync();

        db.SymptomRules.Add(new SymptomRule { Keyword = "đau đầu", DepartmentId = noi.Id, IsActive = true });
        await db.SaveChangesAsync();

        var patient = new Patient { FullName = "P1", DateOfBirth = new DateOnly(2000, 1, 1), Gender = Gender.Male };
        db.Patients.Add(patient);
        await db.SaveChangesAsync();

        var visit = new Visit
        {
            PatientId = patient.Id,
            VisitDate = new DateOnly(2025, 12, 24),
            QueueDate = new DateOnly(2025, 12, 24),
            QueueNumber = 1,
            CurrentStatus = VisitStatus.WaitingTriage
        };
        db.Visits.Add(visit);
        await db.SaveChangesAsync();

        var visitRepo = new VisitRepository(db);
        var deptRepo = new DepartmentRepository(db);
        var ruleRepo = new SymptomRuleRepository(db);
        var engine = new TriageRuleEngine(ruleRepo);

        var svc = new TriageService(visitRepo, deptRepo, engine);

        var (ok, error, result) = await svc.TriageAsync(new TriageRequest(
            visit.Id,
            "BỆNH NHÂN ĐAU ĐẦU NHIỀU",
            null,
            null,
            "u1"
        ));

        Assert.True(ok);
        Assert.Null(error);
        Assert.NotNull(result);
        Assert.Equal(noi.Id, result!.DepartmentId);
    }

    [Fact]
    public async Task TriageAsync_Should_Return_Error_When_No_General_Department_And_No_Match()
    {
        using var db = TestDbContextFactory.CreateInMemoryDbContext();

        // No general department
        var noi = new Department { Code = "NOI", Name = "Internal", IsActive = true, IsGeneral = false };
        db.Departments.Add(noi);
        await db.SaveChangesAsync();

        var patient = new Patient { FullName = "P1", DateOfBirth = new DateOnly(2000, 1, 1), Gender = Gender.Male };
        db.Patients.Add(patient);
        await db.SaveChangesAsync();

        var visit = new Visit
        {
            PatientId = patient.Id,
            VisitDate = new DateOnly(2025, 12, 24),
            QueueDate = new DateOnly(2025, 12, 24),
            QueueNumber = 1,
            CurrentStatus = VisitStatus.WaitingTriage
        };
        db.Visits.Add(visit);
        await db.SaveChangesAsync();

        var visitRepo = new VisitRepository(db);
        var deptRepo = new DepartmentRepository(db);
        var ruleRepo = new SymptomRuleRepository(db);
        var engine = new TriageRuleEngine(ruleRepo);

        var svc = new TriageService(visitRepo, deptRepo, engine);

        var (ok, error, result) = await svc.TriageAsync(new TriageRequest(
            visit.Id,
            "triệu chứng không match",
            null,
            null,
            "u1"
        ));

        Assert.False(ok);
        Assert.NotNull(error);
        Assert.Null(result);
    }

    [Fact]
    public async Task TriageAsync_Should_Keep_Existing_Department_When_DepartmentId_Provided()
    {
        using var db = TestDbContextFactory.CreateInMemoryDbContext();

        var gen = new Department { Code = "GEN", Name = "General", IsActive = true, IsGeneral = true };
        var noi = new Department { Code = "NOI", Name = "Internal", IsActive = true };
        db.Departments.AddRange(gen, noi);
        await db.SaveChangesAsync();

        db.SymptomRules.Add(new SymptomRule { Keyword = "sốt", DepartmentId = noi.Id, IsActive = true });
        await db.SaveChangesAsync();

        var patient = new Patient { FullName = "P1", DateOfBirth = new DateOnly(2000, 1, 1), Gender = Gender.Male };
        db.Patients.Add(patient);
        await db.SaveChangesAsync();

        var visit = new Visit
        {
            PatientId = patient.Id,
            VisitDate = new DateOnly(2025, 12, 24),
            QueueDate = new DateOnly(2025, 12, 24),
            QueueNumber = 1,
            CurrentStatus = VisitStatus.WaitingTriage
        };
        db.Visits.Add(visit);
        await db.SaveChangesAsync();

        var visitRepo = new VisitRepository(db);
        var deptRepo = new DepartmentRepository(db);
        var ruleRepo = new SymptomRuleRepository(db);
        var engine = new TriageRuleEngine(ruleRepo);
        var svc = new TriageService(visitRepo, deptRepo, engine);

        var (ok, error, result) = await svc.TriageAsync(new TriageRequest(visit.Id, "Bệnh nhân sốt cao", gen.Id, null, "u1"));

        Assert.True(ok);
        Assert.Null(error);
        Assert.NotNull(result);
        Assert.Equal(gen.Id, result!.DepartmentId);
    }

    [Fact]
    public async Task TriageAsync_Should_Return_Error_When_Visit_Not_Found()
    {
        using var db = TestDbContextFactory.CreateInMemoryDbContext();

        var visitRepo = new VisitRepository(db);
        var deptRepo = new DepartmentRepository(db);
        var ruleRepo = new SymptomRuleRepository(db);
        var engine = new TriageRuleEngine(ruleRepo);
        var svc = new TriageService(visitRepo, deptRepo, engine);

        var (ok, error, result) = await svc.TriageAsync(new TriageRequest(999999, "abc", null, null, "u1"));

        Assert.False(ok);
        Assert.NotNull(error);
        Assert.Null(result);
    }

    [Fact]
    public async Task TriageAsync_Should_Fallback_To_General_When_Rule_Inactive()
    {
        using var db = TestDbContextFactory.CreateInMemoryDbContext();

        var gen = new Department { Code = "GEN", Name = "General", IsActive = true, IsGeneral = true };
        var noi = new Department { Code = "NOI", Name = "Internal", IsActive = true };
        db.Departments.AddRange(gen, noi);
        await db.SaveChangesAsync();

        db.SymptomRules.Add(new SymptomRule { Keyword = "sốt", DepartmentId = noi.Id, IsActive = false });
        await db.SaveChangesAsync();

        var patient = new Patient { FullName = "P1", DateOfBirth = new DateOnly(2000, 1, 1), Gender = Gender.Male };
        db.Patients.Add(patient);
        await db.SaveChangesAsync();

        var visit = new Visit
        {
            PatientId = patient.Id,
            VisitDate = new DateOnly(2025, 12, 24),
            QueueDate = new DateOnly(2025, 12, 24),
            QueueNumber = 1,
            CurrentStatus = VisitStatus.WaitingTriage
        };
        db.Visits.Add(visit);
        await db.SaveChangesAsync();

        var visitRepo = new VisitRepository(db);
        var deptRepo = new DepartmentRepository(db);
        var ruleRepo = new SymptomRuleRepository(db);
        var engine = new TriageRuleEngine(ruleRepo);
        var svc = new TriageService(visitRepo, deptRepo, engine);

        var (ok, error, result) = await svc.TriageAsync(new TriageRequest(visit.Id, "Bệnh nhân sốt cao", null, null, "u1"));

        Assert.True(ok);
        Assert.Null(error);
        Assert.NotNull(result);
        Assert.Equal(gen.Id, result!.DepartmentId);
    }

    [Fact]
    public async Task TriageAsync_Should_Match_Keyword_CaseInsensitive()
    {
        using var db = TestDbContextFactory.CreateInMemoryDbContext();

        var gen = new Department { Code = "GEN", Name = "General", IsActive = true, IsGeneral = true };
        var noi = new Department { Code = "NOI", Name = "Internal", IsActive = true };
        db.Departments.AddRange(gen, noi);
        await db.SaveChangesAsync();

        db.SymptomRules.Add(new SymptomRule { Keyword = "đau đầu", DepartmentId = noi.Id, IsActive = true });
        await db.SaveChangesAsync();

        var patient = new Patient { FullName = "P1", DateOfBirth = new DateOnly(2000, 1, 1), Gender = Gender.Male };
        db.Patients.Add(patient);
        await db.SaveChangesAsync();

        var visit = new Visit
        {
            PatientId = patient.Id,
            VisitDate = new DateOnly(2025, 12, 24),
            QueueDate = new DateOnly(2025, 12, 24),
            QueueNumber = 1,
            CurrentStatus = VisitStatus.WaitingTriage
        };
        db.Visits.Add(visit);
        await db.SaveChangesAsync();

        var visitRepo = new VisitRepository(db);
        var deptRepo = new DepartmentRepository(db);
        var ruleRepo = new SymptomRuleRepository(db);
        var engine = new TriageRuleEngine(ruleRepo);
        var svc = new TriageService(visitRepo, deptRepo, engine);

        var (ok, error, result) = await svc.TriageAsync(new TriageRequest(visit.Id, "BỆNH NHÂN ĐAU ĐẦU", null, null, "u1"));

        Assert.True(ok);
        Assert.Null(error);
        Assert.NotNull(result);
        Assert.Equal(noi.Id, result!.DepartmentId);
    }

    [Fact]
    public async Task TriageAsync_Should_Advance_To_WaitingDoctor_When_Success()
    {
        using var db = TestDbContextFactory.CreateInMemoryDbContext();

        var gen = new Department { Code = "GEN", Name = "General", IsActive = true, IsGeneral = true };
        db.Departments.Add(gen);
        await db.SaveChangesAsync();

        var patient = new Patient { FullName = "P1", DateOfBirth = new DateOnly(2000, 1, 1), Gender = Gender.Male };
        db.Patients.Add(patient);
        await db.SaveChangesAsync();

        var visit = new Visit
        {
            PatientId = patient.Id,
            VisitDate = new DateOnly(2025, 12, 24),
            QueueDate = new DateOnly(2025, 12, 24),
            QueueNumber = 1,
            CurrentStatus = VisitStatus.WaitingTriage
        };
        db.Visits.Add(visit);
        await db.SaveChangesAsync();

        var visitRepo = new VisitRepository(db);
        var deptRepo = new DepartmentRepository(db);
        var ruleRepo = new SymptomRuleRepository(db);
        var engine = new TriageRuleEngine(ruleRepo);
        var svc = new TriageService(visitRepo, deptRepo, engine);

        var (ok, error, result) = await svc.TriageAsync(new TriageRequest(visit.Id, "không match", null, null, "u1"));

        Assert.True(ok);
        Assert.Null(error);
        Assert.NotNull(result);

        var updated = await visitRepo.GetByIdAsync(visit.Id, includeDetails: true);
        Assert.NotNull(updated);
        Assert.Equal(VisitStatus.WaitingDoctor, updated!.CurrentStatus);
    }

    [Fact]
    public async Task TriageAsync_Should_Set_DoctorId_When_Provided()
    {
        using var db = TestDbContextFactory.CreateInMemoryDbContext();

        var gen = new Department { Code = "GEN", Name = "General", IsActive = true, IsGeneral = true };
        db.Departments.Add(gen);
        await db.SaveChangesAsync();

        var patient = new Patient { FullName = "P1", DateOfBirth = new DateOnly(2000, 1, 1), Gender = Gender.Male };
        db.Patients.Add(patient);
        await db.SaveChangesAsync();

        var visit = new Visit
        {
            PatientId = patient.Id,
            VisitDate = new DateOnly(2025, 12, 24),
            QueueDate = new DateOnly(2025, 12, 24),
            QueueNumber = 1,
            CurrentStatus = VisitStatus.WaitingTriage
        };
        db.Visits.Add(visit);
        await db.SaveChangesAsync();

        var visitRepo = new VisitRepository(db);
        var deptRepo = new DepartmentRepository(db);
        var ruleRepo = new SymptomRuleRepository(db);
        var engine = new TriageRuleEngine(ruleRepo);
        var svc = new TriageService(visitRepo, deptRepo, engine);

        var (ok, error, result) = await svc.TriageAsync(new TriageRequest(
            VisitId: visit.Id,
            Symptoms: "không match",
            DepartmentId: null,
            DoctorId: 123,
            ChangedByUserId: "u1"
        ));

        Assert.True(ok);
        Assert.Null(error);
        Assert.NotNull(result);

        var updated = await visitRepo.GetByIdAsync(visit.Id, includeDetails: true);
        Assert.NotNull(updated);
        Assert.Equal(123, updated!.DoctorId);
    }

    [Fact]
    public async Task TriageAsync_Should_Not_Require_Rules_When_Fallback_To_General()
    {
        using var db = TestDbContextFactory.CreateInMemoryDbContext();

        var gen = new Department { Code = "GEN", Name = "General", IsActive = true, IsGeneral = true };
        db.Departments.Add(gen);
        await db.SaveChangesAsync();

        var patient = new Patient { FullName = "P1", DateOfBirth = new DateOnly(2000, 1, 1), Gender = Gender.Male };
        db.Patients.Add(patient);
        await db.SaveChangesAsync();

        var visit = new Visit
        {
            PatientId = patient.Id,
            VisitDate = new DateOnly(2025, 12, 24),
            QueueDate = new DateOnly(2025, 12, 24),
            QueueNumber = 1,
            CurrentStatus = VisitStatus.WaitingTriage
        };
        db.Visits.Add(visit);
        await db.SaveChangesAsync();

        var visitRepo = new VisitRepository(db);
        var deptRepo = new DepartmentRepository(db);
        var ruleRepo = new SymptomRuleRepository(db);
        var engine = new TriageRuleEngine(ruleRepo);
        var svc = new TriageService(visitRepo, deptRepo, engine);

        var (ok, error, result) = await svc.TriageAsync(new TriageRequest(visit.Id, "abc", null, null, "u1"));

        Assert.True(ok);
        Assert.Null(error);
        Assert.NotNull(result);
        Assert.Equal(gen.Id, result!.DepartmentId);
    }

    [Fact]
    public async Task TriageAsync_Should_Add_StatusHistory_Triaged()
    {
        using var db = TestDbContextFactory.CreateInMemoryDbContext();

        var gen = new Department { Code = "GEN", Name = "General", IsActive = true, IsGeneral = true };
        db.Departments.Add(gen);
        await db.SaveChangesAsync();

        var patient = new Patient { FullName = "P1", DateOfBirth = new DateOnly(2000, 1, 1), Gender = Gender.Male };
        db.Patients.Add(patient);
        await db.SaveChangesAsync();

        var visit = new Visit
        {
            PatientId = patient.Id,
            VisitDate = new DateOnly(2025, 12, 24),
            QueueDate = new DateOnly(2025, 12, 24),
            QueueNumber = 1,
            CurrentStatus = VisitStatus.WaitingTriage
        };
        db.Visits.Add(visit);
        await db.SaveChangesAsync();

        var visitRepo = new VisitRepository(db);
        var deptRepo = new DepartmentRepository(db);
        var ruleRepo = new SymptomRuleRepository(db);
        var engine = new TriageRuleEngine(ruleRepo);
        var svc = new TriageService(visitRepo, deptRepo, engine);

        var (ok, _, _) = await svc.TriageAsync(new TriageRequest(visit.Id, "abc", null, null, "u1"));
        Assert.True(ok);

        var updated = await visitRepo.GetByIdAsync(visit.Id, includeDetails: true);
        Assert.NotNull(updated);
        Assert.Contains(updated!.StatusHistories, h => h.Status == VisitStatus.Triaged);
    }

    [Fact]
    public async Task TriageAsync_Should_Add_StatusHistory_WaitingDoctor()
    {
        using var db = TestDbContextFactory.CreateInMemoryDbContext();

        var gen = new Department { Code = "GEN", Name = "General", IsActive = true, IsGeneral = true };
        db.Departments.Add(gen);
        await db.SaveChangesAsync();

        var patient = new Patient { FullName = "P1", DateOfBirth = new DateOnly(2000, 1, 1), Gender = Gender.Male };
        db.Patients.Add(patient);
        await db.SaveChangesAsync();

        var visit = new Visit
        {
            PatientId = patient.Id,
            VisitDate = new DateOnly(2025, 12, 24),
            QueueDate = new DateOnly(2025, 12, 24),
            QueueNumber = 1,
            CurrentStatus = VisitStatus.WaitingTriage
        };
        db.Visits.Add(visit);
        await db.SaveChangesAsync();

        var visitRepo = new VisitRepository(db);
        var deptRepo = new DepartmentRepository(db);
        var ruleRepo = new SymptomRuleRepository(db);
        var engine = new TriageRuleEngine(ruleRepo);
        var svc = new TriageService(visitRepo, deptRepo, engine);

        var (ok, _, _) = await svc.TriageAsync(new TriageRequest(visit.Id, "abc", null, null, "u1"));
        Assert.True(ok);

        var updated = await visitRepo.GetByIdAsync(visit.Id, includeDetails: true);
        Assert.NotNull(updated);
        Assert.Contains(updated!.StatusHistories, h => h.Status == VisitStatus.WaitingDoctor);
    }

    [Fact]
    public async Task TriageAsync_Should_Set_DepartmentId_To_General_When_No_Match()
    {
        using var db = TestDbContextFactory.CreateInMemoryDbContext();

        var gen = new Department { Code = "GEN", Name = "General", IsActive = true, IsGeneral = true };
        db.Departments.Add(gen);
        await db.SaveChangesAsync();

        var patient = new Patient { FullName = "P1", DateOfBirth = new DateOnly(2000, 1, 1), Gender = Gender.Male };
        db.Patients.Add(patient);
        await db.SaveChangesAsync();

        var visit = new Visit
        {
            PatientId = patient.Id,
            VisitDate = new DateOnly(2025, 12, 24),
            QueueDate = new DateOnly(2025, 12, 24),
            QueueNumber = 1,
            CurrentStatus = VisitStatus.WaitingTriage
        };
        db.Visits.Add(visit);
        await db.SaveChangesAsync();

        var visitRepo = new VisitRepository(db);
        var deptRepo = new DepartmentRepository(db);
        var ruleRepo = new SymptomRuleRepository(db);
        var engine = new TriageRuleEngine(ruleRepo);
        var svc = new TriageService(visitRepo, deptRepo, engine);

        var (ok, error, result) = await svc.TriageAsync(new TriageRequest(visit.Id, "không match", null, null, "u1"));

        Assert.True(ok);
        Assert.Null(error);
        Assert.NotNull(result);
        Assert.Equal(gen.Id, result!.DepartmentId);
    }

    [Fact]
    public async Task TriageAsync_Should_Keep_DoctorId_Null_When_Not_Provided()
    {
        using var db = TestDbContextFactory.CreateInMemoryDbContext();

        var gen = new Department { Code = "GEN", Name = "General", IsActive = true, IsGeneral = true };
        db.Departments.Add(gen);
        await db.SaveChangesAsync();

        var patient = new Patient { FullName = "P1", DateOfBirth = new DateOnly(2000, 1, 1), Gender = Gender.Male };
        db.Patients.Add(patient);
        await db.SaveChangesAsync();

        var visit = new Visit
        {
            PatientId = patient.Id,
            VisitDate = new DateOnly(2025, 12, 24),
            QueueDate = new DateOnly(2025, 12, 24),
            QueueNumber = 1,
            CurrentStatus = VisitStatus.WaitingTriage
        };
        db.Visits.Add(visit);
        await db.SaveChangesAsync();

        var visitRepo = new VisitRepository(db);
        var deptRepo = new DepartmentRepository(db);
        var ruleRepo = new SymptomRuleRepository(db);
        var engine = new TriageRuleEngine(ruleRepo);
        var svc = new TriageService(visitRepo, deptRepo, engine);

        var (ok, error, _) = await svc.TriageAsync(new TriageRequest(visit.Id, "abc", null, null, "u1"));

        Assert.True(ok);
        Assert.Null(error);

        var updated = await visitRepo.GetByIdAsync(visit.Id, includeDetails: true);
        Assert.NotNull(updated);
        Assert.Null(updated!.DoctorId);
    }

    [Fact]
    public async Task TriageAsync_Should_Set_Department_By_Rule_When_Multiple_Rules_Only_One_Matches()
    {
        using var db = TestDbContextFactory.CreateInMemoryDbContext();

        var gen = new Department { Code = "GEN", Name = "General", IsActive = true, IsGeneral = true };
        var noi = new Department { Code = "NOI", Name = "Internal", IsActive = true };
        var ngoai = new Department { Code = "NGOAI", Name = "Surgery", IsActive = true };
        db.Departments.AddRange(gen, noi, ngoai);
        await db.SaveChangesAsync();

        db.SymptomRules.AddRange(
            new SymptomRule { Keyword = "đau bụng", DepartmentId = ngoai.Id, IsActive = true },
            new SymptomRule { Keyword = "sốt", DepartmentId = noi.Id, IsActive = true }
        );
        await db.SaveChangesAsync();

        var patient = new Patient { FullName = "P1", DateOfBirth = new DateOnly(2000, 1, 1), Gender = Gender.Male };
        db.Patients.Add(patient);
        await db.SaveChangesAsync();

        var visit = new Visit
        {
            PatientId = patient.Id,
            VisitDate = new DateOnly(2025, 12, 24),
            QueueDate = new DateOnly(2025, 12, 24),
            QueueNumber = 1,
            CurrentStatus = VisitStatus.WaitingTriage
        };
        db.Visits.Add(visit);
        await db.SaveChangesAsync();

        var visitRepo = new VisitRepository(db);
        var deptRepo = new DepartmentRepository(db);
        var ruleRepo = new SymptomRuleRepository(db);
        var engine = new TriageRuleEngine(ruleRepo);
        var svc = new TriageService(visitRepo, deptRepo, engine);

        var (ok, error, result) = await svc.TriageAsync(new TriageRequest(visit.Id, "Bệnh nhân sốt cao", null, null, "u1"));

        Assert.True(ok);
        Assert.Null(error);
        Assert.NotNull(result);
        Assert.Equal(noi.Id, result!.DepartmentId);
    }

    [Fact]
    public async Task TriageAsync_Should_Keep_VisitDate_And_QueueNumber_Unchanged()
    {
        using var db = TestDbContextFactory.CreateInMemoryDbContext();

        var gen = new Department { Code = "GEN", Name = "General", IsActive = true, IsGeneral = true };
        db.Departments.Add(gen);
        await db.SaveChangesAsync();

        var patient = new Patient { FullName = "P1", DateOfBirth = new DateOnly(2000, 1, 1), Gender = Gender.Male };
        db.Patients.Add(patient);
        await db.SaveChangesAsync();

        var visit = new Visit
        {
            PatientId = patient.Id,
            VisitDate = new DateOnly(2025, 12, 24),
            QueueDate = new DateOnly(2025, 12, 24),
            QueueNumber = 7,
            CurrentStatus = VisitStatus.WaitingTriage
        };
        db.Visits.Add(visit);
        await db.SaveChangesAsync();

        var visitRepo = new VisitRepository(db);
        var deptRepo = new DepartmentRepository(db);
        var ruleRepo = new SymptomRuleRepository(db);
        var engine = new TriageRuleEngine(ruleRepo);
        var svc = new TriageService(visitRepo, deptRepo, engine);

        var (ok, error, _) = await svc.TriageAsync(new TriageRequest(visit.Id, "abc", null, null, "u1"));

        Assert.True(ok);
        Assert.Null(error);

        var updated = await visitRepo.GetByIdAsync(visit.Id, includeDetails: true);
        Assert.NotNull(updated);
        Assert.Equal(new DateOnly(2025, 12, 24), updated!.VisitDate);
        Assert.Equal(7, updated.QueueNumber);
    }

}
