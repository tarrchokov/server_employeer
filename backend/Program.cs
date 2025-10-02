using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    var jwtSection = builder.Configuration.GetSection("Jwt");
    var key = Encoding.UTF8.GetBytes(jwtSection.GetValue<string>("Key")!);
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSection.GetValue<string>("Issuer"),
        ValidAudience = jwtSection.GetValue<string>("Audience"),
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole(UserRole.Admin));
});

var app = builder.Build();

// Initialize database with migrations (preserves data)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    // seed admin
    if (!await db.Users.AnyAsync(u => u.Role == UserRole.Admin))
    {
        var admin = new User
        {
            UserName = "admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
            Role = UserRole.Admin
        };
        db.Users.Add(admin);
        await db.SaveChangesAsync();
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

// Auth endpoints
app.MapPost("/api/auth/register", async (RegisterRequest request, AppDbContext db) =>
{
    if (await db.Users.AnyAsync(u => u.UserName == request.UserName))
    {
        return Results.BadRequest("User already exists");
    }
    var user = new User
    {
        UserName = request.UserName,
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
        Role = UserRole.User
    };
    db.Users.Add(user);
    await db.SaveChangesAsync();
    return Results.Ok();
});

app.MapPost("/api/auth/login", async (LoginRequest request, AppDbContext db, IOptions<JwtOptions> jwtOptions) =>
{
    var user = await db.Users.FirstOrDefaultAsync(u => u.UserName == request.UserName);
    if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
    {
        return Results.Unauthorized();
    }

    var opts = jwtOptions.Value;
    Console.WriteLine($"JWT Key length: {opts.Key.Length} characters");
    Console.WriteLine($"JWT Key bytes: {Encoding.UTF8.GetBytes(opts.Key).Length} bytes");
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(opts.Key));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
        new Claim(ClaimTypes.Name, user.UserName),
        new Claim(ClaimTypes.Role, user.Role)
    };

    var token = new JwtSecurityToken(
        issuer: opts.Issuer,
        audience: opts.Audience,
        claims: claims,
        expires: DateTime.UtcNow.AddHours(8),
        signingCredentials: creds
    );

    var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
    return Results.Ok(new { token = tokenString, role = user.Role });
});

// Employee endpoints
app.MapGet("/api/employees", [Authorize] async (AppDbContext db) =>
{
    var list = await db.Employees.AsNoTracking().ToListAsync();
    return Results.Ok(list);
});

app.MapPost("/api/employees", [Authorize(Policy = "AdminOnly")] async (EmployeeCreateRequest dto, AppDbContext db) =>
{
    var employee = new Employee
    {
        Id = Guid.NewGuid(),
        FirstName = dto.FirstName,
        LastName = dto.LastName,
        Position = dto.Position,
        Department = dto.Department,
        Email = dto.Email
    };
    db.Employees.Add(employee);
    await db.SaveChangesAsync();
    return Results.Created($"/api/employees/{employee.Id}", employee);
});

