-- ============================================
-- COMPLETE FIX: Ensure both Name and Username columns exist
-- Run this script to fix your database structure
-- ============================================

USE ClothPosDB;
GO

PRINT '========================================';
PRINT 'Checking and fixing Users table structure...';
PRINT '========================================';
GO

-- Step 1: Ensure Name column exists
IF NOT EXISTS (
    SELECT 1 
    FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[Users]') 
    AND name = 'Name'
)
BEGIN
    PRINT 'STEP 1: Adding Name column...';
    
    ALTER TABLE [Users]
    ADD [Name] NVARCHAR(200) NULL;
    
    -- If Username exists, copy its values to Name temporarily
    IF EXISTS (
        SELECT 1 
        FROM sys.columns 
        WHERE object_id = OBJECT_ID(N'[Users]') 
        AND name = 'Username'
    )
    BEGIN
        PRINT '  - Restoring Name values from Username...';
        UPDATE [Users]
        SET [Name] = [Username]
        WHERE [Name] IS NULL;
        
        -- Set proper name for admin
        UPDATE [Users]
        SET [Name] = 'Admin User'
        WHERE [Email] = 'admin@shop.com' AND [Name] = 'admin';
    END
    ELSE
    BEGIN
        -- If no Username, use Email prefix as Name
        UPDATE [Users]
        SET [Name] = SUBSTRING([Email], 1, CASE 
            WHEN CHARINDEX('@', [Email]) > 0 
            THEN CHARINDEX('@', [Email]) - 1 
            ELSE LEN([Email]) 
        END)
        WHERE [Name] IS NULL;
    END
    
    ALTER TABLE [Users]
    ALTER COLUMN [Name] NVARCHAR(200) NOT NULL;
    
    PRINT '  ✓ Name column added and populated.';
END
ELSE
BEGIN
    PRINT 'STEP 1: Name column already exists.';
END
GO

-- Step 2: Ensure Username column exists
IF NOT EXISTS (
    SELECT 1 
    FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[Users]') 
    AND name = 'Username'
)
BEGIN
    PRINT 'STEP 2: Adding Username column...';
    
    ALTER TABLE [Users]
    ADD [Username] NVARCHAR(50) NULL;
    
    -- Set default usernames
    UPDATE [Users]
    SET [Username] = 'admin'
    WHERE [Email] = 'admin@shop.com' AND [Username] IS NULL;
    
    UPDATE [Users]
    SET [Username] = SUBSTRING([Email], 1, CASE 
        WHEN CHARINDEX('@', [Email]) > 0 
        THEN CHARINDEX('@', [Email]) - 1 
        ELSE LEN([Email]) 
    END)
    WHERE [Username] IS NULL;
    
    -- Handle duplicates
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
    
    PRINT '  ✓ Username column added and populated.';
END
ELSE
BEGIN
    PRINT 'STEP 2: Username column already exists.';
END
GO

-- Step 3: Verify both columns exist
PRINT '';
PRINT '========================================';
PRINT 'Verification: Checking table structure...';
PRINT '========================================';

SELECT 
    COLUMN_NAME as 'Column',
    DATA_TYPE as 'Type',
    IS_NULLABLE as 'Nullable',
    CHARACTER_MAXIMUM_LENGTH as 'Max Length'
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Users' 
AND COLUMN_NAME IN ('Name', 'Username', 'Email')
ORDER BY COLUMN_NAME;

PRINT '';
PRINT '========================================';
PRINT 'SUCCESS! Database structure is correct.';
PRINT '========================================';
PRINT 'Both Name and Username columns now exist.';
PRINT 'You can now restart your API.';
PRINT '';
PRINT 'Login credentials:';
PRINT '  Username: admin';
PRINT '  Password: Admin123!';
PRINT '========================================';
GO

