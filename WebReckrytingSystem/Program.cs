// Program.cs (только EnsureSchemaColumns скорректирован)
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using WebReckrytingSystem.Models;
using WebReckrytingSystem.Services;
using WebReckrytingSystem.Data;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 21))
    ));

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();
builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<IVacancyRepository, VacancyRepository>();
builder.Services.AddScoped<IResumeRepository, ResumeRepository>();
builder.Services.AddScoped<IResumeService, ResumeService>();
builder.Services.AddScoped<IAdminRepository, AdminRepository>();

builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<IVacancyService, VacancyService>();
builder.Services.AddScoped<AdminService>();
builder.Services.AddScoped<IAdminService>(sp => sp.GetRequiredService<AdminService>());
builder.Services.AddScoped<IVacancySearchService, VacancySearchService>();
builder.Services.AddScoped<ISpecialtyService, SpecialtyService>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options => {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/AccessDenied";
    });
builder.Services.AddAuthorization();
// Настройка лицензии QuestPDF (бесплатная для revenue < $1M)
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
builder.Services.AddRazorPages();
builder.Services.AddControllers();

var app = builder.Build();

EnsureDatabaseCreated(app);

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

    // таблицы notifications, chat_messages, companies, vacancies (без salary_to, salary_period, payment_frequency)
    EnsureTableExists(connection, "notifications", @"`id` INT NOT NULL AUTO_INCREMENT,
`recipient_email` VARCHAR(255) NOT NULL,
`sender_email` VARCHAR(255) NOT NULL,
`title` VARCHAR(255) NOT NULL,
`message` TEXT NOT NULL,
`link` VARCHAR(500) NULL,
`is_read` TINYINT(1) NOT NULL DEFAULT 0,
`created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
PRIMARY KEY (`id`)");

    EnsureTableExists(connection, "chat_messages", @"`id` INT NOT NULL AUTO_INCREMENT,
`sender_email` VARCHAR(255) NOT NULL,
`recipient_email` VARCHAR(255) NOT NULL,
`message` TEXT NOT NULL,
`vacancy_company_name` VARCHAR(255) NULL,
`vacancy_title` VARCHAR(255) NULL,
`sent_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
`is_read` TINYINT(1) NOT NULL DEFAULT 0,
PRIMARY KEY (`id`)");

    EnsureTableExists(connection, "companies", @"`name` VARCHAR(255) NOT NULL,
`description` TEXT NULL,
`website` TEXT NULL,
`logo_url` TEXT NULL,
`verified` TINYINT(1) NOT NULL DEFAULT 0,
PRIMARY KEY (`name`)");

    EnsureTableExists(connection, "vacancies", @"`company_name` VARCHAR(255) NOT NULL,
`title` VARCHAR(255) NOT NULL,
`region` VARCHAR(100) NOT NULL DEFAULT '',
`description` TEXT NOT NULL,
`requirements` TEXT NOT NULL,
`salary_from` INT NULL,
`employment_type` VARCHAR(50) NOT NULL,
`work_schedule` VARCHAR(50) NOT NULL,
`work_hours_per_day` INT NULL,
`work_format` VARCHAR(50) NOT NULL DEFAULT '',
`specialty` VARCHAR(255) NOT NULL DEFAULT '',
`author_email` VARCHAR(255) NOT NULL,
PRIMARY KEY (`company_name`, `title`)");

    EnsureTableExists(connection, "specialties", @"`id` INT NOT NULL AUTO_INCREMENT,
`name` VARCHAR(255) NOT NULL,
PRIMARY KEY (`id`),
UNIQUE INDEX `uq_specialties_name` (`name`)");
    EnsureTableExists(connection, "vacancy_views", @"`id` INT NOT NULL AUTO_INCREMENT,
`user_email` VARCHAR(255) NOT NULL,
`vacancy_company_name` VARCHAR(255) NOT NULL,
`vacancy_title` VARCHAR(255) NOT NULL,
`viewed_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
PRIMARY KEY (`id`)");

    // Добавляем недостающие столбцы для других таблиц
    EnsureColumnExists(connection, "companies", "description", "TEXT NULL");
    EnsureColumnExists(connection, "companies", "website", "TEXT NULL");
    EnsureColumnExists(connection, "companies", "logo_url", "TEXT NULL");
    EnsureColumnExists(connection, "companies", "verified", "TINYINT(1) NOT NULL DEFAULT 0");

    EnsureColumnExists(connection, "vacancies", "region", "VARCHAR(100) NOT NULL DEFAULT ''");
    EnsureColumnExists(connection, "vacancies", "work_hours_per_day", "INT NULL");
    EnsureColumnExists(connection, "vacancies", "work_format", "VARCHAR(50) NOT NULL DEFAULT ''");
    // НОВАЯ КОЛОНКА ДЛЯ ПРАКТИКИ
    EnsureColumnExists(connection, "vacancies", "is_practicum", "TINYINT(1) NOT NULL DEFAULT 0");

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
    EnsureIndexExists(connection, "vacancy_views", "ux_vacancy_views_user_vacancy", "UNIQUE (`user_email`, `vacancy_company_name`, `vacancy_title`)");
}

static void EnsureTableExists(MySqlConnection connection, string tableName, string tableDefinition)
{
    using var command = connection.CreateCommand();
    command.CommandText = $"CREATE TABLE IF NOT EXISTS `{tableName}` ({tableDefinition}) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;";
    command.ExecuteNonQuery();
}

static void EnsureIndexExists(MySqlConnection connection, string tableName, string indexName, string indexDefinition)
{
    using var existsCommand = connection.CreateCommand();
    existsCommand.CommandText = @"SELECT COUNT(*) FROM information_schema.statistics WHERE table_schema = DATABASE() AND table_name = @tableName AND index_name = @indexName;";
    existsCommand.Parameters.AddWithValue("@tableName", tableName);
    existsCommand.Parameters.AddWithValue("@indexName", indexName);
    var exists = Convert.ToInt32(existsCommand.ExecuteScalar()) > 0;

    if (!exists)
    {
        using var addCommand = connection.CreateCommand();
        addCommand.CommandText = $"ALTER TABLE `{tableName}` ADD CONSTRAINT `{indexName}` {indexDefinition};";
        addCommand.ExecuteNonQuery();
    }
}

static void EnsureColumnExists(MySqlConnection connection, string tableName, string columnName, string columnDefinition)
{
    using var existsCommand = connection.CreateCommand();
    existsCommand.CommandText = @"SELECT COUNT(*) FROM information_schema.columns WHERE table_schema = DATABASE() AND table_name = @tableName AND column_name = @columnName;";
    existsCommand.Parameters.AddWithValue("@tableName", tableName);
    existsCommand.Parameters.AddWithValue("@columnName", columnName);
    var exists = Convert.ToInt32(existsCommand.ExecuteScalar()) > 0;
    if (exists) return;

    using var alterCommand = connection.CreateCommand();
    alterCommand.CommandText = $"ALTER TABLE `{tableName}` ADD COLUMN `{columnName}` {columnDefinition};";
    alterCommand.ExecuteNonQuery();
}
