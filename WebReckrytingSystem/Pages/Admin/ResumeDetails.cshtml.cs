using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using WebReckrytingSystem.Models;

namespace WebReckrytingSystem.Pages.Admin;

[Authorize(Roles = "admin")]
public class ResumeDetailsModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public ResumeDetailsModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public Models.Resume? Resume { get; private set; }

    public async Task<IActionResult> OnGetAsync(string email)
    {
        Resume = await _context.Resumes
            .AsNoTracking()
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.UserEmail == email);

        if (Resume == null)
            return NotFound();

        return Page();
    }

    public async Task<IActionResult> OnGetDownloadPdfAsync(string email)
    {
        var resume = await _context.Resumes
            .AsNoTracking()
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.UserEmail == email);

        if (resume == null)
            return NotFound();

        var pdfBytes = GeneratePdf(resume);
        var fileName = $"resume_{resume.UserEmail?.Replace("@", "_")}.pdf";
        return File(pdfBytes, "application/pdf", fileName);
    }

    private byte[] GeneratePdf(Models.Resume resume)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontFamily("Segoe UI").FontSize(11));

                // Заголовок
                page.Header().Element(ComposeHeader(resume));

                // Основное содержимое
                page.Content().Element(ComposeContent(resume));

                // Футер
                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Сгенерировано системой «Твоя Карьера»");
                });
            });
        }).GeneratePdf();
    }

    Action<IContainer> ComposeHeader(Models.Resume resume) => header =>
    {
        header.Column(column =>
        {
            column.Item().Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text($"{resume.User?.LastName} {resume.User?.FirstName}")
                        .FontSize(22).Bold();
                    col.Item().Text($"Желаемая должность: {resume.DesiredPosition}")
                        .FontSize(14).FontColor(Colors.Grey.Darken1);
                });
                // Аватар убран совсем
            });

            column.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
        });
    };

    Action<IContainer> ComposeContent(Models.Resume resume) => content =>
    {
        // Подготовка данных с исправлениями
        var tripReadiness = resume.BusinessTripReadiness switch
        {
            "yes" => "Да",
            "no" => "Нет",
            "sometimes" => "Иногда",
            _ => resume.BusinessTripReadiness
        };

        // Убираем дублирование практик: вырезаем из опыта работы строки после "Практики:" (включительно)
        var experienceText = resume.ExperienceDescription;
        if (!string.IsNullOrWhiteSpace(experienceText))
        {
            var practiceIndex = experienceText.IndexOf("Практики:", StringComparison.OrdinalIgnoreCase);
            if (practiceIndex >= 0)
                experienceText = experienceText.Substring(0, practiceIndex).TrimEnd();
        }

        // Десериализуем практики
        List<PracticeViewModel> practices = new();
        if (!string.IsNullOrWhiteSpace(resume.PracticesJson))
        {
            try
            {
                practices = System.Text.Json.JsonSerializer.Deserialize<List<PracticeViewModel>>(resume.PracticesJson)
                            ?? new List<PracticeViewModel>();
            }
            catch { /* оставляем пустым */ }
        }

        content.Column(column =>
        {
            // 1. Основная информация
            column.Item().Text("Основная информация").FontSize(14).Bold();
            column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(180);
                    columns.RelativeColumn();
                });

                void AddRow(string label, string value)
                {
                    table.Cell().Text(label).FontColor(Colors.Grey.Darken2);
                    table.Cell().Text(value);
                }

                AddRow("Email:", resume.UserEmail);
                AddRow("Город:", resume.City);
                AddRow("Возраст:", resume.Age?.ToString() ?? "");
                AddRow("Пол:", resume.Gender);
                AddRow("Специальность:", resume.Specialty);
                AddRow("Желаемая должность:", resume.DesiredPosition);
                AddRow("Желаемая зарплата:", resume.SalaryExpectations.HasValue
                    ? $"{resume.SalaryExpectations.Value:N0} ₽"
                    : "не указана");
                AddRow("Тип занятости:", resume.EmploymentType);
                AddRow("График:", resume.WorkSchedule);
                AddRow("Готовность к командировкам:", tripReadiness);
                AddRow("Наличие автомобиля:", resume.HasCar ? "Да" : "Нет");
                AddRow("Категория прав:", string.IsNullOrWhiteSpace(resume.DriverLicenseCategory) ? "не указана" : resume.DriverLicenseCategory);
                AddRow("Статус публикации:", resume.IsPublished ? "Опубликовано" : "Не опубликовано");
                AddRow("Статус поиска работы:", resume.SearchStatus);
            });

            // 2. Опыт работы
            if (!string.IsNullOrWhiteSpace(experienceText))
            {
                column.Item().PaddingTop(15).Text("Опыт работы").FontSize(14).Bold();
                column.Item().Text(experienceText);
            }

            // 3. Образование
            if (!string.IsNullOrWhiteSpace(resume.EducationDescription))
            {
                column.Item().PaddingTop(15).Text("Образование").FontSize(14).Bold();
                column.Item().Text(resume.EducationDescription);
            }

            // 4. Навыки
            if (!string.IsNullOrWhiteSpace(resume.Skills))
            {
                column.Item().PaddingTop(15).Text("Навыки").FontSize(14).Bold();
                var skills = resume.Skills.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var skill in skills)
                {
                    column.Item().Text($"• {skill.Trim()}");
                }
            }

            // 5. Практики (отдельно)
            if (practices.Any())
            {
                column.Item().PaddingTop(15).Text("Практики").FontSize(14).Bold();
                foreach (var p in practices)
                {
                    column.Item().Text($"{p.Place} ({p.StartDate:MM.yyyy} – {p.EndDate:MM.yyyy})").Bold();
                    if (!string.IsNullOrWhiteSpace(p.Description))
                        column.Item().Text(p.Description);
                }
            }
        });
    };
}