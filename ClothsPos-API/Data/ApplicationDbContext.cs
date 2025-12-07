using Microsoft.EntityFrameworkCore;
using ClothsPosAPI.Models;
using BCrypt.Net;

namespace ClothsPosAPI.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Item> Items { get; set; }
    public DbSet<Sale> Sales { get; set; }
    public DbSet<SaleItem> SaleItems { get; set; }
    public DbSet<PaymentDue> PaymentDues { get; set; }
    public DbSet<ShopSettings> ShopSettings { get; set; } = null!;
    public DbSet<ReceiptTemplate> ReceiptTemplates { get; set; } = null!;
    public DbSet<EmailTemplate> EmailTemplates { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Role configuration
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasIndex(e => e.Name).IsUnique();
        });
        
        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.Username).IsUnique();
            
            // Foreign key relationship with Role
            entity.HasOne(e => e.Role)
                  .WithMany(r => r.Users)
                  .HasForeignKey(e => e.RoleId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Category configuration
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasIndex(e => e.Name).IsUnique();
        });

        // Item configuration
        modelBuilder.Entity<Item>(entity =>
        {
            entity.HasIndex(e => e.SKU).IsUnique();
            entity.HasIndex(e => e.Code);
            entity.HasOne(e => e.Category)
                  .WithMany()
                  .HasForeignKey(e => e.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Sale configuration
        modelBuilder.Entity<Sale>(entity =>
        {
            entity.HasOne(e => e.Cashier)
                  .WithMany()
                  .HasForeignKey(e => e.CashierId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // SaleItem configuration
        modelBuilder.Entity<SaleItem>(entity =>
        {
            entity.HasOne(e => e.Sale)
                  .WithMany(s => s.SaleItems)
                  .HasForeignKey(e => e.SaleId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.Item)
                  .WithMany()
                  .HasForeignKey(e => e.ItemId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // PaymentDue configuration
        modelBuilder.Entity<PaymentDue>(entity =>
        {
            entity.HasOne(e => e.Sale)
                  .WithMany()
                  .HasForeignKey(e => e.SaleId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ReceiptTemplate configuration
        modelBuilder.Entity<ReceiptTemplate>(entity =>
        {
            entity.HasIndex(e => e.Name);
        });

        // EmailTemplate configuration
        modelBuilder.Entity<EmailTemplate>(entity =>
        {
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.Type);
        });

        // Seed initial data
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // Note: Admin user is seeded at runtime via DatabaseSeeder
        // This ensures the password is properly hashed and avoids issues with seed data
        
        // Seed Categories
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = "1", Name = "Men's Clothing", Description = "All men's apparel", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Category { Id = "2", Name = "Women's Clothing", Description = "All women's apparel", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Category { Id = "3", Name = "Accessories", Description = "Bags, belts, watches", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Category { Id = "4", Name = "Shoes", Description = "Footwear for all", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        );

        // Seed Shop Settings
        // Configure ShopSettings to ignore CreatedAt/UpdatedAt if columns don't exist
        modelBuilder.Entity<ShopSettings>(entity =>
        {
            // Ignore CreatedAt and UpdatedAt columns if they don't exist in database
            entity.Ignore(e => e.CreatedAt);
            entity.Ignore(e => e.UpdatedAt);
        });

        modelBuilder.Entity<ShopSettings>().HasData(
            new ShopSettings
            {
                Id = "1",
                ShopName = "ClothPos Shop",
                Address = "123 Main Street",
                City = "Colombo",
                State = "Western Province",
                ZipCode = "00100",
                Country = "Sri Lanka",
                Phone = "+94 11 1234567",
                Email = "info@clothspos.com",
                TaxRate = 0,
                RoundingRule = "none",
                ReceiptTemplate = "default",
                CurrencySymbol = "Rs.",
                CurrencyCode = "LKR",
                CurrencyPosition = "before",
                POSDevices = "[]"
            }
        );
    }
}

