# Step-by-Step Fix for Username Column Error

## 🎯 The Problem
Your database is missing the `Username` column. The API detected this and stopped.

## ✅ Solution: Run the SQL Script

### Step 1: Open SQL Server Management Studio (SSMS)
1. Press `Windows Key` and type "SQL Server Management Studio"
2. Click to open it
3. Connect to your server:
   - **Server name**: `localhost` (or your SQL Server instance name)
   - **Authentication**: Windows Authentication (usually)
   - Click **Connect**

### Step 2: Select Your Database
1. In the left panel (Object Explorer), expand **Databases**
2. Find and click on **`ClothPosDB`** (or your database name)
3. Make sure it's selected/highlighted

### Step 3: Open the SQL Script
1. In SSMS, click **File** → **Open** → **File**
2. Navigate to: `E:\Myproject\ClothsPos-WebPortal\ClothsPos-API\Database\`
3. Open **`RUN_THIS.sql`** (or `AddUsernameColumn.sql`)

### Step 4: Execute the Script
1. The SQL script should now be in the editor window
2. Press **F5** (or click the **Execute** button)
3. Wait for "Command(s) completed successfully" message

### Step 5: Restart Your API
1. Go back to your API terminal
2. Stop the API (Ctrl+C if running)
3. Start it again: `dotnet run`

### Step 6: Test Login
1. Open your frontend
2. Login with:
   - **Username**: `admin`
   - **Password**: `Admin123!`

## 🚨 Alternative: Quick Copy-Paste (If Script Fails)

If the script file doesn't work, copy and paste this directly into SSMS:

```sql
USE ClothPosDB;
GO

ALTER TABLE [Users] ADD [Username] NVARCHAR(50) NULL;
UPDATE [Users] SET [Username] = 'admin' WHERE [Email] = 'admin@shop.com';
UPDATE [Users] SET [Username] = SUBSTRING([Email], 1, CHARINDEX('@', [Email]) - 1) WHERE [Username] IS NULL;
ALTER TABLE [Users] ALTER COLUMN [Username] NVARCHAR(50) NOT NULL;
CREATE UNIQUE INDEX IX_Users_Username ON [Users]([Username]);
GO
```

## ❓ Troubleshooting

### "Cannot find database ClothPosDB"
- Check your `appsettings.json` for the correct database name
- Or replace `ClothPosDB` in the script with your actual database name

### "Permission denied"
- Make sure you're logged in as a user with ALTER TABLE permissions
- Try connecting as `sa` (system administrator) if needed

### "Table Users does not exist"
- Your database might not be initialized yet
- Restart the API first (it will create the table), then run the migration

### Still having issues?
Check the error message in SSMS - it will tell you exactly what went wrong.

## ✅ Success Indicators

After running the script, you should see:
- "SUCCESS! Username column added."
- Or "Command(s) completed successfully"

Then restart your API - it should work! 🎉

