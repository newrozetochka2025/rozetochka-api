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
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<ProductImage> ProductImages { get; set; } = null!;

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


            // --- Category ---
            modelBuilder.Entity<Category>(c =>
            {
                c.ToTable("Categories");
                c.HasKey(x => x.Id);

                c.HasIndex(x => x.Slug).IsUnique();
                c.Property(x => x.Title).HasMaxLength(256).IsRequired();
                c.Property(x => x.Slug).HasMaxLength(256).IsRequired();
                c.Property(x => x.IconSvgUrl).HasMaxLength(1024);
                c.Property(x => x.ImageUrl).HasMaxLength(1024);

                // самоссылка
                c.HasOne(x => x.Parent)
                 .WithMany(x => x.Children)
                 .HasForeignKey(x => x.ParentId)
                 .OnDelete(DeleteBehavior.Restrict);    // не даст удалить родителя, если у него есть child, чтобы не рушить дерево
                
                c.HasIndex(x => x.ParentId);    // FK indx
            });


            // --- Product  ---
            modelBuilder.Entity<Product>(p =>
            {
                p.ToTable("Products");
                p.HasKey(x => x.Id);

                p.HasIndex(x => x.Slug).IsUnique();
                p.Property(x => x.Slug).HasMaxLength(256).IsRequired();
                p.Property(x => x.Title).HasMaxLength(256).IsRequired();
                p.Property(x => x.ImgUrl).HasMaxLength(1024);

                p.Property(x => x.Price).HasPrecision(18, 2);
                p.Property(x => x.DiscountPrice).HasPrecision(18, 2);
                p.Property(x => x.CreatedAt).HasColumnType("datetime2(0)");

                // Чек - констрейнты на уровне БД
                p.ToTable(t =>
                {
                    t.HasCheckConstraint("CK_Products_Price_Positive", "[Price] >= 0");
                    t.HasCheckConstraint("CK_Products_Discount_NonNegative", "[DiscountPrice] IS NULL OR [DiscountPrice] >= 0");
                    t.HasCheckConstraint("CK_Products_Discount_Le_Price", "[DiscountPrice] IS NULL OR [DiscountPrice] <= [Price]");
                });

                // M2M: Products <-> Categories
                p.HasMany(prod => prod.Categories)
                    .WithMany(cat => cat.Products)
                    .UsingEntity<Dictionary<string, object>>(
                        "ProductCategories",            // имя таблицы связывания
                        j => j.HasOne<Category>()
                              .WithMany()
                              .HasForeignKey("CategoryId")
                              .OnDelete(DeleteBehavior.Cascade),
                        j => j.HasOne<Product>()
                              .WithMany()
                              .HasForeignKey("ProductId")
                              .OnDelete(DeleteBehavior.Cascade),
                        j =>
                        {
                            j.HasKey("ProductId", "CategoryId");    // составной PK
                            j.ToTable("ProductCategories");
                            j.HasIndex("CategoryId", "ProductId")
                             .HasDatabaseName("IX_ProductCategories_Category_Product");
                    });

                // Ускоряет выборку WHERE IsRecommended=1 ORDER BY CreatedAt
                p.HasIndex(x => new { x.IsRecommended, x.CreatedAt }).HasDatabaseName("IX_Products_Recommended");
                // Ускоряет выборку WHERE IsBest=1 ORDER BY CreatedAt
                p.HasIndex(x => new { x.IsBest, x.CreatedAt }).HasDatabaseName("IX_Products_Best");

            });



            // --- ProductImage  ---
            modelBuilder.Entity<ProductImage>(pi =>
            {
                pi.ToTable("ProductImages");
                pi.HasKey(x => x.Id);

                pi.Property(x => x.Url).HasMaxLength(1024).IsRequired();

                pi.HasOne(x => x.Product)
                    .WithMany(prod => prod.Images)
                    .HasForeignKey(x => x.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);

                pi.HasIndex(x => x.ProductId);
            });



        }
    }
}
