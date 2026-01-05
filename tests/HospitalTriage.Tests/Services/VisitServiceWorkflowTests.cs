using HospitalTriage.Application.Models;
using HospitalTriage.Application.Services;
using HospitalTriage.Domain.Entities;
using HospitalTriage.Domain.Enums;
using HospitalTriage.Infrastructure.Repositories;
using HospitalTriage.Tests.Helpers;
using Xunit;

namespace HospitalTriage.Tests.Services;

public class VisitServiceWorkflowTests
{
    [Fact]
    public async Task ChangeStatusAsync_Should_Reject_Invalid_Transition()
    {
        using var db = TestDbContextFactory.CreateInMemoryDbContext();

        var patient = new Patient { FullName = "P1", DateOfBirth = new DateOnly(2000, 1, 1), Gender = Gender.Male };
        db.Patients.Add(patient);
        await db.SaveChangesAsync();

        var visit = new Visit
        {
            PatientId = patient.Id,
            VisitDate = new DateOnly(2025, 12, 24),
            QueueDate = new DateOnly(2025, 12, 24),
            QueueNumber = 1,
            CurrentStatus = VisitStatus.WaitingDoctor
        };
        db.Visits.Add(visit);
        await db.SaveChangesAsync();

        var visitRepo = new VisitRepository(db);
        var patientRepo = new PatientRepository(db);
        var svc = new VisitService(visitRepo, patientRepo);

        // Attempt invalid transition: WaitingDoctor -> Done (skip InExamination)
        var (ok, error) = await svc.ChangeStatusAsync(new VisitStatusChangeRequest(visit.Id, VisitStatus.Done, "u1"));

        Assert.False(ok);
        Assert.NotNull(error);
    }

    [Fact]
    public async Task ChangeStatusAsync_Should_Allow_Valid_Transition()
    {
        using var db = TestDbContextFactory.CreateInMemoryDbContext();

        var patient = new Patient { FullName = "P1", DateOfBirth = new DateOnly(2000, 1, 1), Gender = Gender.Male };
        db.Patients.Add(patient);
        await db.SaveChangesAsync();

        var visit = new Visit
        {
            PatientId = patient.Id,
            VisitDate = new DateOnly(2025, 12, 24),
            QueueDate = new DateOnly(2025, 12, 24),
            QueueNumber = 1,
            CurrentStatus = VisitStatus.WaitingDoctor
        };
        db.Visits.Add(visit);
        await db.SaveChangesAsync();

        var visitRepo = new VisitRepository(db);
        var patientRepo = new PatientRepository(db);
        var svc = new VisitService(visitRepo, patientRepo);

        var (ok, error) = await svc.ChangeStatusAsync(new VisitStatusChangeRequest(visit.Id, VisitStatus.InExamination, "u1"));

        Assert.True(ok);
        Assert.Null(error);

        var updated = await visitRepo.GetByIdAsync(visit.Id, includeDetails: true);
        Assert.NotNull(updated);
        Assert.Equal(VisitStatus.InExamination, updated!.CurrentStatus);
    }

    

    [Fact]
    public async Task ChangeStatusAsync_Should_Return_Error_When_Visit_Not_Found()
    {
        using var db = TestDbContextFactory.CreateInMemoryDbContext();

        var visitRepo = new VisitRepository(db);
        var patientRepo = new PatientRepository(db);
        var svc = new VisitService(visitRepo, patientRepo);

        var (ok, error) = await svc.ChangeStatusAsync(new VisitStatusChangeRequest(999999, VisitStatus.InExamination, "u1"));

        Assert.False(ok);
        Assert.NotNull(error);
    }

    [Fact]
    public async Task ChangeStatusAsync_Should_Not_Change_Status_When_Invalid_Transition()
    {
        using var db = TestDbContextFactory.CreateInMemoryDbContext();

        var patient = new Patient { FullName = "P1", DateOfBirth = new DateOnly(2000, 1, 1), Gender = Gender.Male };
        db.Patients.Add(patient);
        await db.SaveChangesAsync();

        var visit = new Visit
        {
            PatientId = patient.Id,
            VisitDate = new DateOnly(2025, 12, 24),
            QueueDate = new DateOnly(2025, 12, 24),
            QueueNumber = 1,
            CurrentStatus = VisitStatus.WaitingDoctor
        };
        db.Visits.Add(visit);
        await db.SaveChangesAsync();

        var visitRepo = new VisitRepository(db);
        var patientRepo = new PatientRepository(db);
        var svc = new VisitService(visitRepo, patientRepo);

        var (ok, error) = await svc.ChangeStatusAsync(new VisitStatusChangeRequest(visit.Id, VisitStatus.Done, "u1"));

        Assert.False(ok);
        Assert.NotNull(error);

        var updated = await visitRepo.GetByIdAsync(visit.Id, includeDetails: true);
        Assert.NotNull(updated);
        Assert.Equal(VisitStatus.WaitingDoctor, updated!.CurrentStatus);
    }

