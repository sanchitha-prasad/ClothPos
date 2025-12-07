# MySQL Setup Guide for ClothPos API

## Step 1: Update Project File

Add MySQL package to `ClothsPos-API.csproj`:

```xml
<PackageReference Include="Pomelo.EntityFrameworkCore.MySql" Version="8.0.0" />
```

Remove or comment out SQL Server package:
```xml
<!-- <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.0" /> -->
```

---

## Step 2: Update Program.cs

### Change Database Provider

Find this line in `Program.cs`:
```csharp
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
```

Replace with:
```csharp
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

// ...

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, 
        new MySqlServerVersion(new Version(8, 0, 21)),
        mySqlOptions => {
            mySqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        }));
```

---

## Step 3: Update Connection String Format

### For PlanetScale:

Connection string format:
```
Server=host.planetscale.com;Database=database_name;User=username;Password=password;Port=3306;SslMode=Required;
```

Or use the connection string format from PlanetScale dashboard.

### Example appsettings.json:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=aws.connect.psdb.cloud;Database=clothpos;User=your_user;Password=your_password;Port=3306;SslMode=Required;"
  }
}
```

---

## Step 4: Handle MySQL-Specific Differences

### 1. String Length

MySQL has different default string lengths. Update your models if needed:

```csharp
[StringLength(255)] // Explicitly set length
public string Name { get; set; }
```

### 2. GUID Generation

MySQL handles GUIDs differently. Ensure your models use proper GUID generation:

```csharp
[Key]
public string Id { get; set; } = Guid.NewGuid().ToString();
```

### 3. DateTime

MySQL handles DateTime similarly, but ensure UTC:

```csharp
public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
```

---

## Step 5: Test Locally

1. **Install MySQL locally** (optional, for testing):
   - Download MySQL: https://dev.mysql.com/downloads/
   - Or use Docker: `docker run -p 3306:3306 -e MYSQL_ROOT_PASSWORD=password mysql:8.0`

2. **Update local connection string**:
   ```
   Server=localhost;Database=ClothPosDB;User=root;Password=password;Port=3306;
   ```

3. **Run the API**:
   ```bash
   dotnet run
   ```

4. **Verify database is created**:
   - Check MySQL to see if tables are created
   - Test API endpoints

---

## Step 6: Deploy to Render with PlanetScale

1. **Get PlanetScale connection string**:
   - Go to PlanetScale dashboard
   - Click "Connect"
   - Copy connection string

2. **Update Render environment variables**:
   ```
   ConnectionStrings__DefaultConnection = [PlanetScale connection string]
   ```

3. **Deploy**:
   - Render will automatically build and deploy
   - Check logs for any errors

---

## Common Issues & Solutions

### Issue: "Unknown database"

**Solution**: Create the database in PlanetScale first, or let EF Core create it.

### Issue: SSL Connection Required

**Solution**: Add `SslMode=Required;` to connection string.

### Issue: Migration Errors

**Solution**: 
- Drop and recreate database (development only)
- Or manually run SQL migrations

### Issue: String Length Errors

**Solution**: Add `[StringLength(255)]` attributes to string properties.

---

## Migration from SQL Server to MySQL

If you have existing SQL Server data:

1. **Export data** from SQL Server
2. **Convert data types** if needed
3. **Import to MySQL** (PlanetScale)
4. **Verify data integrity**

Or start fresh with new database (recommended for development).

---

## PlanetScale Specific Notes

### Branching

PlanetScale uses branches (like Git):
- **Main branch**: Production database
- **Dev branches**: For testing changes
- **Schema changes**: Made through branches, then merged

### Connection Limits

- Free tier: No connection limits
- Connections are pooled automatically

### SSL

- Always required
- Automatically handled by Pomelo provider

---

## Testing Checklist

- [ ] Local MySQL connection works
- [ ] Database tables created successfully
- [ ] API endpoints work
- [ ] Authentication works
- [ ] CRUD operations work
- [ ] Deployed to Render
- [ ] Production connection works

---

## Next Steps

1. Update code as shown above
2. Test locally with MySQL
3. Set up PlanetScale database
4. Deploy to Render
5. Update frontend API URL
6. Test production deployment

Good luck! 🚀


