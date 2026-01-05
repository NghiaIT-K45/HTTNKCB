using HospitalTriage.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HospitalTriage.Infrastructure.Data;

public class ApplicationDbContext
    : IdentityDbContext<IdentityUser, IdentityRole, string>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Visit> Visits => Set<Visit>();
    public DbSet<VisitStatusHistory> VisitStatusHistories => Set<VisitStatusHistory>();
    public DbSet<SymptomRule> SymptomRules => Set<SymptomRule>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder); // ✅ BẮT BUỘC cho Identity

        // Các cấu hình Fluent API của Domain entity (nếu có) giữ lại ở đây
        builder.Entity<Department>()
            .HasIndex(x => x.Code)
            .IsUnique();

        builder.Entity<Doctor>()
            .HasIndex(x => x.Code)
            .IsUnique();

        builder.Entity<Patient>()
            .HasIndex(x => x.IdentityNumber);

        // ... các config khác nếu bạn có
    }
}
