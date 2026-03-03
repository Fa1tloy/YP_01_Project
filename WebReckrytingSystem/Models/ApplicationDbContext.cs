using Microsoft.EntityFrameworkCore;
using WebReckrytingSystem.Models;

namespace WebReckrytingSystem.Models
{
    public class ApplicationDbContext : DbContext
    {
        // Конструктор для миграций (БЕЗ параметров)
        public ApplicationDbContext() : base()
        {
        }

        // Конструктор для работы приложения (С параметрами)
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // ВСЕ таблицы
        public DbSet<User> Users { get; set; }
        public DbSet<Resume> Resumes { get; set; }
        public DbSet<Vacancy> Vacancies { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<ResumeView> ResumeViews { get; set; }
        public DbSet<JobApplication> JobApplications { get; set; }
        public DbSet<SavedVacancy> SavedVacancies { get; set; }
        public DbSet<DailyAnalytic> DailyAnalytics { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<SavedResume> SavedResumes { get; set; }

        // Для миграций — строка подключения, если опции не переданы
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseMySql(
                    "Server=localhost;Database=rekryting_system;Uid=root;Pwd=vertrigo;",
                    new MySqlServerVersion(new Version(8, 0, 21))
                );
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Конфигурация User
            modelBuilder.Entity<User>().ToTable("users", t => t.ExcludeFromMigrations());
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Email);
                entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(255);
                entity.Property(e => e.PasswordHash).HasColumnName("password_hash").HasMaxLength(255).IsRequired();
                entity.Property(e => e.FirstName).HasColumnName("first_name").HasMaxLength(100).IsRequired();
                entity.Property(e => e.LastName).HasColumnName("last_name").HasMaxLength(100).IsRequired();
                entity.Property(e => e.Role).HasColumnName("role").HasMaxLength(20).IsRequired();

                // === НОВОЕ: СВЯЗЬ С КОМПАНИЕЙ ===
                entity.Property(e => e.CompanyName).HasColumnName("company_name").HasMaxLength(255).IsRequired(false);

                entity.HasOne(u => u.Company)
                    .WithMany()
                    .HasForeignKey(u => u.CompanyName)
                    .HasPrincipalKey(c => c.Name)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Конфигурация Resume
            modelBuilder.Entity<Resume>().ToTable("resumes", t => t.ExcludeFromMigrations());
            modelBuilder.Entity<Resume>(entity =>
            {
                entity.HasKey(e => e.UserEmail);
                entity.Property(e => e.UserEmail).HasColumnName("user_email").HasMaxLength(255);
                entity.Property(e => e.DesiredPosition).HasColumnName("desired_position").HasMaxLength(255).IsRequired();
                entity.Property(e => e.City).HasColumnName("city").HasMaxLength(100).IsRequired();
                entity.Property(e => e.BusinessTripReadiness).HasColumnName("business_trip_readiness").HasMaxLength(20).IsRequired();
                entity.Property(e => e.SearchStatus).HasColumnName("search_status").HasMaxLength(50).IsRequired();
                entity.Property(e => e.Age).HasColumnName("age");
                entity.Property(e => e.EmploymentType).HasColumnName("employment_type").HasMaxLength(50).IsRequired();
                entity.Property(e => e.WorkSchedule).HasColumnName("work_schedule").HasMaxLength(50).IsRequired();
                entity.Property(e => e.Specialty).HasColumnName("specialty").HasMaxLength(255).IsRequired();
                entity.Property(e => e.Gender).HasColumnName("gender").HasMaxLength(20).IsRequired();
                entity.Property(e => e.HasCar).HasColumnName("has_car").HasDefaultValue(false);
                entity.Property(e => e.DriverLicenseCategory).HasColumnName("driver_license_category").HasMaxLength(20).IsRequired(false);
                entity.Property(e => e.ExperienceDescription).HasColumnName("experience_description");
                entity.Property(e => e.EducationDescription).HasColumnName("education_description");
                entity.Property(e => e.Skills).HasColumnName("skills");
                entity.Property(e => e.SalaryExpectations).HasColumnName("salary_expectations");
                entity.Property(e => e.IsPublished).HasColumnName("is_published").HasDefaultValue(false);
                entity.Property(e => e.PracticesJson).HasColumnName("practices_json").HasColumnType("text");

                entity.HasOne(r => r.User)
                    .WithOne() // User не имеет коллекции Resume
                    .HasForeignKey<Resume>(r => r.UserEmail) // UserEmail является FK
                    .HasPrincipalKey<User>(u => u.Email) // Связан с PK User.Email
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Конфигурация Company
            modelBuilder.Entity<Company>().ToTable("companies", t => t.ExcludeFromMigrations());
            modelBuilder.Entity<Company>(entity =>
            {
                entity.HasKey(e => e.Name);
                entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(255);
                entity.Property(e => e.Description).HasColumnName("description");
                entity.Property(e => e.Website).HasColumnName("website");
                entity.Property(e => e.LogoUrl).HasColumnName("logo_url");
                entity.Property(e => e.Verified).HasColumnName("verified").HasDefaultValue(false);
            });

            // Конфигурация Vacancy
            modelBuilder.Entity<Vacancy>().ToTable("vacancies", t => t.ExcludeFromMigrations());
            modelBuilder.Entity<Vacancy>(entity =>
            {
                entity.HasKey(v => new { v.CompanyName, v.Title });
                entity.Property(e => e.CompanyName).HasColumnName("company_name").HasMaxLength(255);
                entity.Property(e => e.Region).HasColumnName("region").HasMaxLength(100);
                entity.Property(e => e.Title).HasColumnName("title").HasMaxLength(255);
                entity.Property(e => e.Description).HasColumnName("description");
                entity.Property(e => e.Requirements).HasColumnName("requirements");
                entity.Property(e => e.SalaryFrom).HasColumnName("salary_from");
                entity.Property(e => e.SalaryTo).HasColumnName("salary_to");
                entity.Property(e => e.EmploymentType).HasColumnName("employment_type");
                entity.Property(e => e.WorkSchedule).HasColumnName("work_schedule");
                entity.Property(e => e.WorkHoursPerDay).HasColumnName("work_hours_per_day");
                entity.Property(e => e.WorkFormat).HasColumnName("work_format").HasMaxLength(50);
                entity.Property(e => e.SalaryPeriod).HasColumnName("salary_period").HasMaxLength(20);
                entity.Property(e => e.PaymentFrequency).HasColumnName("payment_frequency").HasMaxLength(50);
                entity.Property(e => e.Specialty).HasColumnName("specialty").HasMaxLength(255);
                entity.Property(e => e.AuthorEmail).HasColumnName("author_email").HasMaxLength(255);

                entity.HasOne(v => v.Company).WithMany(c => c.Vacancies).HasForeignKey(v => v.CompanyName).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(v => v.Author).WithMany().HasForeignKey(v => v.AuthorEmail).OnDelete(DeleteBehavior.Restrict);
            });

            // Конфигурация ResumeView
            modelBuilder.Entity<ResumeView>().ToTable("resume_views", t => t.ExcludeFromMigrations());
            modelBuilder.Entity<ResumeView>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ResumeEmail).HasColumnName("resume_email").HasMaxLength(255).IsRequired();
                entity.Property(e => e.ViewerEmail).HasColumnName("viewer_email").HasMaxLength(255).IsRequired();
                entity.Property(e => e.ViewedAt).HasColumnName("viewed_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.ViewedFromIp).HasColumnName("viewed_from_ip").HasMaxLength(45);
            });

            // Конфигурация JobApplication
            modelBuilder.Entity<JobApplication>().ToTable("job_applications", t => t.ExcludeFromMigrations());
            modelBuilder.Entity<JobApplication>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.StudentEmail).HasColumnName("student_email").HasMaxLength(255).IsRequired();
                entity.Property(e => e.VacancyCompanyName).HasColumnName("vacancy_company_name").HasMaxLength(255).IsRequired();
                entity.Property(e => e.VacancyTitle).HasColumnName("vacancy_title").HasMaxLength(255).IsRequired();
                entity.Property(e => e.CoverLetter).HasColumnName("cover_letter");
                entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20).HasDefaultValue("pending");
                entity.Property(e => e.AppliedAt).HasColumnName("applied_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // Конфигурация SavedVacancy
            modelBuilder.Entity<SavedVacancy>().ToTable("saved_vacancies", t => t.ExcludeFromMigrations());
            modelBuilder.Entity<SavedVacancy>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.StudentEmail).HasColumnName("student_email").HasMaxLength(255).IsRequired();
                entity.Property(e => e.VacancyCompanyName).HasColumnName("vacancy_company_name").HasMaxLength(255).IsRequired();
                entity.Property(e => e.VacancyTitle).HasColumnName("vacancy_title").HasMaxLength(255).IsRequired();
                entity.Property(e => e.SavedAt).HasColumnName("saved_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.HasIndex(e => new { e.StudentEmail, e.VacancyCompanyName, e.VacancyTitle }).IsUnique();
                entity.HasOne(s => s.Vacancy)
    .WithMany()
    .HasForeignKey(s => new { s.VacancyCompanyName, s.VacancyTitle })
    .HasPrincipalKey(v => new { v.CompanyName, v.Title });

            });
            modelBuilder.Entity<SavedResume>().ToTable("saved_resumes", t => t.ExcludeFromMigrations());
            modelBuilder.Entity<SavedResume>(entity =>
            {
                entity.ToTable("saved_resumes", t => t.ExcludeFromMigrations());
                entity.HasKey(e => e.Id);
                entity.Property(e => e.EmployerEmail).HasColumnName("employer_email").HasMaxLength(255).IsRequired();
                entity.Property(e => e.ResumeUserEmail).HasColumnName("resume_user_email").HasMaxLength(255).IsRequired();
                entity.Property(e => e.SavedAt).HasColumnName("saved_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.HasIndex(e => new { e.EmployerEmail, e.ResumeUserEmail }).IsUnique();
                entity.HasOne(s => s.Resume)
    .WithMany()
    .HasForeignKey(s => s.ResumeUserEmail)
    .HasPrincipalKey(r => r.UserEmail);
            });

            // Конфигурация DailyAnalytic
            modelBuilder.Entity<DailyAnalytic>().ToTable("daily_analytics", t => t.ExcludeFromMigrations());
            modelBuilder.Entity<DailyAnalytic>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserEmail).HasColumnName("user_email").HasMaxLength(255).IsRequired();
                entity.Property(e => e.Date).HasColumnName("date").IsRequired();
                entity.Property(e => e.ProfileViews).HasColumnName("profile_views").HasDefaultValue(0);
                entity.Property(e => e.ApplicationsSent).HasColumnName("applications_sent").HasDefaultValue(0);
                entity.Property(e => e.SavedVacancies).HasColumnName("saved_vacancies").HasDefaultValue(0);
                entity.HasIndex(e => new { e.UserEmail, e.Date }).IsUnique();
            });

            // Конфигурация Notification
            modelBuilder.Entity<Notification>().ToTable("notifications", t => t.ExcludeFromMigrations());
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.RecipientEmail).HasColumnName("recipient_email").HasMaxLength(255).IsRequired();
                entity.Property(e => e.SenderEmail).HasColumnName("sender_email").HasMaxLength(255).IsRequired();
                entity.Property(e => e.Title).HasColumnName("title").HasMaxLength(255).IsRequired();
                entity.Property(e => e.Message).HasColumnName("message").IsRequired();
                entity.Property(e => e.Link).HasColumnName("link").HasMaxLength(500);
                entity.Property(e => e.IsRead).HasColumnName("is_read").HasDefaultValue(false);
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // Конфигурация ChatMessage
            modelBuilder.Entity<ChatMessage>().ToTable("chat_messages", t => t.ExcludeFromMigrations());
            modelBuilder.Entity<ChatMessage>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.SenderEmail).HasColumnName("sender_email").HasMaxLength(255).IsRequired();
                entity.Property(e => e.RecipientEmail).HasColumnName("recipient_email").HasMaxLength(255).IsRequired();
                entity.Property(e => e.Message).HasColumnName("message").IsRequired();
                entity.Property(e => e.VacancyCompanyName).HasColumnName("vacancy_company_name").HasMaxLength(255);
                entity.Property(e => e.VacancyTitle).HasColumnName("vacancy_title").HasMaxLength(255);
                entity.Property(e => e.SentAt).HasColumnName("sent_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.IsRead).HasColumnName("is_read").HasDefaultValue(false);
            });
        }
    }
}
