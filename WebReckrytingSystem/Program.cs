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
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
        options.SlidingExpiration = true;
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

//  регистрацию сервиса компаний
builder.Services.AddScoped<ICompanyService, CompanyService>();

// ДОБАВЬТЕ ЭТИ СТРОЧКИ:
builder.Services.AddScoped<IResumeRepository, ResumeRepository>();
builder.Services.AddScoped<IResumeService, ResumeService>();

// Регистрация репозиториев и сервисов для вакансий
builder.Services.AddScoped<IVacancyRepository, VacancyRepository>();
builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();
builder.Services.AddScoped<IVacancyService, VacancyService>();

// Регистрация сервиса поиска вакансий
builder.Services.AddScoped<IVacancySearchService, VacancySearchService>();


builder.WebHost.UseUrls("http://0.0.0.0:5024");

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
 // добавляем свой   
app.Run();