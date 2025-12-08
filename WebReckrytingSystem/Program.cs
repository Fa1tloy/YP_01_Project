using Microsoft.EntityFrameworkCore;
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