-- ============================================
-- FIX: Restore Name column if it was renamed to Username
-- This script will:
-- 1. Check if Name column exists, if not add it
-- 2. Restore Name values from Username if Name is missing
-- 3. Keep both Name and Username columns
-- ============================================

USE ClothPosDB;
GO

-- Check if Name column exists
IF NOT EXISTS (
    SELECT 1 
    FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[Users]') 
    AND name = 'Name'
)
BEGIN
    PRINT 'Name column is missing. Restoring it...';
    
    -- Add Name column (temporarily nullable)
    ALTER TABLE [Users]
    ADD [Name] NVARCHAR(200) NULL;
    
    PRINT 'Restoring Name values from Username...';
    
    -- Copy Username values to Name (since Name was renamed to Username)
    UPDATE [Users]
    SET [Name] = [Username]
    WHERE [Name] IS NULL;
    
    -- For admin user, set proper name
    UPDATE [Users]
    SET [Name] = 'Admin User'
    WHERE [Email] = 'admin@shop.com' AND [Name] = 'admin';
    
    PRINT 'Making Name required...';
    
    -- Make Name NOT NULL
    ALTER TABLE [Users]
    ALTER COLUMN [Name] NVARCHAR(200) NOT NULL;
    
    PRINT '========================================';
    PRINT 'SUCCESS! Name column restored.';
    PRINT '========================================';
    PRINT 'Both Name and Username columns now exist.';
END
ELSE
BEGIN
    PRINT 'Name column already exists.';
    
    -- Check if we have Username column
    IF NOT EXISTS (
        SELECT 1 
        FROM sys.columns 
        WHERE object_id = OBJECT_ID(N'[Users]') 
        AND name = 'Username'
    )
    BEGIN
        PRINT 'Username column is missing. Adding it...';
        
        -- Add Username column
        ALTER TABLE [Users]
        ADD [Username] NVARCHAR(50) NULL;
        
        -- Set default usernames from email
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
        
        PRINT 'Username column added successfully.';
    END
    ELSE
    BEGIN
        PRINT 'Both Name and Username columns exist. Database is correct.';
    END
END
GO

-- Verify both columns exist
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Users' 
AND COLUMN_NAME IN ('Name', 'Username')
ORDER BY COLUMN_NAME;

GO

