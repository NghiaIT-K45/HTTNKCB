using HospitalTriage.Domain.Entities;
using HospitalTriage.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HospitalTriage.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(ApplicationDbContext db, IServiceProvider services)
    {
        // Apply migrations
        await db.Database.MigrateAsync();

        await SeedRolesAsync(services);
        await SeedAdminUserAsync(services);

        await SeedDepartmentsAsync(db);
        await SeedDoctorsAsync(db);

        await SeedSampleUsersAsync(db, services);

        await SeedSymptomRulesAsync(db);
    }

    private static async Task SeedRolesAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        var roles = new[]
        {
            "Admin",
            "Receptionist",
            "Nurse",
            "Doctor",
            "Manager"
        };

        foreach (var r in roles)
        {
            if (!await roleManager.RoleExistsAsync(r))
                await roleManager.CreateAsync(new IdentityRole(r));
        }
    }

    private static async Task SeedAdminUserAsync(IServiceProvider services)
    {
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

        const string adminEmail = "admin@hospital.local";
        const string adminPassword = "Admin@12345";

        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin is null)
        {
            admin = new IdentityUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            var created = await userManager.CreateAsync(admin, adminPassword);
            if (!created.Succeeded)
            {
                var errors = string.Join("; ", created.Errors.Select(e => e.Description));
                throw new Exception("Không thể tạo Admin user: " + errors);
            }
        }

        if (!await userManager.IsInRoleAsync(admin, "Admin"))
            await userManager.AddToRoleAsync(admin, "Admin");
    }

    private static async Task SeedDepartmentsAsync(ApplicationDbContext db)
    {
        if (await db.Departments.AnyAsync())
            return;

        var depts = new[]
        {
            new Department { Code = "GEN", Name = "Khoa Tổng Quát", IsActive = true, IsGeneral = true },
            new Department { Code = "NOI", Name = "Khoa Nội", IsActive = true },
            new Department { Code = "NGOAI", Name = "Khoa Ngoại", IsActive = true },
            new Department { Code = "NHI", Name = "Khoa Nhi", IsActive = true }
        };

        db.Departments.AddRange(depts);
        await db.SaveChangesAsync();
    }

    private static async Task SeedDoctorsAsync(ApplicationDbContext db)
    {
        if (await db.Doctors.AnyAsync())
            return;

        var noi = await db.Departments.FirstAsync(x => x.Code == "NOI");
        var ngoai = await db.Departments.FirstAsync(x => x.Code == "NGOAI");
        var nhi = await db.Departments.FirstAsync(x => x.Code == "NHI");

        var doctors = new[]
        {
            new Doctor { Code = "DR001", FullName = "BS Nguyễn Văn A", DepartmentId = noi.Id, IsActive = true },
            new Doctor { Code = "DR002", FullName = "BS Trần Thị B", DepartmentId = ngoai.Id, IsActive = true },
            new Doctor { Code = "DR003", FullName = "BS Lê Văn C", DepartmentId = nhi.Id, IsActive = true },
        };

        db.Doctors.AddRange(doctors);
        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Tạo một số user mẫu để test phân quyền nhanh:
    /// - receptionist@hospital.local / Receptionist@12345
    /// - nurse@hospital.local / Nurse@12345
    /// - manager@hospital.local / Manager@12345
    /// - DR001@hospital.local / Doctor@12345 (role Doctor, map theo Doctor.Code=DR001)
    /// </summary>
    private static async Task SeedSampleUsersAsync(ApplicationDbContext db, IServiceProvider services)
    {
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

        await EnsureUserInRoleAsync(userManager,
            email: "receptionist@hospital.local",
            password: "Receptionist@12345",
            role: "Receptionist");

        await EnsureUserInRoleAsync(userManager,
            email: "nurse@hospital.local",
            password: "Nurse@12345",
            role: "Nurse");

        await EnsureUserInRoleAsync(userManager,
            email: "manager@hospital.local",
            password: "Manager@12345",
            role: "Manager");

        // Doctor users: set UserName = Email để login giống các user khác
        var doctors = await db.Doctors.AsNoTracking().Where(d => d.IsActive).ToListAsync();
        foreach (var d in doctors)
        {
            var email = $"{d.Code}@hospital.local";

            await EnsureUserInRoleAsync(userManager,
                email: email,
                password: "Doctor@12345",
                role: "Doctor",
                userName: email); // ✅ IMPORTANT: UserName = Email
        }
    }

    private static async Task EnsureUserInRoleAsync(
    UserManager<IdentityUser> userManager,
    string email,
    string password,
    string role,
    string? userName = null)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user is null)
        {
            user = new IdentityUser
            {
                UserName = userName ?? email,
                Email = email,
                EmailConfirmed = true
            };

            var created = await userManager.CreateAsync(user, password);
            if (!created.Succeeded)
            {
                var errors = string.Join("; ", created.Errors.Select(e => e.Description));
                throw new Exception($"Không thể tạo user '{email}': " + errors);
            }
        }
        else
        {
            // ✅ đảm bảo username đúng (để login đúng theo input)
            var desiredUserName = userName ?? email;
            if (!string.Equals(user.UserName, desiredUserName, StringComparison.Ordinal))
            {
                user.UserName = desiredUserName;
                user.EmailConfirmed = true;

                var updated = await userManager.UpdateAsync(user);
                if (!updated.Succeeded)
                {
                    var errors = string.Join("; ", updated.Errors.Select(e => e.Description));
                    throw new Exception($"Không thể update username '{email}': " + errors);
                }
            }

            // ✅ nếu mật khẩu khác thì reset về password chuẩn
            var okPassword = await userManager.CheckPasswordAsync(user, password);
            if (!okPassword)
            {
                var token = await userManager.GeneratePasswordResetTokenAsync(user);
                var reset = await userManager.ResetPasswordAsync(user, token, password);
                if (!reset.Succeeded)
                {
                    var errors = string.Join("; ", reset.Errors.Select(e => e.Description));
                    throw new Exception($"Không thể reset password '{email}': " + errors);
                }
            }
        }

        if (!await userManager.IsInRoleAsync(user, role))
        {
            var added = await userManager.AddToRoleAsync(user, role);
            if (!added.Succeeded)
            {
                var errors = string.Join("; ", added.Errors.Select(e => e.Description));
                throw new Exception($"Không thể add role '{role}' cho '{email}': " + errors);
            }
        }
    }

    private static async Task SeedSymptomRulesAsync(ApplicationDbContext db)
    {
        if (await db.SymptomRules.AnyAsync())
            return;

        var gen = await db.Departments.FirstAsync(x => x.Code == "GEN");
        var noi = await db.Departments.FirstAsync(x => x.Code == "NOI");
        var ngoai = await db.Departments.FirstAsync(x => x.Code == "NGOAI");
        var nhi = await db.Departments.FirstAsync(x => x.Code == "NHI");

        var rules = new[]
        {
            // Nội
            new SymptomRule { Keyword = "sốt", DepartmentId = noi.Id, IsActive = true },
            new SymptomRule { Keyword = "ho", DepartmentId = noi.Id, IsActive = true },
            new SymptomRule { Keyword = "đau bụng", DepartmentId = noi.Id, IsActive = true },

            // Ngoại
            new SymptomRule { Keyword = "chấn thương", DepartmentId = ngoai.Id, IsActive = true },
            new SymptomRule { Keyword = "gãy", DepartmentId = ngoai.Id, IsActive = true },

            // Nhi
            new SymptomRule { Keyword = "trẻ em", DepartmentId = nhi.Id, IsActive = true },
            new SymptomRule { Keyword = "bé", DepartmentId = nhi.Id, IsActive = true },
        };

        db.SymptomRules.AddRange(rules);
        await db.SaveChangesAsync();
    }
}
