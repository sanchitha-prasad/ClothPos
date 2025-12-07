-- Migration Script: Add Username column to Users table
-- Run this script on your SQL Server database to add the Username column

-- Check if column doesn't exist, then add it
IF NOT EXISTS (
    SELECT 1 
    FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[Users]') 
    AND name = 'Username'
)
BEGIN
    -- Add Username column
    ALTER TABLE [Users]
    ADD [Username] NVARCHAR(50) NULL;
    
    -- Update existing users with a default username (using email prefix)
    -- First, set admin user to 'admin' if exists
    UPDATE [Users]
    SET [Username] = 'admin'
    WHERE [Email] = 'admin@shop.com' AND [Username] IS NULL;
    
    -- For other users, use email prefix
    UPDATE [Users]
    SET [Username] = SUBSTRING([Email], 1, CHARINDEX('@', [Email]) - 1)
    WHERE [Username] IS NULL;
    
    -- Handle duplicate usernames by appending numbers
    DECLARE @Counter INT = 1;
    WHILE EXISTS (
        SELECT 1 
        FROM [Users] u1
        WHERE u1.[Username] IN (
            SELECT u2.[Username]
            FROM [Users] u2
            GROUP BY u2.[Username]
            HAVING COUNT(*) > 1
        )
    )
    BEGIN
        UPDATE u1
        SET u1.[Username] = u1.[Username] + CAST(@Counter AS NVARCHAR(10))
        FROM [Users] u1
        INNER JOIN (
            SELECT [Username], MIN([Id]) as MinId
            FROM [Users]
            GROUP BY [Username]
            HAVING COUNT(*) > 1
        ) dup ON u1.[Username] = dup.[Username] AND u1.[Id] > dup.MinId;
        
        SET @Counter = @Counter + 1;
    END
    
    -- Make Username NOT NULL and UNIQUE
    ALTER TABLE [Users]
    ALTER COLUMN [Username] NVARCHAR(50) NOT NULL;
    
    -- Create unique index
    CREATE UNIQUE INDEX IX_Users_Username ON [Users]([Username]);
    
    PRINT 'Username column added successfully!';
END
ELSE
BEGIN
    PRINT 'Username column already exists.';
END

