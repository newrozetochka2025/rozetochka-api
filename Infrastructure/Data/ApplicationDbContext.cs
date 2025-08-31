using Microsoft.EntityFrameworkCore;
using rozetochka_api.Domain.Entities;

namespace rozetochka_api.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<UserRefreshToken> UserRefreshTokens { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
       
            // --- User ---
            modelBuilder.Entity<User>(b =>
            {
                // Уникальные индексы + имена индексов
                b.HasIndex(u => u.Email)
                 .IsUnique()
                 .HasDatabaseName("IX_Users_Email");

                b.HasIndex(u => u.Username)
                 .IsUnique()
                 .HasDatabaseName("IX_Users_Username");

                // Свойства
                b.Property(u => u.Email)
                 .IsRequired()
                 .HasMaxLength(100);

                b.Property(u => u.Username)
                 .IsRequired()
                 .HasMaxLength(50);

                b.Property(u => u.Phone)
                 .IsRequired()
                 .HasMaxLength(15);

                b.Property(u => u.CreatedAt)
                 .IsRequired()
                 .HasColumnType("datetime2(0)");  // 0 = обрезает миллисекунды
            });

            // --- UserRefreshToken ---
            modelBuilder.Entity<UserRefreshToken>(b =>
            {
                b.ToTable("UserRefreshTokens");
                b.HasKey(x => x.Id);

                // User -> RefreshTokens
                b.HasOne(x => x.User)
                 .WithMany(u => u.RefreshTokens)
                 .HasForeignKey(x => x.UserId)
                 .OnDelete(DeleteBehavior.Cascade);

                b.Property(x => x.Token)
                 .IsRequired()
                 .HasMaxLength(256);

                b.Property(x => x.ExpiresAt)
                 .IsRequired();

                b.Property(x => x.IsRevoked)
                 .HasDefaultValue(false);

                // Индексы
                b.HasIndex(x => x.Token).IsUnique();
                b.HasIndex(x => new { x.UserId, x.IsRevoked });
            });

        }
    }
}
