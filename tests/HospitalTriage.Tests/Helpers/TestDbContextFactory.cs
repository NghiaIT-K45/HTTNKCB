using HospitalTriage.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HospitalTriage.Tests.Helpers;

public static class TestDbContextFactory
{
    public static ApplicationDbContext CreateInMemoryDbContext(string? dbName = null)
    {
        dbName ??= Guid.NewGuid().ToString("N");

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(dbName)
            .EnableSensitiveDataLogging()
            .Options;

        return new ApplicationDbContext(options);
    }
}