app.MapPut("/api/employees/{id:guid}", [Authorize(Policy = "AdminOnly")] async (Guid id, EmployeeUpdateRequest dto, AppDbContext db) =>
{
    var employee = await db.Employees.FindAsync(id);
    if (employee is null) return Results.NotFound();
    employee.FirstName = dto.FirstName;
    employee.LastName = dto.LastName;
    employee.Position = dto.Position;
    employee.Department = dto.Department;
    employee.Email = dto.Email;
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDelete("/api/employees/{id:guid}", [Authorize(Policy = "AdminOnly")] async (Guid id, AppDbContext db) =>
{
    var employee = await db.Employees.FindAsync(id);
    if (employee is null) return Results.NotFound();
    db.Employees.Remove(employee);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// Reports endpoints
app.MapGet("/api/reports/statistics", [Authorize] async (AppDbContext db) =>
{
    var employees = await db.Employees.AsNoTracking().ToListAsync();
    var departmentBreakdown = employees.GroupBy(e => e.Department).ToDictionary(g => g.Key, g => g.Count());
    var positionBreakdown = employees.GroupBy(e => e.FirstName + " " + e.LastName).ToDictionary(g => g.Key, g => g.Count());
    
    var largestDepartment = departmentBreakdown.OrderByDescending(kvp => kvp.Value).FirstOrDefault();
    var mostPopularPosition = positionBreakdown.OrderByDescending(kvp => kvp.Value).FirstOrDefault();
    
    var stats = new
    {
        TotalEmployees = employees.Count,
        TotalDepartments = departmentBreakdown.Count,
        TotalPositions = positionBreakdown.Count,
        DepartmentBreakdown = departmentBreakdown,
        PositionBreakdown = positionBreakdown,
        AverageEmployeesPerDepartment = departmentBreakdown.Count > 0 ? (double)employees.Count / departmentBreakdown.Count : 0,
        LargestDepartment = largestDepartment.Key ?? "Не определен",
        MostPopularPosition = mostPopularPosition.Key ?? "Не определена"
    };
    
    return Results.Ok(stats);
});

app.MapPost("/api/reports", [Authorize] async (ReportRequest request, AppDbContext db) =>
{
    var employees = await db.Employees.AsNoTracking().ToListAsync();
    var reportContent = GenerateReportContent(request.Type, employees);
    
    var reportId = Guid.NewGuid();
    var report = new Report
    {
        Id = reportId,
        Title = request.Title,
        Type = request.Type,
        Content = reportContent,
        CreatedAt = DateTime.UtcNow,
        CreatedBy = "System", // В реальном приложении здесь был бы ID пользователя
        DownloadUrl = $"/api/reports/download/{reportId}"
    };
    
    db.Reports.Add(report);
    await db.SaveChangesAsync();
    
    return Results.Ok(new
    {
        Id = report.Id.ToString(),
        Title = report.Title,
        Content = report.Content,
        CreatedAt = report.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss"),
        DownloadUrl = report.DownloadUrl
    });
});

app.MapGet("/api/reports", [Authorize] async (AppDbContext db) =>
{
    var reports = await db.Reports.AsNoTracking().OrderByDescending(r => r.CreatedAt).ToListAsync();
    
    return Results.Ok(reports.Select(r => new
    {
        Id = r.Id.ToString(),
        Title = r.Title,
        Content = r.Content,
        CreatedAt = r.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss"),
        DownloadUrl = r.DownloadUrl
    }));
});

app.MapGet("/api/reports/{id}", [Authorize] async (string id, AppDbContext db) =>
{
    if (!Guid.TryParse(id, out var reportId))
        return Results.BadRequest("Invalid report ID");
    
    var report = await db.Reports.AsNoTracking().FirstOrDefaultAsync(r => r.Id == reportId);
    if (report is null)
        return Results.NotFound();
    
    return Results.Ok(new
    {
        Id = report.Id.ToString(),
        Title = report.Title,
        Content = report.Content,
        CreatedAt = report.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss"),
        DownloadUrl = report.DownloadUrl
    });
});

app.MapDelete("/api/reports/{id}", [Authorize] async (string id, AppDbContext db) =>
{
    if (!Guid.TryParse(id, out var reportId))
        return Results.BadRequest("Invalid report ID");
    
    var report = await db.Reports.FindAsync(reportId);
    if (report is null)
        return Results.NotFound();
    
    db.Reports.Remove(report);
    await db.SaveChangesAsync();
    
    return Results.NoContent();
});

// Download report endpoint
app.MapGet("/api/reports/download/{id}", [Authorize] async (string id, AppDbContext db) =>
{
    if (!Guid.TryParse(id, out var reportId))
        return Results.BadRequest("Invalid report ID");
    
    var report = await db.Reports.AsNoTracking().FirstOrDefaultAsync(r => r.Id == reportId);
    if (report is null)
        return Results.NotFound();
    
    var fileName = $"{report.Title}_{report.CreatedAt:yyyy-MM-dd}.txt";
    var content = report.Content;
    
    return Results.File(
        System.Text.Encoding.UTF8.GetBytes(content),
        "text/plain",
        fileName
    );
});

string GenerateReportContent(string reportType, List<Employee> employees)
{
    return reportType.ToLower() switch
    {
        "general" => GenerateGeneralReport(employees),
        "department" => GenerateDepartmentReport(employees),
        "position" => GeneratePositionReport(employees),
        "statistics" => GenerateStatisticsReport(employees),
        "export" => GenerateExportReport(employees),
        _ => "Неизвестный тип отчета"
    };
}

string GenerateGeneralReport(List<Employee> employees)
{
    return $"ОБЩИЙ ОТЧЕТ ПО СОТРУДНИКАМ\n\n" +
           $"Всего сотрудников: {employees.Count}\n" +
           $"Дата создания: {DateTime.Now:dd.MM.yyyy HH:mm}\n\n" +
           $"СПИСОК СОТРУДНИКОВ:\n" +
           string.Join("\n", employees.Select((e, i) => $"{i + 1}. {e.FirstName} {e.LastName} - {e.Department} - {e.Email}"));
}

string GenerateDepartmentReport(List<Employee> employees)
{
    var departments = employees.GroupBy(e => e.Department).OrderByDescending(g => g.Count());
    return $"ОТЧЕТ ПО ОТДЕЛАМ\n\n" +
           $"Дата создания: {DateTime.Now:dd.MM.yyyy HH:mm}\n\n" +
           $"СТАТИСТИКА ПО ОТДЕЛАМ:\n" +
           string.Join("\n", departments.Select(d => $"{d.Key}: {d.Count()} сотрудников"));
}

string GeneratePositionReport(List<Employee> employees)
{
    return $"ОТЧЕТ ПО ДОЛЖНОСТЯМ\n\n" +
           $"Дата создания: {DateTime.Now:dd.MM.yyyy HH:mm}\n\n" +
           $"Всего сотрудников: {employees.Count}\n" +
           $"Уникальных должностей: {employees.Select(e => e.FirstName + " " + e.LastName).Distinct().Count()}";
}

string GenerateStatisticsReport(List<Employee> employees)
{
    var departments = employees.GroupBy(e => e.Department);
    return $"СТАТИСТИЧЕСКИЙ ОТЧЕТ\n\n" +
           $"Дата создания: {DateTime.Now:dd.MM.yyyy HH:mm}\n\n" +
           $"ОБЩАЯ СТАТИСТИКА:\n" +
           $"• Всего сотрудников: {employees.Count}\n" +
           $"• Количество отделов: {departments.Count()}\n" +
           $"• Среднее количество сотрудников на отдел: {(departments.Count() > 0 ? (double)employees.Count / departments.Count() : 0):F1}\n\n" +
           $"ПО ОТДЕЛАМ:\n" +
           string.Join("\n", departments.OrderByDescending(d => d.Count()).Select(d => $"• {d.Key}: {d.Count()} сотрудников"));
}

string GenerateExportReport(List<Employee> employees)
{
    return $"ЭКСПОРТ ДАННЫХ\n\n" +
           $"Дата создания: {DateTime.Now:dd.MM.yyyy HH:mm}\n\n" +
           $"Экспортировано {employees.Count} записей сотрудников.\n" +
           $"Формат: CSV\n" +
           $"Кодировка: UTF-8";
}

app.Run();

// Data and options
public sealed class JwtOptions
{
    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
}

public static class UserRole
{
    public const string Admin = "Admin";
    public const string User = "User";
}

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<User> Users => Set<User>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Report> Reports => Set<Report>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasIndex(u => u.UserName)
            .IsUnique();
    }
}

public sealed class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UserName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = UserRole.User;
}

public sealed class Employee
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public sealed class RegisterRequest
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public sealed class LoginRequest
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class EmployeeCreateRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public sealed class EmployeeUpdateRequest : EmployeeCreateRequest { }

public sealed class Report
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
    public string DownloadUrl { get; set; } = string.Empty;
}

public sealed class ReportRequest
{
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
}