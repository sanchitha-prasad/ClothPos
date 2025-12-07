# Database Schema Files

This directory contains database schema files for the ClothPos API.

## Files

1. **Schema.sql** - SQLite schema (default)
2. **Schema_SQLServer.sql** - SQL Server schema
3. **DATABASE_SCHEMA.md** - Complete documentation

## Usage

### SQLite (Default)

The database is automatically created by Entity Framework when you run the API. However, you can manually create it using:

```bash
sqlite3 ClothPos.db < Schema.sql
```

### SQL Server

1. Create a new database:
```sql
CREATE DATABASE ClothPosDB;
GO
USE ClothPosDB;
GO
```

2. Run the schema script:
```sql
-- Execute Schema_SQLServer.sql
```

Or use SQL Server Management Studio to execute the script.

## Entity Framework Migrations

If you want to use EF Migrations instead:

```bash
# Install EF Tools (if not already installed)
dotnet tool install --global dotnet-ef

# Create initial migration
dotnet ef migrations add InitialCreate

# Apply migration
dotnet ef database update
```

## Database Diagram

See `DATABASE_SCHEMA.md` for the complete ERD and relationship diagrams.


