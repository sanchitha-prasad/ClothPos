-- ============================================
-- Update Admin User Script
-- Run this to update existing admin user or ensure it has Admin role
-- ============================================

USE ClothPosDB;
GO

-- Check if Admin role exists
IF NOT EXISTS (SELECT 1 FROM [Roles] WHERE [Name] = 'Admin')
BEGIN
    PRINT 'ERROR: Admin role does not exist. Please run CreateRolesTable.sql first.';
    RETURN;
END
GO

-- Get Admin role ID
DECLARE @AdminRoleId NVARCHAR(450);
SELECT @AdminRoleId = [Id] FROM [Roles] WHERE [Name] = 'Admin';

-- Update existing user with username 'admin-dev' or 'Admin-dev' to have Admin role
IF EXISTS (SELECT 1 FROM [Users] WHERE [Username] = 'admin-dev' OR [Username] = 'Admin-dev')
BEGIN
    UPDATE [Users]
    SET [RoleId] = @AdminRoleId,
        [IsActive] = 1,
        [UpdatedAt] = GETUTCDATE()
    WHERE [Username] = 'admin-dev' OR [Username] = 'Admin-dev';
    
    PRINT 'Updated existing user to have Admin role.';
END
ELSE IF EXISTS (SELECT 1 FROM [Users] WHERE [Username] = 'admin')
BEGIN
    -- Update username from 'admin' to 'admin-dev' and ensure Admin role
    UPDATE [Users]
    SET [Username] = 'admin-dev',
        [RoleId] = @AdminRoleId,
        [IsActive] = 1,
        [UpdatedAt] = GETUTCDATE()
    WHERE [Username] = 'admin';
    
    PRINT 'Updated username from "admin" to "admin-dev" and assigned Admin role.';
END
ELSE
BEGIN
    -- Create new admin user if none exists
    IF NOT EXISTS (SELECT 1 FROM [Users] WHERE [Username] = 'admin-dev' OR [Email] = 'admin@shop.com')
    BEGIN
        INSERT INTO [Users] ([Id], [Username], [Email], [RoleId], [Passcode], [Permissions], [IsActive], [CreatedAt], [UpdatedAt])
        VALUES (
            NEWID(),
            'admin-dev',
            'admin@shop.com',
            @AdminRoleId,
            '$2a$11$YourHashedPasswordHere', -- This will be updated by the API seeder
            '["all"]',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
        
        PRINT 'Created new admin user. Note: Password will be set by API seeder on next restart.';
    END
END
GO

-- Verify the admin user
SELECT 
    u.[Id],
    u.[Username],
    u.[Email],
    r.[Name] AS [RoleName],
    u.[IsActive],
    u.[CreatedAt]
FROM [Users] u
LEFT JOIN [Roles] r ON u.[RoleId] = r.[Id]
WHERE u.[Username] = 'admin-dev' OR u.[Email] = 'admin@shop.com';
GO

PRINT '';
PRINT '========================================';
PRINT 'Admin user update complete!';
PRINT '========================================';
PRINT 'Username: admin-dev';
PRINT 'Email: admin@shop.com';
PRINT 'Role: Admin';
PRINT '========================================';
GO

