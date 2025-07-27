using FYP2025.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FYP2025.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; } // THÊM DÒNG NÀY

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Category>().HasKey(c => c.Id);
            modelBuilder.Entity<Product>().HasKey(p => p.Id);
            modelBuilder.Entity<ProductVariant>().HasKey(pv => pv.Id); // THÊM DÒNG NÀY

            // Cấu hình quan hệ 1-nhiều giữa Category và Product
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId);

            // Cấu hình quan hệ 1-nhiều giữa Product và ProductVariant
            modelBuilder.Entity<ProductVariant>() // ProductVariant có một Product
                .HasOne(pv => pv.Product)
                .WithMany(p => p.Variants) // Product có nhiều ProductVariants
                .HasForeignKey(pv => pv.ProductId)
                .OnDelete(DeleteBehavior.Cascade); // Khi xóa Product, xóa luôn các Variants của nó
        }
    }
}