    [Fact]
    public async Task ChangeStatusAsync_Should_Allow_WaitingDoctor_To_InExamination_To_Done()
    {
        using var db = TestDbContextFactory.CreateInMemoryDbContext();

        var patient = new Patient { FullName = "P1", DateOfBirth = new DateOnly(2000, 1, 1), Gender = Gender.Male };
        db.Patients.Add(patient);
        await db.SaveChangesAsync();

        var visit = new Visit
        {
            PatientId = patient.Id,
            VisitDate = new DateOnly(2025, 12, 24),
            QueueDate = new DateOnly(2025, 12, 24),
            QueueNumber = 1,
            CurrentStatus = VisitStatus.WaitingDoctor
        };
        db.Visits.Add(visit);
        await db.SaveChangesAsync();

        var visitRepo = new VisitRepository(db);
        var patientRepo = new PatientRepository(db);
        var svc = new VisitService(visitRepo, patientRepo);

        var r1 = await svc.ChangeStatusAsync(new VisitStatusChangeRequest(visit.Id, VisitStatus.InExamination, "u1"));
        Assert.True(r1.ok);
        Assert.Null(r1.error);

        var r2 = await svc.ChangeStatusAsync(new VisitStatusChangeRequest(visit.Id, VisitStatus.Done, "u1"));
        Assert.True(r2.ok);
        Assert.Null(r2.error);

        var updated = await visitRepo.GetByIdAsync(visit.Id, includeDetails: true);
        Assert.NotNull(updated);
        Assert.Equal(VisitStatus.Done, updated!.CurrentStatus);
    }

    [Fact]
    public async Task ChangeStatusAsync_Should_Reject_Done_To_InExamination()
    {
        using var db = TestDbContextFactory.CreateInMemoryDbContext();

        var patient = new Patient { FullName = "P1", DateOfBirth = new DateOnly(2000, 1, 1), Gender = Gender.Male };
        db.Patients.Add(patient);
        await db.SaveChangesAsync();

        var visit = new Visit
        {
            PatientId = patient.Id,
            VisitDate = new DateOnly(2025, 12, 24),
            QueueDate = new DateOnly(2025, 12, 24),
            QueueNumber = 1,
            CurrentStatus = VisitStatus.Done
        };
        db.Visits.Add(visit);
        await db.SaveChangesAsync();

        var visitRepo = new VisitRepository(db);
        var patientRepo = new PatientRepository(db);
        var svc = new VisitService(visitRepo, patientRepo);

        var (ok, error) = await svc.ChangeStatusAsync(new VisitStatusChangeRequest(visit.Id, VisitStatus.InExamination, "u1"));

        Assert.False(ok);
        Assert.NotNull(error);
    }

    [Fact]
    public async Task ChangeStatusAsync_Should_Create_StatusHistory_When_Success()
    {
        using var db = TestDbContextFactory.CreateInMemoryDbContext();

        var patient = new Patient { FullName = "P1", DateOfBirth = new DateOnly(2000, 1, 1), Gender = Gender.Male };
        db.Patients.Add(patient);
        await db.SaveChangesAsync();

        var visit = new Visit
        {
            PatientId = patient.Id,
            VisitDate = new DateOnly(2025, 12, 24),
            QueueDate = new DateOnly(2025, 12, 24),
            QueueNumber = 1,
            CurrentStatus = VisitStatus.WaitingDoctor
        };
        db.Visits.Add(visit);
        await db.SaveChangesAsync();

        var visitRepo = new VisitRepository(db);
        var patientRepo = new PatientRepository(db);
        var svc = new VisitService(visitRepo, patientRepo);

        var (ok, error) = await svc.ChangeStatusAsync(new VisitStatusChangeRequest(visit.Id, VisitStatus.InExamination, "u1"));
        Assert.True(ok);
        Assert.Null(error);

        var updated = await visitRepo.GetByIdAsync(visit.Id, includeDetails: true);
        Assert.NotNull(updated);

        Assert.Contains(updated!.StatusHistories, h => h.Status == VisitStatus.InExamination);
    }

