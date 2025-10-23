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
            modelBuilder.Entity<User>().ToTable("users");

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Email);
                entity.Property(e => e.Email)
                    .HasMaxLength(255);
                entity.Property(e => e.PasswordHash)
                    .HasMaxLength(255)
                    .IsRequired();
                entity.Property(e => e.FirstName)
                    .HasMaxLength(100)
                    .IsRequired();
                entity.Property(e => e.LastName)
                    .HasMaxLength(100)
                    .IsRequired();
                entity.Property(e => e.Role)
                    .HasMaxLength(20)
                    .IsRequired();
                // Убрали .HasConversion<string>() так как теперь Role - string
            });
        }
    }
}