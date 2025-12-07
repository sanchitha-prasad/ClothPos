using Microsoft.EntityFrameworkCore;
using ClothsPosAPI.Data;
using ClothsPosAPI.Models;
using BCrypt.Net;
using Microsoft.Extensions.Configuration;

namespace ClothsPosAPI.Services;

public class DatabaseSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public DatabaseSeeder(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    /// <summary>
    /// Seeds default roles if they don't exist
    /// </summary>
    public async Task SeedRolesAsync()
    {
        // Check if roles already exist
        if (await _context.Roles.AnyAsync())
        {
            return; // Roles already seeded
        }

        var defaultRoles = new[]
        {
            new Role
            {
                Id = "1",
                Name = "Admin",
                Description = "Full system access with all permissions",
                Permissions = "[\"all\"]",
                IsActive = true,
                IsSystemRole = true,
                DisplayOrder = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Role
            {
                Id = "2",
                Name = "Cashier",
                Description = "Can process sales and view inventory",
                Permissions = "[\"sales\", \"inventory_view\"]",
                IsActive = true,
                IsSystemRole = true,
                DisplayOrder = 2,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Role
            {
                Id = "3",
                Name = "Mobile Staff",
                Description = "Can process sales on mobile devices",
                Permissions = "[\"sales\"]",
                IsActive = true,
                IsSystemRole = true,
                DisplayOrder = 3,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        _context.Roles.AddRange(defaultRoles);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Ensures the admin user exists in the database
    /// This is called after database creation to seed the admin user
    /// The default credentials can be changed via environment variables or appsettings
    /// </summary>
    public async Task SeedAdminUserAsync()
    {
        // First ensure roles exist
        await SeedRolesAsync();

        // Get admin role
        var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
        if (adminRole == null)
        {
            throw new InvalidOperationException("Admin role not found. Please seed roles first.");
        }

        // Get default admin credentials from configuration (appsettings.json) or environment variables
        var adminUsername = Environment.GetEnvironmentVariable("ADMIN_USERNAME") 
            ?? _configuration["AdminUser:Username"] 
            ?? "admin-dev";
        var adminEmail = Environment.GetEnvironmentVariable("ADMIN_EMAIL") 
            ?? _configuration["AdminUser:Email"] 
            ?? "admin@shop.com";
        var adminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD") 
            ?? _configuration["AdminUser:Password"] 
            ?? "Admin123!";

        // Check if admin user already exists (by username or email)
        var existingAdmin = await _context.Users
            .FirstOrDefaultAsync(u => 
                (u.Username == adminUsername || u.Email == adminEmail) ||
                (u.RoleId == adminRole.Id && u.IsActive));

        if (existingAdmin == null)
        {
            // Create admin user with hashed password
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(adminPassword);
            
            var adminUser = new User
            {
                Id = Guid.NewGuid().ToString(),
                Username = adminUsername,
                Email = adminEmail,
                RoleId = adminRole.Id,
                Passcode = hashedPassword,
                Permissions = "[\"all\"]",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(adminUser);
            await _context.SaveChangesAsync();
        }
        else
        {
            // Update existing admin user to ensure it has the correct role and credentials
            if (existingAdmin.RoleId != adminRole.Id)
            {
                existingAdmin.RoleId = adminRole.Id;
            }
            
            // Update username/email if they don't match
            if (existingAdmin.Username != adminUsername)
            {
                existingAdmin.Username = adminUsername;
            }
            
            if (existingAdmin.Email != adminEmail)
            {
                existingAdmin.Email = adminEmail;
            }
            
            // Update password if it's not hashed (plain text)
            if (!existingAdmin.Passcode.StartsWith("$2a$") && 
                !existingAdmin.Passcode.StartsWith("$2b$") && 
                !existingAdmin.Passcode.StartsWith("$2y$"))
            {
                existingAdmin.Passcode = BCrypt.Net.BCrypt.HashPassword(adminPassword);
            }
            
            existingAdmin.IsActive = true;
            existingAdmin.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
        }
    }
}

