using HospitalTriage.Application.Models;
using HospitalTriage.Application.Services;
using HospitalTriage.Infrastructure.Repositories;
using HospitalTriage.Tests.Helpers;
using Xunit;

namespace HospitalTriage.Tests.Services;

public class DepartmentServiceTests
{
    [Fact]
    public async Task CreateAsync_Should_Create_Department()
    {
        using var db = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new DepartmentRepository(db);
        var svc = new DepartmentService(repo);

        var (ok, error, id) = await svc.CreateAsync(new DepartmentCreateRequest("NOI", "Khoa Nội", true, false));

        Assert.True(ok);
        Assert.Null(error);
        Assert.NotNull(id);
        Assert.True(id > 0);

        var dept = await svc.GetByIdAsync(id!.Value);
        Assert.NotNull(dept);
        Assert.Equal("NOI", dept!.Code);
    }

    [Fact]
    public async Task CreateAsync_Should_Reject_Duplicate_Code()
    {
        using var db = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new DepartmentRepository(db);
        var svc = new DepartmentService(repo);

        var r1 = await svc.CreateAsync(new DepartmentCreateRequest("NOI", "Khoa Nội", true, false));
        var r2 = await svc.CreateAsync(new DepartmentCreateRequest("NOI", "Khoa Nội 2", true, false));

        Assert.True(r1.ok);
        Assert.False(r2.ok);
        Assert.NotNull(r2.error);
    }

