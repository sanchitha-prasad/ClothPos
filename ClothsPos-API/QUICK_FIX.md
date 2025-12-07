# Quick Fix: Username Column Missing

## The Problem
Your database table `Users` doesn't have the `Username` column yet, causing the error:
```
Invalid column name 'Username'
```

## Quick Solution (Choose One)

### Option 1: Run Migration Script (Keeps Your Data) ✅ RECOMMENDED

1. **Open SQL Server Management Studio**
2. **Connect to your server** (localhost)
3. **Select your database** (`ClothPosDB`)
4. **Open this file**: `ClothsPos-API/Database/AddUsernameColumn.sql`
5. **Execute the script** (Press F5 or click Execute)

The script will:
- Add the `Username` column
- Set default usernames for existing users
- Make it required and unique

### Option 2: Drop and Recreate (Deletes All Data) ⚠️ DEVELOPMENT ONLY

If you don't have important data:

1. **Open SQL Server Management Studio**
2. **Run this SQL**:
   ```sql
   DROP DATABASE ClothPosDB;
   ```
3. **Restart your API** - it will create a fresh database with the new schema

### Option 3: Manual SQL (If Script Fails)

Run these commands one by one:

```sql
-- 1. Add the column (temporarily nullable)
ALTER TABLE [Users]
ADD [Username] NVARCHAR(50) NULL;

-- 2. Set default usernames
UPDATE [Users]
SET [Username] = 'admin'
WHERE [Email] = 'admin@shop.com';

UPDATE [Users]
SET [Username] = SUBSTRING([Email], 1, CHARINDEX('@', [Email]) - 1)
WHERE [Username] IS NULL;

-- 3. Make it required
ALTER TABLE [Users]
ALTER COLUMN [Username] NVARCHAR(50) NOT NULL;

-- 4. Create unique index
CREATE UNIQUE INDEX IX_Users_Username ON [Users]([Username]);
```

## After Migration

1. **Restart your API**
2. **Login with**:
   - Username: `admin`
   - Password: `Admin123!`

## Still Having Issues?

Check:
- ✅ You're connected to the correct database
- ✅ You have ALTER TABLE permissions
- ✅ The Users table exists
- ✅ No other applications are using the database

