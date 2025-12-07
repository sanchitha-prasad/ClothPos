using Microsoft.EntityFrameworkCore;
using ClothsPosAPI.Data;
using ClothsPosAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using Microsoft.Data.SqlClient;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using MySqlConnector;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Configure enum serialization to use string names instead of numbers
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        // Handle circular references by ignoring them
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ClothPos API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Database Configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? builder.Configuration.GetConnectionString("MySqlConnection")
    ?? "Data Source=localhost;Initial Catalog=ClothPosDB;Integrated Security=True;TrustServerCertificate=True;";

// Determine database provider from connection string or environment variable
var useMySql = builder.Configuration.GetValue<bool>("UseMySql", false) 
    || connectionString.Contains("Server=") && connectionString.Contains("Database=") && !connectionString.Contains("Data Source=");

if (useMySql)
{
    // MySQL Configuration (for PlanetScale, Aiven, etc.)
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseMySql(connectionString, 
            new MySqlServerVersion(new Version(8, 0, 21)),
            mySqlOptions => {
                mySqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
                mySqlOptions.SchemaBehavior(MySqlSchemaBehavior.Ignore);
            }));
}
else
{
    // SQL Server Configuration (default)
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(connectionString));
}

// CORS Configuration
builder.Services.AddCors(options =>
{
    var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() 
        ?? new[] { "http://localhost:3000", "http://localhost:5173" };
    
    options.AddPolicy("AllowReactApp", policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        }
        else
        {
            // Production: Allow specific origins or all (configure via environment variable)
            var frontendUrl = builder.Configuration["FrontendUrl"];
            if (!string.IsNullOrEmpty(frontendUrl))
            {
                policy.WithOrigins(frontendUrl)
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            }
            else
            {
                // Fallback: Allow all origins (less secure, but works for quick setup)
                policy.AllowAnyOrigin()
                      .AllowAnyHeader()
                      .AllowAnyMethod();
            }
        }
    });
});

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "YourSuperSecretKeyThatIsAtLeast32CharactersLong!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "ClothPosAPI";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "ClothPosClient";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

// Register Services
builder.Services.AddScoped<IItemService, ItemService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<ISaleService, SaleService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ISettingsService, SettingsService>();
builder.Services.AddScoped<IReceiptTemplateService, ReceiptTemplateService>();
builder.Services.AddScoped<IEmailTemplateService, EmailTemplateService>();
builder.Services.AddScoped<DatabaseSeeder>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // Enable static file serving for uploaded images
app.UseCors("AllowReactApp");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Initialize Database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    // Ensure database is created
    try
    {
        db.Database.EnsureCreated();
        logger.LogInformation("Database ensured/created successfully");
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Database creation issue");
    }
    
    // Check if Username column exists, if not, provide migration instructions
    try
    {
        // Try a simple query that uses Username to check if column exists
        // Use database-agnostic syntax (LIMIT works for both MySQL and SQL Server 2012+)
        var testQuery = await db.Database.ExecuteSqlRawAsync(
            useMySql 
                ? "SELECT Username FROM Users WHERE 1=0 LIMIT 1"
                : "SELECT TOP 1 Username FROM Users WHERE 1=0"
        );
    }
    catch (Microsoft.Data.SqlClient.SqlException sqlEx) when (sqlEx.Number == 207) // Invalid column name (SQL Server)
    {
        logger.LogError("==========================================");
        logger.LogError("DATABASE MIGRATION REQUIRED!");
        logger.LogError("==========================================");
        logger.LogError("The 'Username' column is missing from the Users table.");
        logger.LogError("");
        logger.LogError("SOLUTION: Run the migration script:");
        logger.LogError("  1. Open SQL Server Management Studio");
        logger.LogError("  2. Connect to your database");
        logger.LogError("  3. Open and execute: ClothsPos-API/Database/AddUsernameColumn.sql");
        logger.LogError("");
        logger.LogError("OR drop and recreate the database (DEVELOMPMENT ONLY - loses data):");
        logger.LogError("  DROP DATABASE ClothPosDB;");
        logger.LogError("  (Then restart the API)");
    }
    catch (MySqlConnector.MySqlException mySqlEx) when (mySqlEx.Message.Contains("Unknown column") || mySqlEx.Message.Contains("doesn't exist")) // Invalid column name (MySQL)
    {
        logger.LogError("==========================================");
        logger.LogError("DATABASE MIGRATION REQUIRED!");
        logger.LogError("==========================================");
        logger.LogError("The 'Username' column is missing from the Users table.");
        logger.LogError("");
        logger.LogError("SOLUTION: Run the migration script:");
        logger.LogError("  1. Connect to MySQL");
        logger.LogError("  2. Execute: ClothsPos-API/Database/AddUsernameColumn.sql");
        logger.LogError("");
        logger.LogError("OR drop and recreate the database (DEVELOMPMENT ONLY - loses data):");
        logger.LogError("  DROP DATABASE ClothPosDB;");
        logger.LogError("  (Then restart the API)");
        logger.LogError("==========================================");
        logger.LogError("");
        logger.LogError("Application will now exit. Please run the migration and restart.");
        Environment.Exit(1);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error checking database schema");
    }
    
    // Seed roles and admin user if not exists
    try
    {
        var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
        await seeder.SeedRolesAsync();
        logger.LogInformation("Roles seeded successfully");
        await seeder.SeedAdminUserAsync();
        logger.LogInformation("Admin user seeded successfully");
    }
    catch (Microsoft.Data.SqlClient.SqlException sqlEx) when (sqlEx.Number == 208) // Invalid object name
    {
        logger.LogError("==========================================");
        logger.LogError("DATABASE MIGRATION REQUIRED!");
        logger.LogError("==========================================");
        logger.LogError("The 'Roles' table is missing from the database.");
        logger.LogError("Please run the migration script: ClothsPos-API/Database/CreateRolesTable.sql");
        logger.LogError("==========================================");
        throw;
    }
    catch (Microsoft.Data.SqlClient.SqlException sqlEx) when (sqlEx.Number == 207) // Invalid column name
    {
        logger.LogError("==========================================");
        logger.LogError("DATABASE MIGRATION REQUIRED!");
        logger.LogError("==========================================");
        logger.LogError("Please run: ClothsPos-API/Database/AddUsernameColumn.sql");
        logger.LogError("==========================================");
        Environment.Exit(1);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error seeding admin user");
        throw;
    }
}

app.Run();