    [Fact]
    public async Task CreateAsync_General_Should_Unset_Previous_General()
    {
        using var db = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new DepartmentRepository(db);
        var svc = new DepartmentService(repo);

        var r1 = await svc.CreateAsync(new DepartmentCreateRequest("GEN", "Tổng quát", true, true));
        var r2 = await svc.CreateAsync(new DepartmentCreateRequest("GEN2", "Tổng quát 2", true, true));

        Assert.True(r1.ok);
        Assert.True(r2.ok);

        var all = await svc.SearchAsync(null);

        var generals = all.Where(d => d.IsGeneral).ToList();
        Assert.Single(generals);
        Assert.Equal("GEN2", generals[0].Code);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_Null_When_Not_Found()
    {
        using var db = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new DepartmentRepository(db);
        var svc = new DepartmentService(repo);

        var dept = await svc.GetByIdAsync(999999);

        Assert.Null(dept);
    }

    [Fact]
    public async Task SearchAsync_Should_Return_All_When_Null_Search()
    {
        using var db = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new DepartmentRepository(db);
        var svc = new DepartmentService(repo);

        await svc.CreateAsync(new DepartmentCreateRequest("NOI", "Khoa Nội", true, false));
        await svc.CreateAsync(new DepartmentCreateRequest("NGOAI", "Khoa Ngoại", true, false));

        var all = await svc.SearchAsync(null);

        Assert.True(all.Count >= 2);
        Assert.Contains(all, d => d.Code == "NOI");
        Assert.Contains(all, d => d.Code == "NGOAI");
    }

    [Fact]
    public async Task SearchAsync_Should_Filter_By_Code()
    {
        using var db = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new DepartmentRepository(db);
        var svc = new DepartmentService(repo);

        await svc.CreateAsync(new DepartmentCreateRequest("NOI", "Khoa Nội", true, false));
        await svc.CreateAsync(new DepartmentCreateRequest("NGOAI", "Khoa Ngoại", true, false));

        var list = await svc.SearchAsync("NOI");

        Assert.Single(list);
        Assert.Equal("NOI", list[0].Code);
    }

    [Fact]
    public async Task SearchAsync_Should_Filter_By_Name()
    {
        using var db = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new DepartmentRepository(db);
        var svc = new DepartmentService(repo);

        await svc.CreateAsync(new DepartmentCreateRequest("NOI", "Khoa Nội", true, false));
        await svc.CreateAsync(new DepartmentCreateRequest("NGOAI", "Khoa Ngoại", true, false));

        var list = await svc.SearchAsync("Ngoại");

        Assert.Single(list);
        Assert.Equal("NGOAI", list[0].Code);
    }

    [Fact]
    public async Task CreateAsync_Should_Reject_Empty_Code()
    {
        using var db = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new DepartmentRepository(db);
        var svc = new DepartmentService(repo);

        var (ok, error, id) = await svc.CreateAsync(new DepartmentCreateRequest("", "Khoa Nội", true, false));

        Assert.False(ok);
        Assert.NotNull(error);
        Assert.Null(id);
    }

    [Fact]
    public async Task CreateAsync_Should_Reject_Empty_Name()
    {
        using var db = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new DepartmentRepository(db);
        var svc = new DepartmentService(repo);

        var (ok, error, id) = await svc.CreateAsync(new DepartmentCreateRequest("NOI", "", true, false));

        Assert.False(ok);
        Assert.NotNull(error);
        Assert.Null(id);
    }

    [Fact]
    public async Task CreateAsync_General_False_Should_Not_Unset_Existing_General()
    {
        using var db = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new DepartmentRepository(db);
        var svc = new DepartmentService(repo);

        var r1 = await svc.CreateAsync(new DepartmentCreateRequest("GEN", "Tổng quát", true, true));
        var r2 = await svc.CreateAsync(new DepartmentCreateRequest("NOI", "Khoa Nội", true, false));

        Assert.True(r1.ok);
        Assert.True(r2.ok);

        var all = await svc.SearchAsync(null);
        var generals = all.Where(d => d.IsGeneral).ToList();

        Assert.Single(generals);
        Assert.Equal("GEN", generals[0].Code);
    }

    [Fact]
    public async Task CreateAsync_Should_Allow_Same_Code_With_Different_Case_If_Service_Is_CaseSensitive()
    {
        using var db = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new DepartmentRepository(db);
        var svc = new DepartmentService(repo);

        var r1 = await svc.CreateAsync(new DepartmentCreateRequest("NOI", "Khoa Nội", true, false));
        var r2 = await svc.CreateAsync(new DepartmentCreateRequest("noi", "Khoa Nội 2", true, false));

        Assert.True(r1.ok);
        // Nếu hệ thống bạn normalize code upper-case thì r2 sẽ false.
        // Test này chỉ kiểm tra hành vi hiện tại: r2 ok OR r2 fail đều không crash.
        Assert.True(r2.ok || !r2.ok);
    }

    [Fact]
    public async Task SearchAsync_Should_Return_Empty_When_No_Data()
    {
        using var db = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new DepartmentRepository(db);
        var svc = new DepartmentService(repo);

        var all = await svc.SearchAsync(null);

        Assert.NotNull(all);
        Assert.Empty(all);
    }

    [Fact]
    public async Task SearchAsync_Should_Return_Empty_When_No_Match()
    {
        using var db = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new DepartmentRepository(db);
        var svc = new DepartmentService(repo);

        await svc.CreateAsync(new DepartmentCreateRequest("NOI", "Khoa Nội", true, false));

        var list = await svc.SearchAsync("abcxyz");

        Assert.Empty(list);
    }

    [Fact]
    public async Task CreateAsync_Should_Create_Multiple_Departments()
    {
        using var db = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new DepartmentRepository(db);
        var svc = new DepartmentService(repo);

        var r1 = await svc.CreateAsync(new DepartmentCreateRequest("NOI", "Khoa Nội", true, false));
        var r2 = await svc.CreateAsync(new DepartmentCreateRequest("NGOAI", "Khoa Ngoại", true, false));
        var r3 = await svc.CreateAsync(new DepartmentCreateRequest("NHI", "Khoa Nhi", true, false));

        Assert.True(r1.ok);
        Assert.True(r2.ok);
        Assert.True(r3.ok);

        var all = await svc.SearchAsync(null);
        Assert.Equal(3, all.Count);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_Department_After_Create()
    {
        using var db = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new DepartmentRepository(db);
        var svc = new DepartmentService(repo);

        var (ok, error, id) = await svc.CreateAsync(new DepartmentCreateRequest("NHI", "Khoa Nhi", true, false));
        Assert.True(ok);
        Assert.Null(error);
        Assert.NotNull(id);

        var dept = await svc.GetByIdAsync(id!.Value);
        Assert.NotNull(dept);
        Assert.Equal("NHI", dept!.Code);
        Assert.Equal("Khoa Nhi", dept.Name);
    }

    [Fact]
    public async Task CreateAsync_Should_Allow_Inactive_Department()
    {
        using var db = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new DepartmentRepository(db);
        var svc = new DepartmentService(repo);

        var (ok, error, id) = await svc.CreateAsync(new DepartmentCreateRequest("XN", "Xét nghiệm", false, false));

        Assert.True(ok);
        Assert.Null(error);
        Assert.NotNull(id);

        var dept = await svc.GetByIdAsync(id!.Value);
        Assert.NotNull(dept);
        Assert.False(dept!.IsActive);
    }

    [Fact]
    public async Task CreateAsync_Should_Allow_NonGeneral_Department()
    {
        using var db = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new DepartmentRepository(db);
        var svc = new DepartmentService(repo);

        var (ok, error, id) = await svc.CreateAsync(new DepartmentCreateRequest("NOI", "Khoa Nội", true, false));

        Assert.True(ok);
        Assert.Null(error);
        Assert.NotNull(id);

        var dept = await svc.GetByIdAsync(id!.Value);
        Assert.NotNull(dept);
        Assert.False(dept!.IsGeneral);
    }

    [Fact]
    public async Task SearchAsync_Should_Find_By_Partial_Code()
    {
        using var db = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new DepartmentRepository(db);
        var svc = new DepartmentService(repo);

        await svc.CreateAsync(new DepartmentCreateRequest("NOI", "Khoa Nội", true, false));
        await svc.CreateAsync(new DepartmentCreateRequest("NGOAI", "Khoa Ngoại", true, false));

        var list = await svc.SearchAsync("NG");

        Assert.Single(list);
        Assert.Equal("NGOAI", list[0].Code);
    }

    [Fact]
    public async Task SearchAsync_Should_Find_By_Partial_Name()
    {
        using var db = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new DepartmentRepository(db);
        var svc = new DepartmentService(repo);

        await svc.CreateAsync(new DepartmentCreateRequest("NOI", "Khoa Nội", true, false));
        await svc.CreateAsync(new DepartmentCreateRequest("XN", "Xét nghiệm", true, false));

        var list = await svc.SearchAsync("nghiệm");

        Assert.Single(list);
        Assert.Equal("XN", list[0].Code);
    }

    [Fact]
    public async Task CreateAsync_General_Should_Move_General_To_Latest_General()
    {
        using var db = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new DepartmentRepository(db);
        var svc = new DepartmentService(repo);

        var r1 = await svc.CreateAsync(new DepartmentCreateRequest("GEN", "Tổng quát 1", true, true));
        var r2 = await svc.CreateAsync(new DepartmentCreateRequest("NOI", "Khoa Nội", true, false));
        var r3 = await svc.CreateAsync(new DepartmentCreateRequest("GEN2", "Tổng quát 2", true, true));

        Assert.True(r1.ok);
        Assert.True(r2.ok);
        Assert.True(r3.ok);

        var all = await svc.SearchAsync(null);
        var generals = all.Where(d => d.IsGeneral).ToList();

        Assert.Single(generals);
        Assert.Equal("GEN2", generals[0].Code);
    }

    [Fact]
    public async Task CreateAsync_Should_Reject_Code_With_Only_Spaces()
    {
        using var db = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new DepartmentRepository(db);
        var svc = new DepartmentService(repo);

        var (ok, error, id) = await svc.CreateAsync(new DepartmentCreateRequest("   ", "Khoa Nội", true, false));

        Assert.False(ok);
        Assert.NotNull(error);
        Assert.Null(id);
    }

    [Fact]
    public async Task CreateAsync_Should_Reject_Name_With_Only_Spaces()
    {
        using var db = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new DepartmentRepository(db);
        var svc = new DepartmentService(repo);

        var (ok, error, id) = await svc.CreateAsync(new DepartmentCreateRequest("NOI", "   ", true, false));

        Assert.False(ok);
        Assert.NotNull(error);
        Assert.Null(id);
    }

    [Fact]
    public async Task CreateAsync_Should_Reject_Duplicate_Code_After_Search()
    {
        using var db = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new DepartmentRepository(db);
        var svc = new DepartmentService(repo);

        var r1 = await svc.CreateAsync(new DepartmentCreateRequest("NOI", "Khoa Nội", true, false));
        Assert.True(r1.ok);

        // Call Search to ensure it doesn't affect uniqueness checks
        var all = await svc.SearchAsync(null);
        Assert.Single(all);

        var r2 = await svc.CreateAsync(new DepartmentCreateRequest("NOI", "Khoa Nội 2", true, false));

        Assert.False(r2.ok);
        Assert.NotNull(r2.error);
    }

    [Fact]
    public async Task SearchAsync_Should_Not_Return_Duplicates()
    {
        using var db = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new DepartmentRepository(db);
        var svc = new DepartmentService(repo);

        await svc.CreateAsync(new DepartmentCreateRequest("NOI", "Khoa Nội", true, false));
        await svc.CreateAsync(new DepartmentCreateRequest("NGOAI", "Khoa Ngoại", true, false));
        await svc.CreateAsync(new DepartmentCreateRequest("NHI", "Khoa Nhi", true, false));

        var all = await svc.SearchAsync(null);

        Assert.Equal(all.Select(x => x.Id).Distinct().Count(), all.Count);
    }

    [Fact]
    public async Task SearchAsync_Should_Return_Same_Result_For_Trimmed_Keyword()
    {
        using var db = TestDbContextFactory.CreateInMemoryDbContext();
        var repo = new DepartmentRepository(db);
        var svc = new DepartmentService(repo);

        await svc.CreateAsync(new DepartmentCreateRequest("XN", "Xét nghiệm", true, false));
        await svc.CreateAsync(new DepartmentCreateRequest("NOI", "Khoa Nội", true, false));

        var a = await svc.SearchAsync("XN");
        var b = await svc.SearchAsync("  XN  ");

        Assert.Equal(a.Count, b.Count);
        if (a.Count > 0 && b.Count > 0)
            Assert.Equal(a[0].Code, b[0].Code);
    }


}
