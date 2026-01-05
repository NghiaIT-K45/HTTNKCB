using HospitalTriage.Application.Interfaces.Repositories;
using HospitalTriage.Application.Interfaces.Services;
using HospitalTriage.Application.Rules;
using HospitalTriage.Application.Services;
using HospitalTriage.Infrastructure.Data;
using HospitalTriage.Infrastructure.Repositories;
// NOTE: DbInitializer is exposed under HospitalTriage.Infrastructure.Data to avoid
// design-time/runtime namespace confusion when running migrations and seeding.
using HospitalTriage.Web.Middleware;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.Host.UseSerilog((ctx, lc) =>
{
    lc.ReadFrom.Configuration(ctx.Configuration)
      .WriteTo.Console()
      .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day);
});

// MVC + Views
builder.Services.AddControllersWithViews();

// Identity UI (Razor Pages)
builder.Services.AddRazorPages();

// DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sql =>
        {
            sql.CommandTimeout(120); // tăng timeout lên 120s
            sql.EnableRetryOnFailure(5); // retry transient errors
        });
});

// Identity (Cookie-based)
builder.Services
    .AddDefaultIdentity<IdentityUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;

        // Minimal password policy for demo
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = 8;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";

    // Logout của Identity (POST /Identity/Account/Logout)
    options.LogoutPath = "/Identity/Account/Logout";

    // Nếu có returnUrl thì dùng param này (mặc định là ReturnUrl)
    options.ReturnUrlParameter = "returnUrl";
});

// Swagger (API only)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Repositories (Infrastructure)
builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
builder.Services.AddScoped<IDoctorRepository, DoctorRepository>();
builder.Services.AddScoped<IPatientRepository, PatientRepository>();
builder.Services.AddScoped<IVisitRepository, VisitRepository>();
builder.Services.AddScoped<ISymptomRuleRepository, SymptomRuleRepository>();

// Services (Application)
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IDoctorService, DoctorService>();
builder.Services.AddScoped<IPatientService, PatientService>();
builder.Services.AddScoped<IVisitService, VisitService>();
builder.Services.AddScoped<ITriageRuleEngine, TriageRuleEngine>();
builder.Services.AddScoped<ITriageService, TriageService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IReportService, ReportService>();

// ApiServices (Web pattern)
builder.Services.AddScoped<HospitalTriage.Web.Services.ApiServices.DepartmentApiService>();
builder.Services.AddScoped<HospitalTriage.Web.Services.ApiServices.DoctorApiService>();
builder.Services.AddScoped<HospitalTriage.Web.Services.ApiServices.PatientApiService>();
builder.Services.AddScoped<HospitalTriage.Web.Services.ApiServices.VisitApiService>();
builder.Services.AddScoped<HospitalTriage.Web.Services.ApiServices.TriageApiService>();
builder.Services.AddScoped<HospitalTriage.Web.Services.ApiServices.DashboardApiService>();
builder.Services.AddScoped<HospitalTriage.Web.Services.ApiServices.ReportApiService>();

var app = builder.Build();

// Global Exception Handling Middleware
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

// Seed data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<ApplicationDbContext>();
    // Ensure schema (including ASP.NET Identity tables) exists before seeding.
    await db.Database.MigrateAsync();

    await DbInitializer.SeedAsync(db, services);
}

app.Run();
