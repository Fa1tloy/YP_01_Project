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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<User>().ToTable("users");
            modelBuilder.Entity<User>().ToTable("users", t => t.ExcludeFromMigrations());

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Email);
                entity.Property(e => e.Email)
                    .HasColumnName("email") // Добавьте имя столбца
                    .HasMaxLength(255);
                entity.Property(e => e.PasswordHash)
                    .HasColumnName("password_hash") // Добавьте имя столбца
                    .HasMaxLength(255)
                    .IsRequired();
                entity.Property(e => e.FirstName)
                    .HasColumnName("first_name") // Добавьте имя столбца
                    .HasMaxLength(100)
                    .IsRequired();
                entity.Property(e => e.LastName)
                    .HasColumnName("last_name") // Добавьте имя столбца
                    .HasMaxLength(100)
                    .IsRequired();
                entity.Property(e => e.Role)
                    .HasColumnName("role") // Добавьте имя столбца
                    .HasMaxLength(20)
                    .IsRequired();
                // Убрал HasConversion<string>() так как теперь Role - string
            });
        }
    }
}