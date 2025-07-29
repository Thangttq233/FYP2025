using Microsoft.EntityFrameworkCore;
using FYP2025.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.Linq; // Cần thiết cho SelectMany nếu chưa có
using FYP2025.Application.Common; // <--- THÊM USING NÀY CHO RolesEnum

namespace FYP2025.Infrastructure.Data
{
    // Thay đổi kế thừa từ DbContext sang IdentityDbContext
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string> // string là kiểu dữ liệu của Id (Guid.ToString())
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany() 
                .HasForeignKey(p => p.CategoryId)
                .IsRequired(); 

            modelBuilder.Entity<ProductVariant>()
                .HasOne(pv => pv.Product)
                .WithMany(p => p.Variants) 
                .HasForeignKey(pv => pv.ProductId)
                .IsRequired(); 

            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict; // Hoặc .NoAction
            }

            string adminRoleId = "a8204620-8025-4523-a18d-68e1c6b1a37c"; 
            string customerRoleId = "c25c30f4-5f53-43a9-a9a7-8028120b6088"; 
            string salerRoleId = "s9d4e5f6-7890-1234-a789-b876c543d210"; 

            modelBuilder.Entity<ApplicationRole>().HasData(
                new ApplicationRole { Id = adminRoleId, Name = RolesEnum.Admin.ToString(), NormalizedName = RolesEnum.Admin.ToString().ToUpper() },
                new ApplicationRole { Id = customerRoleId, Name = RolesEnum.Customer.ToString(), NormalizedName = RolesEnum.Customer.ToString().ToUpper() },
                new ApplicationRole { Id = salerRoleId, Name = RolesEnum.Saler.ToString(), NormalizedName = RolesEnum.Saler.ToString().ToUpper() }
            );
        }
    }
}