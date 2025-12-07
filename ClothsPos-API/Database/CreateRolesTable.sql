-- ============================================
-- Create Roles Table
-- Run this script to create the Roles table
-- ============================================

USE ClothPosDB;
GO

-- Create Roles table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Roles]') AND type in (N'U'))
BEGIN
    CREATE TABLE [Roles] (
        [Id] NVARCHAR(450) PRIMARY KEY,
        [Name] NVARCHAR(50) NOT NULL UNIQUE,
        [Description] NVARCHAR(200) NULL,
        [Permissions] NVARCHAR(MAX) NOT NULL DEFAULT '[]',
        [IsActive] BIT NOT NULL DEFAULT 1,
        [IsSystemRole] BIT NOT NULL DEFAULT 0,
        [DisplayOrder] INT NOT NULL DEFAULT 0,
        [CreatedAt] DATETIME2 NOT NULL,
        [UpdatedAt] DATETIME2 NOT NULL
    );
    
    CREATE UNIQUE INDEX IX_Roles_Name ON [Roles]([Name]);
    
    PRINT 'Roles table created successfully.';
END
ELSE
BEGIN
    PRINT 'Roles table already exists.';
END
GO

-- Insert default roles
IF NOT EXISTS (SELECT 1 FROM [Roles])
BEGIN
    INSERT INTO [Roles] ([Id], [Name], [Description], [Permissions], [IsActive], [IsSystemRole], [DisplayOrder], [CreatedAt], [UpdatedAt])
    VALUES
        ('1', 'Admin', 'Full system access with all permissions', '["all"]', 1, 1, 1, GETUTCDATE(), GETUTCDATE()),
        ('2', 'Cashier', 'Can process sales and view inventory', '["sales", "inventory_view"]', 1, 1, 2, GETUTCDATE(), GETUTCDATE()),
        ('3', 'Mobile Staff', 'Can process sales on mobile devices', '["sales"]', 1, 1, 3, GETUTCDATE(), GETUTCDATE());
    
    PRINT 'Default roles inserted successfully.';
END
ELSE
BEGIN
    PRINT 'Roles already exist.';
END
GO

-- Add RoleId column to Users table if it doesn't exist
IF NOT EXISTS (
    SELECT 1 
    FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[Users]') 
    AND name = 'RoleId'
)
BEGIN
    -- First, ensure Roles table exists and has data
    IF NOT EXISTS (SELECT 1 FROM [Roles] WHERE [Id] = '1')
    BEGIN
        PRINT 'Warning: Admin role (Id=1) does not exist. Please ensure roles are seeded first.';
    END
    
    -- Add RoleId column (temporarily nullable)
    ALTER TABLE [Users]
    ADD [RoleId] NVARCHAR(450) NULL;
    
    PRINT 'RoleId column added (nullable).';
END
ELSE
BEGIN
    PRINT 'RoleId column already exists in Users table.';
END
GO

-- Set default RoleId for existing users (only if column exists)
IF EXISTS (
    SELECT 1 
    FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[Users]') 
    AND name = 'RoleId'
)
BEGIN
    -- Set default RoleId for existing users
    -- Assign all existing users to Admin role (Id = '1')
    -- You may need to adjust this based on your actual data
    UPDATE [Users]
    SET [RoleId] = '1' -- Admin role ID
    WHERE [RoleId] IS NULL;
    
    PRINT 'Default RoleId values set for existing users.';
END
GO

-- Make RoleId NOT NULL after populating it
IF EXISTS (
    SELECT 1 
    FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[Users]') 
    AND name = 'RoleId'
    AND is_nullable = 1
)
BEGIN
    ALTER TABLE [Users]
    ALTER COLUMN [RoleId] NVARCHAR(450) NOT NULL;
    
    PRINT 'RoleId column set to NOT NULL.';
END
GO

-- Add foreign key constraint if it doesn't exist
IF NOT EXISTS (
    SELECT 1 
    FROM sys.foreign_keys 
    WHERE name = 'FK_Users_Roles_RoleId'
)
BEGIN
    IF EXISTS (
        SELECT 1 
        FROM sys.columns 
        WHERE object_id = OBJECT_ID(N'[Users]') 
        AND name = 'RoleId'
    )
    BEGIN
        ALTER TABLE [Users]
        ADD CONSTRAINT FK_Users_Roles_RoleId 
        FOREIGN KEY ([RoleId]) REFERENCES [Roles]([Id])
        ON DELETE NO ACTION;
        
        PRINT 'Foreign key constraint added successfully.';
    END
END
ELSE
BEGIN
    PRINT 'Foreign key constraint already exists.';
END
GO

-- Create index if it doesn't exist
IF NOT EXISTS (
    SELECT 1 
    FROM sys.indexes 
    WHERE name = 'IX_Users_RoleId' 
    AND object_id = OBJECT_ID(N'[Users]')
)
BEGIN
    IF EXISTS (
        SELECT 1 
        FROM sys.columns 
        WHERE object_id = OBJECT_ID(N'[Users]') 
        AND name = 'RoleId'
    )
    BEGIN
        CREATE INDEX IX_Users_RoleId ON [Users]([RoleId]);
        
        PRINT 'Index IX_Users_RoleId created successfully.';
    END
END
ELSE
BEGIN
    PRINT 'Index IX_Users_RoleId already exists.';
END
GO

-- Remove old Role column if it exists (if you had a Role enum column)
IF EXISTS (
    SELECT 1 
    FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[Users]') 
    AND name = 'Role'
    AND system_type_id = 56 -- INT type (enum was stored as int)
)
BEGIN
    ALTER TABLE [Users]
    DROP COLUMN [Role];
    
    PRINT 'Old Role enum column removed successfully.';
END
ELSE
BEGIN
    PRINT 'Old Role column does not exist or is not an INT type.';
END
GO

PRINT '';
PRINT '========================================';
PRINT 'SUCCESS! Roles table setup complete.';
PRINT '========================================';
GO

