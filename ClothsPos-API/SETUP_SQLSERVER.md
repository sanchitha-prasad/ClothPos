# SQL Server Setup Instructions

## Database Configuration Complete

The application has been configured to use SQL Server instead of SQLite.

## Connection String

The connection string has been updated in:
- `appsettings.json`
- `appsettings.Development.json`
- `Program.cs` (fallback)

**Connection String:**
```
Data Source=localhost;Initial Catalog=ClothPosDB;Integrated Security=True;Persist Security Info=False;Pooling=False;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Command Timeout=0
```

## Database Setup Steps

### 1. Create the Database

Open SQL Server Management Studio (SSMS) or use SQL Server Command Line and run:

```sql
CREATE DATABASE ClothPosDB;
GO
```

### 2. Run the Schema Script

Execute the SQL Server schema script to create all tables:

```sql
USE ClothPosDB;
GO

-- Execute the contents of Database/Schema_SQLServer.sql
```

Or use SSMS:
1. Open `ClothsPos-API/Database/Schema_SQLServer.sql`
2. Connect to your SQL Server instance
3. Select `ClothPosDB` as the database
4. Execute the script

### 3. Verify Database Creation

Check that all tables were created:
- Users
- Categories
- Items
- Sales
- SaleItems
- PaymentDues
- ShopSettings

### 4. Run the Application

```bash
cd ClothsPos-API
dotnet run
```

The application will now connect to SQL Server instead of SQLite.

## Important Notes

1. **Database Name**: The connection string uses `Initial Catalog=ClothPosDB`. Make sure this database exists.

2. **Integrated Security**: The connection uses Windows Authentication (Integrated Security=True). If you need SQL Server authentication, use:
   ```
   User ID=your_username;Password=your_password;
   ```

3. **TrustServerCertificate**: Set to `True` for local development. For production, use proper SSL certificates.

4. **Entity Framework**: The application uses EF Core with `EnsureCreated()`, which will automatically create tables if they don't exist. However, it's recommended to run the schema script first for proper foreign key constraints.

## Troubleshooting

### Error: Cannot open database "ClothPosDB"

**Solution**: Create the database first:
```sql
CREATE DATABASE ClothPosDB;
GO
```

### Error: Login failed for user

**Solution**: 
- Ensure you have SQL Server authentication enabled
- Or use SQL Server authentication in the connection string:
  ```
  User ID=sa;Password=your_password;
  ```

### Error: Foreign key constraint violations

**Solution**: Run the `Schema_SQLServer.sql` script to ensure proper table creation with foreign keys.

## Migration from SQLite

If you were using SQLite before:

1. **Export data** from SQLite (if needed)
2. **Create new SQL Server database**
3. **Run schema script** to create tables
4. **Import data** (if needed) using SQL Server Import/Export tools

## Next Steps

1. Create the database in SQL Server
2. Run the schema script
3. Start the application
4. Test the API endpoints

