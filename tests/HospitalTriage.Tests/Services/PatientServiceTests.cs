using HospitalTriage.Application.Models;
using HospitalTriage.Application.Services;
using HospitalTriage.Domain.Enums;
using HospitalTriage.Infrastructure.Repositories;
using HospitalTriage.Tests.Helpers;
using Xunit;

namespace HospitalTriage.Tests.Services;

public class PatientServiceTests
{
    [Fact]
    public async Task UpsertAsync_Should_Create_New_Patient_When_Not_Exists()
    {
        using var db = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new PatientRepository(db);
        var svc = new PatientService(repo);

        var req = new PatientUpsertRequest(
            FullName: "Nguyễn Văn A",
            DateOfBirth: new DateOnly(1990, 1, 1),
            Gender: Gender.Male,
            IdentityNumber: "0123456789",
            Phone: "0900000000",
            Address: "HCM",
            InsuranceCode: "BHYT001"
        );

        var (ok, error, result) = await svc.UpsertAsync(req);

        Assert.True(ok);
        Assert.Null(error);
        Assert.NotNull(result);
        Assert.True(result!.IsNew);
        Assert.True(result.PatientId > 0);
    }

    [Fact]
    public async Task UpsertAsync_Should_Update_Existing_Patient_By_IdentityNumber()
    {
        using var db = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new PatientRepository(db);
        var svc = new PatientService(repo);

        var req1 = new PatientUpsertRequest("Nguyễn Văn A", new DateOnly(1990, 1, 1), Gender.Male, "0123", "0900", "Addr1", null);
        var r1 = await svc.UpsertAsync(req1);

        var req2 = new PatientUpsertRequest("Nguyễn Văn A (Updated)", new DateOnly(1990, 1, 1), Gender.Male, "0123", "0900", "Addr2", "BHYT002");
        var r2 = await svc.UpsertAsync(req2);

        Assert.True(r1.ok);
        Assert.True(r2.ok);

        Assert.NotNull(r2.result);
        Assert.False(r2.result!.IsNew);
        Assert.Equal(r1.result!.PatientId, r2.result.PatientId);

        var p = await svc.GetByIdAsync(r2.result.PatientId);
        Assert.NotNull(p);
        Assert.Equal("Nguyễn Văn A (Updated)", p!.FullName);
        Assert.Equal("Addr2", p.Address);
        Assert.Equal("BHYT002", p.InsuranceCode);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_Null_When_Not_Found()
    {
        using var db = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new PatientRepository(db);
        var svc = new PatientService(repo);

        var p = await svc.GetByIdAsync(999999);

        Assert.Null(p);
    }

    [Fact]
    public async Task UpsertAsync_Should_Reject_Empty_FullName()
    {
        using var db = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new PatientRepository(db);
        var svc = new PatientService(repo);

        var req = new PatientUpsertRequest(
            FullName: "",
            DateOfBirth: new DateOnly(1990, 1, 1),
            Gender: Gender.Male,
            IdentityNumber: "0123",
            Phone: "0900",
            Address: "HCM",
            InsuranceCode: null
        );

        var (ok, error, result) = await svc.UpsertAsync(req);

        Assert.False(ok);
        Assert.NotNull(error);
        Assert.Null(result);
    }

    [Fact]
    public async Task UpsertAsync_Should_Reject_Default_DateOfBirth()
    {
        using var db = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new PatientRepository(db);
        var svc = new PatientService(repo);

        var req = new PatientUpsertRequest(
            FullName: "Nguyễn Văn A",
            DateOfBirth: default,
            Gender: Gender.Male,
            IdentityNumber: "0123",
            Phone: "0900",
            Address: "HCM",
            InsuranceCode: null
        );

        var (ok, error, result) = await svc.UpsertAsync(req);

        Assert.False(ok);
        Assert.NotNull(error);
        Assert.Null(result);
    }

    [Fact]
    public async Task UpsertAsync_Should_Create_New_When_IdentityNumber_Is_Null()
    {
        using var db = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new PatientRepository(db);
        var svc = new PatientService(repo);

        var req = new PatientUpsertRequest(
            FullName: "Nguyễn Văn A",
            DateOfBirth: new DateOnly(1990, 1, 1),
            Gender: Gender.Male,
            IdentityNumber: null,
            Phone: "0900",
            Address: "HCM",
            InsuranceCode: null
        );

        var (ok, error, result) = await svc.UpsertAsync(req);

        Assert.True(ok);
        Assert.Null(error);
        Assert.NotNull(result);
        Assert.True(result!.IsNew);
        Assert.True(result.PatientId > 0);
    }

    [Fact]
    public async Task UpsertAsync_Should_Update_By_IdentityNumber_And_Keep_Same_Id()
    {
        using var db = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new PatientRepository(db);
        var svc = new PatientService(repo);

        var r1 = await svc.UpsertAsync(new PatientUpsertRequest(
            "Nguyễn Văn A",
            new DateOnly(1990, 1, 1),
            Gender.Male,
            "9999",
            "0900",
            "Addr1",
            null
        ));

        var r2 = await svc.UpsertAsync(new PatientUpsertRequest(
            "Nguyễn Văn A",
            new DateOnly(1990, 1, 1),
            Gender.Male,
            "9999",
            "0911",
            "Addr2",
            "BHYT-X"
        ));

        Assert.True(r1.ok);
        Assert.True(r2.ok);
        Assert.NotNull(r2.result);
        Assert.False(r2.result!.IsNew);
        Assert.Equal(r1.result!.PatientId, r2.result.PatientId);

        var p = await svc.GetByIdAsync(r2.result.PatientId);
        Assert.NotNull(p);
        Assert.Equal("0911", p!.Phone);
        Assert.Equal("Addr2", p.Address);
        Assert.Equal("BHYT-X", p.InsuranceCode);
    }

    [Fact]
    public async Task UpsertAsync_Same_IdentityNumber_Should_Not_Create_Duplicate()
    {
        using var db = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new PatientRepository(db);
        var svc = new PatientService(repo);

        var r1 = await svc.UpsertAsync(new PatientUpsertRequest(
            "Nguyễn Văn A",
            new DateOnly(1990, 1, 1),
            Gender.Male,
            "0123",
            "0900",
            "Addr1",
            null
        ));

        var r2 = await svc.UpsertAsync(new PatientUpsertRequest(
            "Nguyễn Văn A (Updated)",
            new DateOnly(1990, 1, 1),
            Gender.Male,
            "0123",
            "0900",
            "Addr2",
            null
        ));

        Assert.True(r1.ok);
        Assert.True(r2.ok);
        Assert.Equal(r1.result!.PatientId, r2.result!.PatientId);
    }

    [Fact]
    public async Task UpsertAsync_Should_Allow_Clear_InsuranceCode_To_Null()
    {
        using var db = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new PatientRepository(db);
        var svc = new PatientService(repo);

        var r1 = await svc.UpsertAsync(new PatientUpsertRequest(
            "Nguyễn Văn A",
            new DateOnly(1990, 1, 1),
            Gender.Male,
            "7777",
            "0900",
            "Addr1",
            "BHYT001"
        ));

        var r2 = await svc.UpsertAsync(new PatientUpsertRequest(
            "Nguyễn Văn A",
            new DateOnly(1990, 1, 1),
            Gender.Male,
            "7777",
            "0900",
            "Addr1",
            null
        ));

        Assert.True(r1.ok);
        Assert.True(r2.ok);

        var p = await svc.GetByIdAsync(r2.result!.PatientId);
        Assert.NotNull(p);
        Assert.Null(p!.InsuranceCode);
    }

    [Fact]
    public async Task UpsertAsync_Should_Create_Two_Patients_With_Different_IdentityNumber()
    {
        using var db = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new PatientRepository(db);
        var svc = new PatientService(repo);

        var r1 = await svc.UpsertAsync(new PatientUpsertRequest("A", new DateOnly(1990, 1, 1), Gender.Male, "111", "0900", "HCM", null));
        var r2 = await svc.UpsertAsync(new PatientUpsertRequest("B", new DateOnly(1991, 1, 1), Gender.Female, "222", "0901", "HN", null));

        Assert.True(r1.ok);
        Assert.True(r2.ok);
        Assert.NotEqual(r1.result!.PatientId, r2.result!.PatientId);
    }

    [Fact]
    public async Task UpsertAsync_Should_Update_Phone_When_Same_IdentityNumber()
    {
        using var db = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new PatientRepository(db);
        var svc = new PatientService(repo);

        var r1 = await svc.UpsertAsync(new PatientUpsertRequest("A", new DateOnly(1990, 1, 1), Gender.Male, "111", "0900", "Addr", null));
        var r2 = await svc.UpsertAsync(new PatientUpsertRequest("A", new DateOnly(1990, 1, 1), Gender.Male, "111", "0999", "Addr", null));

        Assert.True(r1.ok);
        Assert.True(r2.ok);

        var p = await svc.GetByIdAsync(r2.result!.PatientId);
        Assert.NotNull(p);
        Assert.Equal("0999", p!.Phone);
    }

    [Fact]
    public async Task UpsertAsync_Should_Update_Address_When_Same_IdentityNumber()
    {
        using var db = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new PatientRepository(db);
        var svc = new PatientService(repo);

        var r1 = await svc.UpsertAsync(new PatientUpsertRequest("A", new DateOnly(1990, 1, 1), Gender.Male, "111", "0900", "Addr1", null));
        var r2 = await svc.UpsertAsync(new PatientUpsertRequest("A", new DateOnly(1990, 1, 1), Gender.Male, "111", "0900", "Addr2", null));

        Assert.True(r1.ok);
        Assert.True(r2.ok);

        var p = await svc.GetByIdAsync(r2.result!.PatientId);
        Assert.NotNull(p);
        Assert.Equal("Addr2", p!.Address);
    }

    [Fact]
    public async Task UpsertAsync_Should_Update_FullName_When_Same_IdentityNumber()
    {
        using var db = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new PatientRepository(db);
        var svc = new PatientService(repo);

        var r1 = await svc.UpsertAsync(new PatientUpsertRequest("A", new DateOnly(1990, 1, 1), Gender.Male, "111", "0900", "Addr", null));
        var r2 = await svc.UpsertAsync(new PatientUpsertRequest("A Updated", new DateOnly(1990, 1, 1), Gender.Male, "111", "0900", "Addr", null));

        Assert.True(r1.ok);
        Assert.True(r2.ok);

        var p = await svc.GetByIdAsync(r2.result!.PatientId);
        Assert.NotNull(p);
        Assert.Equal("A Updated", p!.FullName);
    }

    [Fact]
    public async Task UpsertAsync_Should_Update_InsuranceCode_When_Same_IdentityNumber()
    {
        using var db = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new PatientRepository(db);
        var svc = new PatientService(repo);

        var r1 = await svc.UpsertAsync(new PatientUpsertRequest("A", new DateOnly(1990, 1, 1), Gender.Male, "111", "0900", "Addr", "BHYT001"));
        var r2 = await svc.UpsertAsync(new PatientUpsertRequest("A", new DateOnly(1990, 1, 1), Gender.Male, "111", "0900", "Addr", "BHYT002"));

        Assert.True(r1.ok);
        Assert.True(r2.ok);

        var p = await svc.GetByIdAsync(r2.result!.PatientId);
        Assert.NotNull(p);
        Assert.Equal("BHYT002", p!.InsuranceCode);
    }

    [Fact]
    public async Task UpsertAsync_Should_Update_Gender_When_Same_IdentityNumber()
    {
        using var db = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new PatientRepository(db);
        var svc = new PatientService(repo);

        var r1 = await svc.UpsertAsync(new PatientUpsertRequest("A", new DateOnly(1990, 1, 1), Gender.Male, "ID1", "0900", "Addr", null));
        var r2 = await svc.UpsertAsync(new PatientUpsertRequest("A", new DateOnly(1990, 1, 1), Gender.Female, "ID1", "0900", "Addr", null));

        Assert.True(r1.ok);
        Assert.True(r2.ok);

        var p = await svc.GetByIdAsync(r2.result!.PatientId);
        Assert.NotNull(p);
        Assert.Equal(Gender.Female, p!.Gender);
    }

    [Fact]
    public async Task UpsertAsync_Should_Update_DateOfBirth_When_Same_IdentityNumber()
    {
        using var db = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new PatientRepository(db);
        var svc = new PatientService(repo);

        var r1 = await svc.UpsertAsync(new PatientUpsertRequest("A", new DateOnly(1990, 1, 1), Gender.Male, "ID2", "0900", "Addr", null));
        var r2 = await svc.UpsertAsync(new PatientUpsertRequest("A", new DateOnly(1991, 2, 2), Gender.Male, "ID2", "0900", "Addr", null));

        Assert.True(r1.ok);
        Assert.True(r2.ok);

        var p = await svc.GetByIdAsync(r2.result!.PatientId);
        Assert.NotNull(p);
        Assert.Equal(new DateOnly(1991, 2, 2), p!.DateOfBirth);
    }

    [Fact]
    public async Task UpsertAsync_Should_Store_Null_InsuranceCode_When_Create()
    {
        using var db = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new PatientRepository(db);
        var svc = new PatientService(repo);

        var (ok, error, result) = await svc.UpsertAsync(
            new PatientUpsertRequest("A", new DateOnly(1990, 1, 1), Gender.Male, "ID3", "0900", "Addr", null)
        );

        Assert.True(ok);
        Assert.Null(error);
        Assert.NotNull(result);

        var p = await svc.GetByIdAsync(result!.PatientId);
        Assert.NotNull(p);
        Assert.Null(p!.InsuranceCode);
    }

    [Fact]
    public async Task UpsertAsync_Should_Return_Same_Id_When_Upsert_Same_IdentityNumber_Many_Times()
    {
        using var db = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new PatientRepository(db);
        var svc = new PatientService(repo);

        var r1 = await svc.UpsertAsync(new PatientUpsertRequest("A", new DateOnly(1990, 1, 1), Gender.Male, "ID6", "0900", "Addr1", null));
        var r2 = await svc.UpsertAsync(new PatientUpsertRequest("A2", new DateOnly(1990, 1, 1), Gender.Male, "ID6", "0901", "Addr2", null));
        var r3 = await svc.UpsertAsync(new PatientUpsertRequest("A3", new DateOnly(1990, 1, 1), Gender.Male, "ID6", "0902", "Addr3", null));

        Assert.True(r1.ok);
        Assert.True(r2.ok);
        Assert.True(r3.ok);

        Assert.Equal(r1.result!.PatientId, r2.result!.PatientId);
        Assert.Equal(r1.result!.PatientId, r3.result!.PatientId);
    }

    [Fact]
    public async Task UpsertAsync_Should_Create_New_When_IdentityNumber_Is_Empty_String()
    {
        using var db = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new PatientRepository(db);
        var svc = new PatientService(repo);

        var (ok, error, result) = await svc.UpsertAsync(
            new PatientUpsertRequest("A", new DateOnly(1990, 1, 1), Gender.Male, "", "0900", "Addr", null)
        );

        Assert.True(ok);
        Assert.Null(error);
        Assert.NotNull(result);
        Assert.True(result!.PatientId > 0);
    }

    [Fact]
    public async Task UpsertAsync_Should_Not_Create_Duplicate_When_Same_IdentityNumber_And_Different_Name()
    {
        using var db = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new PatientRepository(db);
        var svc = new PatientService(repo);

        var r1 = await svc.UpsertAsync(new PatientUpsertRequest("A", new DateOnly(1990, 1, 1), Gender.Male, "ID-DUP", "0900", "Addr", null));
        var r2 = await svc.UpsertAsync(new PatientUpsertRequest("B", new DateOnly(1990, 1, 1), Gender.Male, "ID-DUP", "0900", "Addr", null));

        Assert.True(r1.ok);
        Assert.True(r2.ok);
        Assert.Equal(r1.result!.PatientId, r2.result!.PatientId);

        var p = await svc.GetByIdAsync(r2.result.PatientId);
        Assert.NotNull(p);
        Assert.Equal("B", p!.FullName);
    }

    [Fact]
    public async Task UpsertAsync_Should_Persist_All_Fields_On_Create()
    {
        using var db = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new PatientRepository(db);
        var svc = new PatientService(repo);

        var (ok, error, result) = await svc.UpsertAsync(new PatientUpsertRequest(
            "Nguyễn Văn A",
            new DateOnly(1990, 1, 1),
            Gender.Male,
            "ID-FULL",
            "0900000000",
            "HCM",
            "BHYT001"
        ));

        Assert.True(ok);
        Assert.Null(error);
        Assert.NotNull(result);

        var p = await svc.GetByIdAsync(result!.PatientId);
        Assert.NotNull(p);
        Assert.Equal("Nguyễn Văn A", p!.FullName);
        Assert.Equal(new DateOnly(1990, 1, 1), p.DateOfBirth);
        Assert.Equal(Gender.Male, p.Gender);
        Assert.Equal("ID-FULL", p.IdentityNumber);
        Assert.Equal("0900000000", p.Phone);
        Assert.Equal("HCM", p.Address);
        Assert.Equal("BHYT001", p.InsuranceCode);
    }

    [Fact]
    public async Task UpsertAsync_Should_Update_All_Fields_On_Update()
    {
        using var db = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new PatientRepository(db);
        var svc = new PatientService(repo);

        var r1 = await svc.UpsertAsync(new PatientUpsertRequest("A", new DateOnly(1990, 1, 1), Gender.Male, "ID-UPD", "0900", "Addr1", "B1"));
        var r2 = await svc.UpsertAsync(new PatientUpsertRequest("A2", new DateOnly(1991, 2, 2), Gender.Female, "ID-UPD", "0999", "Addr2", "B2"));

        Assert.True(r1.ok);
        Assert.True(r2.ok);

        var p = await svc.GetByIdAsync(r2.result!.PatientId);
        Assert.NotNull(p);
        Assert.Equal("A2", p!.FullName);
        Assert.Equal(new DateOnly(1991, 2, 2), p.DateOfBirth);
        Assert.Equal(Gender.Female, p.Gender);
        Assert.Equal("0999", p.Phone);
        Assert.Equal("Addr2", p.Address);
        Assert.Equal("B2", p.InsuranceCode);
    }
}
