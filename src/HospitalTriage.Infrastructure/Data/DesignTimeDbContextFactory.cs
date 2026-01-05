using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace HospitalTriage.Infrastructure.Data;

public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        // NOTE: chỉ dùng cho design-time (dotnet ef). Khi chạy runtime, lấy connection từ appsettings.
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        // 1) Ưu tiên env var để dev có thể ép đúng DB/instance khi chạy migrations
        var connStr =
    Environment.GetEnvironmentVariable("HOSPITALTRIAGE_CONNECTION")
    ?? "Server=DESKTOP-EP0IOQV;Database=HospitalTriageDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True";

        // 2) Nếu không có env var, đọc ConnectionStrings:DefaultConnection từ Web/appsettings.json
        if (string.IsNullOrWhiteSpace(connStr))
        {
            var cwd = Directory.GetCurrentDirectory();
            var candidates = new[]
            {
                Path.GetFullPath(Path.Combine(cwd, "..", "HospitalTriage.Web")),
                Path.GetFullPath(Path.Combine(cwd, "..", "..", "HospitalTriage.Web")),
                Path.GetFullPath(Path.Combine(cwd, "src", "HospitalTriage.Web")),
            };

            var webDir = candidates.FirstOrDefault(p => File.Exists(Path.Combine(p, "appsettings.json")));
            if (!string.IsNullOrWhiteSpace(webDir))
            {
                var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                var config = new ConfigurationBuilder()
                    .SetBasePath(webDir)
                    .AddJsonFile("appsettings.json", optional: false)
                    .AddJsonFile($"appsettings.{env}.json", optional: true)
                    .Build();

                connStr = config.GetConnectionString("DefaultConnection");
            }
        }

        if (string.IsNullOrWhiteSpace(connStr))
            throw new InvalidOperationException(
                "Không tìm thấy connection string cho design-time. Hãy set env var HOSPITALTRIAGE_CONNECTION hoặc cấu hình ConnectionStrings:DefaultConnection trong HospitalTriage.Web/appsettings.json");

        optionsBuilder.UseSqlServer(connStr);

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
