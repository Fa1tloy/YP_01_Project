using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using WebReckrytingSystem.Models;
using WebReckrytingSystem.Services;
using WebReckrytingSystem.Data;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// 1. DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 21))
    ));

// 2. ВСЕ репозитории
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();
builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<IVacancyRepository, VacancyRepository>();
builder.Services.AddScoped<IResumeRepository, ResumeRepository>();
builder.Services.AddScoped<IResumeService, ResumeService>(); // ✅ ДОБАВЛЕНО!
builder.Services.AddScoped<IAdminRepository, AdminRepository>();

// 3. ВСЕ сервисы
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<IVacancyService, VacancyService>();
builder.Services.AddScoped<AdminService, AdminService>();
builder.Services.AddScoped<IVacancySearchService, VacancySearchService>();

// 4. Авторизация и аутентификация
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options => {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/AccessDenied";
    });
builder.Services.AddAuthorization();

// 5. Razor Pages и Controllers (для AJAX)
builder.Services.AddRazorPages();
builder.Services.AddControllers();

var app = builder.Build();

EnsureDatabaseCreated(app);

// 6. Конвейер HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// 7. Маршруты
app.MapRazorPages();
app.MapControllers();

app.Run();

static void EnsureDatabaseCreated(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var connectionString = config.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

    var csBuilder = new MySqlConnectionStringBuilder(connectionString);
    var databaseName = csBuilder.Database;

    if (!string.IsNullOrWhiteSpace(databaseName))
    {
        var serverConnectionBuilder = new MySqlConnectionStringBuilder(connectionString)
        {
            Database = string.Empty
        };

        using var serverConnection = new MySqlConnection(serverConnectionBuilder.ConnectionString);
        serverConnection.Open();

        using var command = serverConnection.CreateCommand();
        command.CommandText = $"CREATE DATABASE IF NOT EXISTS `{databaseName}` CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;";
        command.ExecuteNonQuery();
    }

    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.EnsureCreated();

    EnsureSchemaColumns(connectionString);
}

static void EnsureSchemaColumns(string connectionString)
{
    using var connection = new MySqlConnection(connectionString);
    connection.Open();

    EnsureColumnExists(connection, "users", "company_name", "VARCHAR(255) NULL");

    EnsureColumnExists(connection, "resumes", "city", "VARCHAR(100) NOT NULL DEFAULT ''");
    EnsureColumnExists(connection, "resumes", "business_trip_readiness", "VARCHAR(20) NOT NULL DEFAULT ''");
    EnsureColumnExists(connection, "resumes", "search_status", "VARCHAR(50) NOT NULL DEFAULT ''");
    EnsureColumnExists(connection, "resumes", "age", "INT NULL");
    EnsureColumnExists(connection, "resumes", "employment_type", "VARCHAR(50) NOT NULL DEFAULT ''");
    EnsureColumnExists(connection, "resumes", "work_schedule", "VARCHAR(50) NOT NULL DEFAULT ''");
    EnsureColumnExists(connection, "resumes", "specialty", "VARCHAR(255) NOT NULL DEFAULT ''");
    EnsureColumnExists(connection, "resumes", "gender", "VARCHAR(20) NOT NULL DEFAULT ''");
    EnsureColumnExists(connection, "resumes", "has_car", "TINYINT(1) NOT NULL DEFAULT 0");
    EnsureColumnExists(connection, "resumes", "driver_license_category", "VARCHAR(20) NULL");
}

static void EnsureColumnExists(MySqlConnection connection, string tableName, string columnName, string columnDefinition)
{
    using var existsCommand = connection.CreateCommand();
    existsCommand.CommandText = @"SELECT COUNT(*)
                                 FROM information_schema.columns
                                 WHERE table_schema = DATABASE()
                                   AND table_name = @tableName
                                   AND column_name = @columnName;";
    existsCommand.Parameters.AddWithValue("@tableName", tableName);
    existsCommand.Parameters.AddWithValue("@columnName", columnName);

    var exists = Convert.ToInt32(existsCommand.ExecuteScalar()) > 0;
    if (exists)
    {
        return;
    }

    using var alterCommand = connection.CreateCommand();
    alterCommand.CommandText = $"ALTER TABLE `{tableName}` ADD COLUMN `{columnName}` {columnDefinition};";
    alterCommand.ExecuteNonQuery();
}
