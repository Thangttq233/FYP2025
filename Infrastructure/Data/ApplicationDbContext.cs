using Microsoft.EntityFrameworkCore;
using FYP2025.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.Linq; 
using FYP2025.Application.Common; 

namespace FYP2025.Infrastructure.Data
{

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string> 
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }


        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);



            modelBuilder.Entity<Category>()
                .Property(c => c.MainCategory)
                .HasConversion<string>() // enum → string
                .IsRequired();

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .IsRequired();

            modelBuilder.Entity<ProductVariant>()
                .HasOne(pv => pv.Product)
                .WithMany(p => p.Variants)
                .HasForeignKey(pv => pv.ProductId)
                .IsRequired();

            modelBuilder.Entity<Cart>()
                .HasMany(c => c.Items)
                .WithOne(ci => ci.Cart)
                .HasForeignKey(ci => ci.CartId)
                .IsRequired();

            modelBuilder.Entity<Cart>()
                .HasIndex(c => c.UserId)
                .IsUnique();

            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.ProductVariant)
                .WithMany()
                .HasForeignKey(ci => ci.ProductVariantId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict); // Ngăn chặn cascade delete

            modelBuilder.Entity<Order>()
                .HasOne(o => o.User) // Specify the navigation property
                .WithMany()
                .HasForeignKey(o => o.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.ProductVariant)
                .WithMany()
                .HasForeignKey(oi => oi.ProductVariantId)
                .IsRequired() 
                .OnDelete(DeleteBehavior.Restrict); // Ngăn chặn cascade delete

            modelBuilder.Entity<Order>()
                .Property(o => o.Status)
                .HasConversion<string>();

            modelBuilder.Entity<RefreshToken>()
                .HasOne(rt => rt.User)
                .WithMany()
                .HasForeignKey(rt => rt.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);


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