    [Fact]
    public async Task ChangeStatusAsync_Should_Reject_Same_Status_Transition()
    {
        using var db = TestDbContextFactory.CreateInMemoryDbContext();

        var patient = new Patient { FullName = "P1", DateOfBirth = new DateOnly(2000, 1, 1), Gender = Gender.Male };
        db.Patients.Add(patient);
        await db.SaveChangesAsync();

        var visit = new Visit
        {
            PatientId = patient.Id,
            VisitDate = new DateOnly(2025, 12, 24),
            QueueDate = new DateOnly(2025, 12, 24),
            QueueNumber = 1,
            CurrentStatus = VisitStatus.WaitingDoctor
        };
        db.Visits.Add(visit);
        await db.SaveChangesAsync();

        var visitRepo = new VisitRepository(db);
        var patientRepo = new PatientRepository(db);
        var svc = new VisitService(visitRepo, patientRepo);

        var (ok, error) = await svc.ChangeStatusAsync(new VisitStatusChangeRequest(visit.Id, VisitStatus.WaitingDoctor, "u1"));

        Assert.False(ok);
        Assert.NotNull(error);
    }

    [Fact]
    public async Task ChangeStatusAsync_Should_Reject_Done_To_Done()
    {
        using var db = TestDbContextFactory.CreateInMemoryDbContext();

        var patient = new Patient { FullName = "P1", DateOfBirth = new DateOnly(2000, 1, 1), Gender = Gender.Male };
        db.Patients.Add(patient);
        await db.SaveChangesAsync();

        var visit = new Visit
        {
            PatientId = patient.Id,
            VisitDate = new DateOnly(2025, 12, 24),
            QueueDate = new DateOnly(2025, 12, 24),
            QueueNumber = 1,
            CurrentStatus = VisitStatus.Done
        };
        db.Visits.Add(visit);
        await db.SaveChangesAsync();

        var visitRepo = new VisitRepository(db);
        var patientRepo = new PatientRepository(db);
        var svc = new VisitService(visitRepo, patientRepo);

        var (ok, error) = await svc.ChangeStatusAsync(new VisitStatusChangeRequest(visit.Id, VisitStatus.Done, "u1"));

        Assert.False(ok);
        Assert.NotNull(error);
    }

    [Fact]
    public async Task ChangeStatusAsync_Should_Add_StatusHistory_For_Done_When_Success()
    {
        using var db = TestDbContextFactory.CreateInMemoryDbContext();

        var patient = new Patient { FullName = "P1", DateOfBirth = new DateOnly(2000, 1, 1), Gender = Gender.Male };
        db.Patients.Add(patient);
        await db.SaveChangesAsync();

        var visit = new Visit
        {
            PatientId = patient.Id,
            VisitDate = new DateOnly(2025, 12, 24),
            QueueDate = new DateOnly(2025, 12, 24),
            QueueNumber = 1,
            CurrentStatus = VisitStatus.InExamination
        };
        db.Visits.Add(visit);
        await db.SaveChangesAsync();

        var visitRepo = new VisitRepository(db);
        var patientRepo = new PatientRepository(db);
        var svc = new VisitService(visitRepo, patientRepo);

        var (ok, error) = await svc.ChangeStatusAsync(new VisitStatusChangeRequest(visit.Id, VisitStatus.Done, "u1"));

        Assert.True(ok);
        Assert.Null(error);

        var updated = await visitRepo.GetByIdAsync(visit.Id, includeDetails: true);
        Assert.NotNull(updated);
        Assert.Contains(updated!.StatusHistories, h => h.Status == VisitStatus.Done);
    }

