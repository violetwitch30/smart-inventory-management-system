using Microsoft.AspNetCore.Identity;
using SmartInventoryManagementSystem.Areas.ProductManagement.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SmartInventoryManagementSystem.Areas.ProjectManagement.Models;

namespace SmartInventoryManagementSystem.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Order> Orders { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("Identity");
        
        modelBuilder.Entity<ApplicationUser>(entity => { entity.ToTable("User"); });
        modelBuilder.Entity<IdentityRole>(entity => { entity.ToTable("Role"); });
        modelBuilder.Entity<IdentityUserRole<string>>(entity => { entity.ToTable("UserRoles"); });
        modelBuilder.Entity<IdentityUserClaim<string>>(entity => { entity.ToTable("UserClaims"); });
        modelBuilder.Entity<IdentityUserLogin<string>>(entity => { entity.ToTable("UserLogins"); });
        modelBuilder.Entity<IdentityRoleClaim<string>>(entity => { entity.ToTable("RoleClaims"); });
        modelBuilder.Entity<IdentityUserToken<string>>(entity => { entity.ToTable("UserTokens"); });
        
        // seed Categories table with example data
        modelBuilder.Entity<Category>().HasData(
            new Category { CategoryId = 1, Name = "Electronics" },
            new Category { CategoryId = 2, Name = "Home" },
            new Category { CategoryId = 3, Name = "Clothing" },
            new Category { CategoryId = 4, Name = "Food" },
            new Category { CategoryId = 5, Name = "Books" }
        );
        // seed Products table with example data
        modelBuilder.Entity<Product>().HasData(
            // Books
            new Product
            {
                ProductId = 1, Name = "Harry Potter", Description = "Harry Potter books for fantasy lovers",
                CategoryId = 5, Price = 50, Quantity = 100
            },
            new Product
            {
                ProductId = 2, Name = "Hunger Games", Description = "Hunger Games for dystopia lovers", CategoryId = 5,
                Price = 40, Quantity = 50
            },
            new Product
            {
                ProductId = 3, Name = "The Lord of the Rings", Description = "Classic fantasy novel", CategoryId = 5,
                Price = 60, Quantity = 30
            },

            // Electronics
            new Product
            {
                ProductId = 4, Name = "Laptop", Description = "New fast laptop for work and gaming", CategoryId = 1,
                Price = 1500, Quantity = 30
            },
            new Product
            {
                ProductId = 5, Name = "Smartphone", Description = "Latest smartphone with great camera", CategoryId = 1,
                Price = 999, Quantity = 20
            },
            new Product
            {
                ProductId = 6, Name = "Wireless Earbuds", Description = "Noise-canceling wireless earbuds",
                CategoryId = 1, Price = 150, Quantity = 50
            },
            new Product
            {
                ProductId = 7, Name = "Smartwatch", Description = "Feature-packed smartwatch", CategoryId = 1,
                Price = 250, Quantity = 3
            },

            // Home Products
            new Product
            {
                ProductId = 8, Name = "Vacuum Cleaner", Description = "Powerful vacuum cleaner for home use",
                CategoryId = 2, Price = 200, Quantity = 15
            },
            new Product
            {
                ProductId = 9, Name = "Coffee Maker", Description = "Automatic coffee maker", CategoryId = 2,
                Price = 80, Quantity = 40
            },

            // Clothing
            new Product
            {
                ProductId = 10, Name = "Summer Dress", Description = "Beautiful summer dress", CategoryId = 3,
                Price = 25, Quantity = 5
            },
            new Product
            {
                ProductId = 11, Name = "Leather Jacket", Description = "Stylish leather jacket", CategoryId = 3,
                Price = 150, Quantity = 10
            },
            new Product
            {
                ProductId = 12, Name = "Running Shoes", Description = "Comfortable running shoes", CategoryId = 3,
                Price = 120, Quantity = 20
            },
            new Product
            {
                ProductId = 13, Name = "Jeans", Description = "High-quality denim jeans", CategoryId = 3, Price = 60,
                Quantity = 30
            },

            // Food Items
            new Product
            {
                ProductId = 14, Name = "Chocolate Bar", Description = "Delicious milk chocolate", CategoryId = 4,
                Price = 2.5f, Quantity = 100
            },
            new Product
            {
                ProductId = 15, Name = "Organic Honey", Description = "Pure organic honey", CategoryId = 4, Price = 15,
                Quantity = 9
            },
            new Product
            {
                ProductId = 16, Name = "Pasta Pack", Description = "Italian pasta", CategoryId = 4, Price = 5,
                Quantity = 75
            }
        );
        
        // seed Orders with some example data
        modelBuilder.Entity<Order>().HasData(
            new Order
            {
                OrderId = 1,
                OrderDate = new DateTime(2025, 03, 31, 0, 0, 0, DateTimeKind.Utc), // Fixed date
                CustomerName = "John Doe",
                CustomerEmail = "john@gmail.com",
                ProductIds = new int[] { 1, 4, 5 }, // Consider changing List<int> to int[]
                Quantities = new int[] { 2, 1, 1 }
            },
            new Order
            {
                OrderId = 2,
                OrderDate = new DateTime(2025, 03, 30, 0, 0, 0, DateTimeKind.Utc), // Fixed date
                CustomerName = "Jane Smith",
                CustomerEmail = "jane@gmail.com",
                ProductIds = new int[] { 2, 10 }, // Consider changing List<int> to int[]
                Quantities = new int[] { 1, 1 }
            }
        );
    }
}