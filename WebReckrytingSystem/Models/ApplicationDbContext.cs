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
        }
    }
}