    [Fact]
    public async Task ChangeStatusAsync_Should_Keep_StatusHistories_When_Invalid_Transition()
    {
        using var db = TestDbContextFactory.CreateInMemoryDbContext();

        var patient = new Patient { FullName = "P1", DateOfBirth = new DateOnly(2000, 1, 1), Gender = Gender.Male };
        db.Patients.Add(patient);
        await db.SaveChangesAsync();

        var visit = new Visit
        {
            PatientId = patient.Id,
            VisitDate = new DateOnly(2025, 12, 24),
            QueueDate = new DateOnly(2025, 12, 24),
            QueueNumber = 1,
            CurrentStatus = VisitStatus.WaitingDoctor
        };
        db.Visits.Add(visit);
        await db.SaveChangesAsync();

        var visitRepo = new VisitRepository(db);
        var patientRepo = new PatientRepository(db);
        var svc = new VisitService(visitRepo, patientRepo);

        var before = await visitRepo.GetByIdAsync(visit.Id, includeDetails: true);
        var beforeCount = before?.StatusHistories?.Count ?? 0;

        var (ok, error) = await svc.ChangeStatusAsync(new VisitStatusChangeRequest(visit.Id, VisitStatus.Done, "u1"));
        Assert.False(ok);
        Assert.NotNull(error);

        var after = await visitRepo.GetByIdAsync(visit.Id, includeDetails: true);
        var afterCount = after?.StatusHistories?.Count ?? 0;

        Assert.Equal(beforeCount, afterCount);
    }

    [Fact]
    public async Task ChangeStatusAsync_Should_Allow_InExamination_To_Done_And_Persist_Status()
    {
        using var db = TestDbContextFactory.CreateInMemoryDbContext();

        var patient = new Patient { FullName = "P1", DateOfBirth = new DateOnly(2000, 1, 1), Gender = Gender.Male };
        db.Patients.Add(patient);
        await db.SaveChangesAsync();

        var visit = new Visit
        {
            PatientId = patient.Id,
            VisitDate = new DateOnly(2025, 12, 24),
            QueueDate = new DateOnly(2025, 12, 24),
            QueueNumber = 1,
            CurrentStatus = VisitStatus.InExamination
        };
        db.Visits.Add(visit);
        await db.SaveChangesAsync();

        var visitRepo = new VisitRepository(db);
        var patientRepo = new PatientRepository(db);
        var svc = new VisitService(visitRepo, patientRepo);

        var (ok, error) = await svc.ChangeStatusAsync(new VisitStatusChangeRequest(visit.Id, VisitStatus.Done, "u1"));

        Assert.True(ok);
        Assert.Null(error);

        var updated = await visitRepo.GetByIdAsync(visit.Id, includeDetails: true);
        Assert.NotNull(updated);
        Assert.Equal(VisitStatus.Done, updated!.CurrentStatus);
    }

    [Fact]
    public async Task ChangeStatusAsync_Should_Reject_WaitingDoctor_To_Done()
    {
        using var db = TestDbContextFactory.CreateInMemoryDbContext();

        var patient = new Patient { FullName = "P1", DateOfBirth = new DateOnly(2000, 1, 1), Gender = Gender.Male };
        db.Patients.Add(patient);
        await db.SaveChangesAsync();

        var visit = new Visit
        {
            PatientId = patient.Id,
            VisitDate = new DateOnly(2025, 12, 24),
            QueueDate = new DateOnly(2025, 12, 24),
            QueueNumber = 1,
            CurrentStatus = VisitStatus.WaitingDoctor
        };
        db.Visits.Add(visit);
        await db.SaveChangesAsync();

        var visitRepo = new VisitRepository(db);
        var patientRepo = new PatientRepository(db);
        var svc = new VisitService(visitRepo, patientRepo);

        var (ok, error) = await svc.ChangeStatusAsync(new VisitStatusChangeRequest(visit.Id, VisitStatus.Done, "u1"));

        Assert.False(ok);
        Assert.NotNull(error);
    }

    [Fact]
    public async Task ChangeStatusAsync_Should_Reject_InExamination_To_WaitingDoctor()
    {
        using var db = TestDbContextFactory.CreateInMemoryDbContext();

        var patient = new Patient { FullName = "P1", DateOfBirth = new DateOnly(2000, 1, 1), Gender = Gender.Male };
        db.Patients.Add(patient);
        await db.SaveChangesAsync();

        var visit = new Visit
        {
            PatientId = patient.Id,
            VisitDate = new DateOnly(2025, 12, 24),
            QueueDate = new DateOnly(2025, 12, 24),
            QueueNumber = 1,
            CurrentStatus = VisitStatus.InExamination
        };
        db.Visits.Add(visit);
        await db.SaveChangesAsync();

        var visitRepo = new VisitRepository(db);
        var patientRepo = new PatientRepository(db);
        var svc = new VisitService(visitRepo, patientRepo);

        var (ok, error) = await svc.ChangeStatusAsync(new VisitStatusChangeRequest(visit.Id, VisitStatus.WaitingDoctor, "u1"));

        Assert.False(ok);
        Assert.NotNull(error);
    }

