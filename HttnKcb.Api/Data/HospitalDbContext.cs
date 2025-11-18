using HttnKcb.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace HttnKcb.Api.Data;

public class HospitalDbContext : DbContext
{
    public HospitalDbContext(DbContextOptions<HospitalDbContext> options) : base(options) { }

    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<AppUser> Users => Set<AppUser>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Unique department name
        modelBuilder.Entity<Department>()
            .HasIndex(d => d.Name)
            .IsUnique();

        modelBuilder.Entity<Doctor>()
            .HasOne(d => d.Department)
            .WithMany(p => p.Doctors)
            .HasForeignKey(d => d.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Unique username
        modelBuilder.Entity<AppUser>()
            .HasIndex(u => u.Username)
            .IsUnique();
    }
}

public static class DataSeeder
{
    public static void Seed(HospitalDbContext db)
    {
        if (!db.Departments.Any())
        {
            db.Departments.AddRange(
                new Department { Name = "Khoa Nội", Description = "Khám nội tổng quát" },
                new Department { Name = "Khoa Ngoại", Description = "Ngoại tổng quát" },
                new Department { Name = "Khoa Nhi", Description = "Nhi khoa" }
            );
            db.SaveChanges();
        }

        if (!db.Doctors.Any())
        {
            var dep = db.Departments.First();
            db.Doctors.AddRange(
                new Doctor { FullName = "BS. Nguyễn Văn A", DepartmentId = dep.Id, Email = "a@example.com" },
                new Doctor { FullName = "BS. Trần Thị B", DepartmentId = dep.Id }
            );
            db.SaveChanges();
        }

        if (!db.Users.Any())
        {
            db.Users.AddRange(
                new AppUser { Username = "admin", FullName = "Quản trị", Role = UserRole.Admin, IsActive = true },
                new AppUser { Username = "tiep_tan", FullName = "Tiếp tân", Role = UserRole.Receptionist, IsActive = true }
            );
            db.SaveChanges();
        }
    }
}
