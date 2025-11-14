// Models/ApplicationDbContext.cs
using Microsoft.EntityFrameworkCore;
using WebReckrytingSystem.Models;

namespace WebReckrytingSystem.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Resume> Resumes { get; set; }
        public DbSet<Vacancy> Vacancies { get; set; }
        public DbSet<Company> Companies { get; set; };


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("users", t => t.ExcludeFromMigrations());
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Email);
                entity.Property(e => e.Email)
                    .HasColumnName("email") // Добавлено имя столбца
                    .HasMaxLength(255);
                entity.Property(e => e.PasswordHash)
                    .HasColumnName("password_hash") // Добавлено имя столбца
                    .HasMaxLength(255)
                    .IsRequired();
                entity.Property(e => e.FirstName)
                    .HasColumnName("first_name") // Добавлено имя столбца
                    .HasMaxLength(100)
                    .IsRequired();
                entity.Property(e => e.LastName)
                    .HasColumnName("last_name") // Добавлено имя столбца
                    .HasMaxLength(100)
                    .IsRequired();
                entity.Property(e => e.Role)
                    .HasColumnName("role") // Добавлено имя столбца
                    .HasMaxLength(20)
                    .IsRequired();
            });

            // Конфигурация для Resume
            modelBuilder.Entity<Resume>().ToTable("resumes", t => t.ExcludeFromMigrations());
            modelBuilder.Entity<Resume>(entity =>
            {
                entity.HasKey(e => e.UserEmail);
                entity.Property(e => e.UserEmail)
                    .HasColumnName("user_email") // Добавлено имя столбца
                    .HasMaxLength(255);
                entity.Property(e => e.DesiredPosition)
                    .HasColumnName("desired_position") // Добавлено имя столбца
                    .HasMaxLength(255)
                    .IsRequired();
                entity.Property(e => e.ExperienceDescription)
                    .HasColumnName("experience_description"); // Добавлено имя столбца
                entity.Property(e => e.EducationDescription)
                    .HasColumnName("education_description"); // Добавлено имя столбца
                entity.Property(e => e.Skills)
                    .HasColumnName("skills"); // Добавлено имя столбца
                entity.Property(e => e.SalaryExpectations)
                    .HasColumnName("salary_expectations"); // Добавлено имя столбца
                entity.Property(e => e.IsPublished)
                    .HasColumnName("is_published") // Добавлено имя столбца
                    .HasDefaultValue(false);

                // Связь с User
                entity.HasOne(r => r.User)
                      .WithOne()
                      .HasForeignKey<Resume>(r => r.UserEmail)
                      .OnDelete(DeleteBehavior.Cascade);
            });
            // Конфигурация для Company
            modelBuilder.Entity<Company>().ToTable("companies", t => t.ExcludeFromMigrations());
            modelBuilder.Entity<Company>(entity =>
            {
                entity.HasKey(e => e.Name);
                entity.Property(e => e.Name)
                    .HasColumnName("name")
                    .HasMaxLength(255);
                entity.Property(e => e.Description)
                    .HasColumnName("description");
                entity.Property(e => e.Website)
                    .HasColumnName("website");
                entity.Property(e => e.LogoUrl)
                    .HasColumnName("logo_url");
                entity.Property(e => e.Verified)
                    .HasColumnName("verified")
                    .HasDefaultValue(false);
            });

            // Конфигурация для Vacancy
            modelBuilder.Entity<Vacancy>().ToTable("vacancies", t => t.ExcludeFromMigrations());
            modelBuilder.Entity<Vacancy>(entity =>
            {
                // Составной первичный ключ
                entity.HasKey(v => new { v.CompanyName, v.Title });

                entity.Property(e => e.CompanyName)
                    .HasColumnName("company_name")
                    .HasMaxLength(255);
                entity.Property(e => e.Title)
                    .HasColumnName("title")
                    .HasMaxLength(255);
                entity.Property(e => e.Description)
                    .HasColumnName("description");
                entity.Property(e => e.Requirements)
                    .HasColumnName("requirements");
                entity.Property(e => e.SalaryFrom)
                    .HasColumnName("salary_from");
                entity.Property(e => e.SalaryTo)
                    .HasColumnName("salary_to");
                entity.Property(e => e.EmploymentType)
                    .HasColumnName("employment_type");
                entity.Property(e => e.WorkSchedule)
                    .HasColumnName("work_schedule");
                entity.Property(e => e.AuthorEmail)
                    .HasColumnName("author_email")
                    .HasMaxLength(255);

                // Связи
                entity.HasOne(v => v.Company)
                      .WithMany(c => c.Vacancies)
                      .HasForeignKey(v => v.CompanyName)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(v => v.Author)
                      .WithMany()
                      .HasForeignKey(v => v.AuthorEmail)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}