    [Fact]
    public async Task ChangeStatusAsync_Should_Reject_Done_To_WaitingDoctor()
    {
        using var db = TestDbContextFactory.CreateInMemoryDbContext();

        var patient = new Patient { FullName = "P1", DateOfBirth = new DateOnly(2000, 1, 1), Gender = Gender.Male };
        db.Patients.Add(patient);
        await db.SaveChangesAsync();

        var visit = new Visit
        {
            PatientId = patient.Id,
            VisitDate = new DateOnly(2025, 12, 24),
            QueueDate = new DateOnly(2025, 12, 24),
            QueueNumber = 1,
            CurrentStatus = VisitStatus.Done
        };
        db.Visits.Add(visit);
        await db.SaveChangesAsync();

        var visitRepo = new VisitRepository(db);
        var patientRepo = new PatientRepository(db);
        var svc = new VisitService(visitRepo, patientRepo);

        var (ok, error) = await svc.ChangeStatusAsync(new VisitStatusChangeRequest(visit.Id, VisitStatus.WaitingDoctor, "u1"));

        Assert.False(ok);
        Assert.NotNull(error);
    }

    [Fact]
    public async Task ChangeStatusAsync_Should_Persist_History_For_Two_Steps()
    {
        using var db = TestDbContextFactory.CreateInMemoryDbContext();

        var patient = new Patient { FullName = "P1", DateOfBirth = new DateOnly(2000, 1, 1), Gender = Gender.Male };
        db.Patients.Add(patient);
        await db.SaveChangesAsync();

        var visit = new Visit
        {
            PatientId = patient.Id,
            VisitDate = new DateOnly(2025, 12, 24),
            QueueDate = new DateOnly(2025, 12, 24),
            QueueNumber = 1,
            CurrentStatus = VisitStatus.WaitingDoctor
        };
        db.Visits.Add(visit);
        await db.SaveChangesAsync();

        var visitRepo = new VisitRepository(db);
        var patientRepo = new PatientRepository(db);
        var svc = new VisitService(visitRepo, patientRepo);

        var r1 = await svc.ChangeStatusAsync(new VisitStatusChangeRequest(visit.Id, VisitStatus.InExamination, "u1"));
        var r2 = await svc.ChangeStatusAsync(new VisitStatusChangeRequest(visit.Id, VisitStatus.Done, "u1"));

        Assert.True(r1.ok);
        Assert.True(r2.ok);

        var updated = await visitRepo.GetByIdAsync(visit.Id, includeDetails: true);
        Assert.NotNull(updated);

        var statuses = updated!.StatusHistories.Select(h => h.Status).ToList();
        Assert.Contains(VisitStatus.InExamination, statuses);
        Assert.Contains(VisitStatus.Done, statuses);
    }

    [Fact]
    public async Task ChangeStatusAsync_Should_Not_Add_History_On_Failed_Transition()
    {
        using var db = TestDbContextFactory.CreateInMemoryDbContext();

        var patient = new Patient { FullName = "P1", DateOfBirth = new DateOnly(2000, 1, 1), Gender = Gender.Male };
        db.Patients.Add(patient);
        await db.SaveChangesAsync();

        var visit = new Visit
        {
            PatientId = patient.Id,
            VisitDate = new DateOnly(2025, 12, 24),
            QueueDate = new DateOnly(2025, 12, 24),
            QueueNumber = 1,
            CurrentStatus = VisitStatus.WaitingDoctor
        };
        db.Visits.Add(visit);
        await db.SaveChangesAsync();

        var visitRepo = new VisitRepository(db);
        var patientRepo = new PatientRepository(db);
        var svc = new VisitService(visitRepo, patientRepo);

        var before = await visitRepo.GetByIdAsync(visit.Id, includeDetails: true);
        var beforeCount = before?.StatusHistories?.Count ?? 0;

        var (ok, error) = await svc.ChangeStatusAsync(new VisitStatusChangeRequest(visit.Id, VisitStatus.Done, "u1"));
        Assert.False(ok);
        Assert.NotNull(error);

        var after = await visitRepo.GetByIdAsync(visit.Id, includeDetails: true);
        var afterCount = after?.StatusHistories?.Count ?? 0;

        Assert.Equal(beforeCount, afterCount);
    }

}
