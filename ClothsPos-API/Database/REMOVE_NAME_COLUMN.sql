-- ============================================
-- REMOVE Name Column from Users Table
-- Run this script to remove the Name column
-- ============================================

USE ClothPosDB;
GO

-- Check if Name column exists, then remove it
IF EXISTS (
    SELECT 1 
    FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[Users]') 
    AND name = 'Name'
)
BEGIN
    PRINT 'Removing Name column from Users table...';
    
    -- Drop the column
    ALTER TABLE [Users]
    DROP COLUMN [Name];
    
    PRINT '========================================';
    PRINT 'SUCCESS! Name column removed.';
    PRINT '========================================';
    PRINT 'Only Username column remains for user identification.';
END
ELSE
BEGIN
    PRINT 'Name column does not exist. Database structure is correct.';
END
GO

-- Verify column structure
SELECT 
    COLUMN_NAME as 'Column',
    DATA_TYPE as 'Type',
    IS_NULLABLE as 'Nullable',
    CHARACTER_MAXIMUM_LENGTH as 'Max Length'
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Users' 
AND COLUMN_NAME IN ('Username', 'Email')
ORDER BY COLUMN_NAME;

GO

