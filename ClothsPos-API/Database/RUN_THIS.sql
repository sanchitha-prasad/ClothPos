-- ============================================
-- COPY AND PASTE THIS ENTIRE SCRIPT INTO SQL SERVER MANAGEMENT STUDIO
-- ============================================
-- Make sure you're connected to the correct database (ClothPosDB)
-- Then select all (Ctrl+A) and execute (F5)
-- ============================================

USE ClothPosDB;
GO

-- First, ensure Name column exists (it should exist, but check anyway)
IF NOT EXISTS (
    SELECT 1 
    FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[Users]') 
    AND name = 'Name'
)
BEGIN
    PRINT 'WARNING: Name column is missing! Adding it...';
    ALTER TABLE [Users]
    ADD [Name] NVARCHAR(200) NULL;
    
    -- Try to restore Name from Username if Username exists
    IF EXISTS (
        SELECT 1 
        FROM sys.columns 
        WHERE object_id = OBJECT_ID(N'[Users]') 
        AND name = 'Username'
    )
    BEGIN
        UPDATE [Users] SET [Name] = [Username] WHERE [Name] IS NULL;
    END
    
    ALTER TABLE [Users] ALTER COLUMN [Name] NVARCHAR(200) NOT NULL;
END

-- Check if Username column doesn't exist, then add it
IF NOT EXISTS (
    SELECT 1 
    FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[Users]') 
    AND name = 'Username'
)
BEGIN
    PRINT 'Adding Username column...';
    
    -- Add Username column (temporarily nullable)
    ALTER TABLE [Users]
    ADD [Username] NVARCHAR(50) NULL;
    
    PRINT 'Setting default usernames...';
    
    -- Set admin user to 'admin'
    UPDATE [Users]
    SET [Username] = 'admin'
    WHERE [Email] = 'admin@shop.com' AND [Username] IS NULL;
    
    -- For other users, use email prefix (before @)
    UPDATE [Users]
    SET [Username] = SUBSTRING([Email], 1, CASE 
        WHEN CHARINDEX('@', [Email]) > 0 
        THEN CHARINDEX('@', [Email]) - 1 
        ELSE LEN([Email]) 
    END)
    WHERE [Username] IS NULL;
    
    PRINT 'Handling duplicate usernames...';
    
    -- Handle duplicates by appending numbers
    DECLARE @Counter INT = 1;
    WHILE EXISTS (
        SELECT 1 
        FROM [Users] u1
        WHERE u1.[Username] IN (
            SELECT u2.[Username]
            FROM [Users] u2
            WHERE u2.[Username] IS NOT NULL
            GROUP BY u2.[Username]
            HAVING COUNT(*) > 1
        )
    ) AND @Counter < 100
    BEGIN
        UPDATE u1
        SET u1.[Username] = u1.[Username] + CAST(@Counter AS NVARCHAR(10))
        FROM [Users] u1
        INNER JOIN (
            SELECT [Username], MIN([Id]) as MinId
            FROM [Users]
            WHERE [Username] IS NOT NULL
            GROUP BY [Username]
            HAVING COUNT(*) > 1
        ) dup ON u1.[Username] = dup.[Username] AND u1.[Id] > dup.MinId;
        
        SET @Counter = @Counter + 1;
    END
    
    PRINT 'Making Username required and unique...';
    
    -- Make Username NOT NULL
    ALTER TABLE [Users]
    ALTER COLUMN [Username] NVARCHAR(50) NOT NULL;
    
    -- Create unique index
    IF NOT EXISTS (
        SELECT 1 FROM sys.indexes 
        WHERE name = 'IX_Users_Username' 
        AND object_id = OBJECT_ID(N'[Users]')
    )
    BEGIN
        CREATE UNIQUE INDEX IX_Users_Username ON [Users]([Username]);
    END
    
    PRINT '========================================';
    PRINT 'SUCCESS! Username column added.';
    PRINT '========================================';
    PRINT 'You can now restart your API.';
    PRINT 'Login with: Username: admin, Password: Admin123!';
END
ELSE
BEGIN
    PRINT 'Username column already exists.';
END
GO

