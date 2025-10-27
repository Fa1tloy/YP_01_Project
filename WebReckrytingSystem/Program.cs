// Program.cs
using Microsoft.EntityFrameworkCore;
using WebReckrytingSystem.Data;
using WebReckrytingSystem.Models;
using WebReckrytingSystem.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Добавление сервисов в контейнер
builder.Services.AddRazorPages();

// Добавляем аутентификацию
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/AccessDenied";
    });

// Регистрация контекста базы данных
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 21))
    ));

// Регистрация репозиториев и сервисов
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<UserService>();

// ДОБАВЬТЕ ЭТИ СТРОЧКИ:
builder.Services.AddScoped<IResumeRepository, ResumeRepository>();
builder.Services.AddScoped<IResumeService, ResumeService>();

var app = builder.Build();

// Конфигурация HTTP pipeline
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

app.Run();