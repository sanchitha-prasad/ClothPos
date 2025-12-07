# Database Migration Guide: Adding Username Column

## Problem
The `Users` table in your SQL Server database doesn't have the `Username` column yet, causing the error:
```
Invalid column name 'Username'
```

## Solution

You have two options:

### Option 1: Run Migration SQL Script (Recommended)

1. **Open SQL Server Management Studio** (SSMS) or your preferred SQL client
2. **Connect to your database** (the one specified in `appsettings.json`)
3. **Open and run** the migration script:
   ```
   ClothsPos-API/Database/AddUsernameColumn.sql
   ```

This script will:
- Add the `Username` column to the `Users` table
- Set default usernames for existing users (using email prefix)
- Make the column NOT NULL and UNIQUE
- Create the unique index

### Option 2: Drop and Recreate Database (Development Only)

⚠️ **WARNING**: This will delete all your data!

1. **Stop the API** if it's running
2. **Delete the database** in SQL Server:
   ```sql
   DROP DATABASE ClothPosDB;
   ```
3. **Restart the API** - it will create the database with the new schema

### Option 3: Use Entity Framework Migrations (Advanced)

If you want to use EF Core migrations:

1. **Install EF Core Tools** (if not already installed):
   ```bash
   dotnet tool install --global dotnet-ef
   ```

2. **Create a migration**:
   ```bash
   cd ClothsPos-API
   dotnet ef migrations add AddUsernameToUser
   ```

3. **Apply the migration**:
   ```bash
   dotnet ef database update
   ```

## Verification

After running the migration, verify the column exists:

```sql
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'Username'
```

You should see:
- `COLUMN_NAME`: Username
- `DATA_TYPE`: nvarchar
- `IS_NULLABLE`: NO

## Default Admin Credentials

After migration, the admin user will be created with:
- **Username**: `admin`
- **Password**: `Admin123!`
- **Email**: `admin@shop.com`

## Troubleshooting

### If you get "Username already exists" error:
The script tries to create usernames from email addresses. If there are duplicates, you may need to manually set unique usernames:

```sql
-- Check for duplicate usernames
SELECT Username, COUNT(*) as Count
FROM Users
GROUP BY Username
HAVING COUNT(*) > 1;

-- Manually fix duplicates
UPDATE Users
SET Username = 'admin1'
WHERE Id = 'your-user-id';
```

### If migration script fails:
Make sure you're connected to the correct database and have appropriate permissions (ALTER TABLE, CREATE INDEX).

