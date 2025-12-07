-- Create ReceiptTemplates table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ReceiptTemplates]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[ReceiptTemplates] (
        [Id] NVARCHAR(450) NOT NULL PRIMARY KEY,
        [Name] NVARCHAR(200) NOT NULL,
        [Description] NVARCHAR(500) NULL,
        [Content] NVARCHAR(MAX) NOT NULL,
        [Structure] NVARCHAR(MAX) NULL,
        [IsDefault] BIT NOT NULL DEFAULT 0,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );

    -- Create index on Name
    CREATE INDEX [IX_ReceiptTemplates_Name] ON [dbo].[ReceiptTemplates] ([Name]);

    PRINT 'ReceiptTemplates table created successfully';
END
ELSE
BEGIN
    -- Check if Structure column exists, if not add it
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ReceiptTemplates]') AND name = 'Structure')
    BEGIN
        ALTER TABLE [dbo].[ReceiptTemplates] ADD [Structure] NVARCHAR(MAX) NULL;
        PRINT 'Structure column added to ReceiptTemplates table';
    END

    -- Check if CreatedAt and UpdatedAt columns exist, if not add them
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ReceiptTemplates]') AND name = 'CreatedAt')
    BEGIN
        ALTER TABLE [dbo].[ReceiptTemplates] ADD [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE();
        PRINT 'CreatedAt column added to ReceiptTemplates table';
    END

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ReceiptTemplates]') AND name = 'UpdatedAt')
    BEGIN
        ALTER TABLE [dbo].[ReceiptTemplates] ADD [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE();
        PRINT 'UpdatedAt column added to ReceiptTemplates table';
    END

    PRINT 'ReceiptTemplates table already exists, structure updated if needed';
END
GO

-- Create EmailTemplates table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EmailTemplates]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[EmailTemplates] (
        [Id] NVARCHAR(450) NOT NULL PRIMARY KEY,
        [Name] NVARCHAR(200) NOT NULL,
        [Description] NVARCHAR(500) NULL,
        [Subject] NVARCHAR(200) NOT NULL,
        [Body] NVARCHAR(MAX) NOT NULL,
        [Structure] NVARCHAR(MAX) NULL,
        [Type] NVARCHAR(50) NOT NULL DEFAULT 'general',
        [IsActive] BIT NOT NULL DEFAULT 1,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );

    -- Create indexes
    CREATE INDEX [IX_EmailTemplates_Name] ON [dbo].[EmailTemplates] ([Name]);
    CREATE INDEX [IX_EmailTemplates_Type] ON [dbo].[EmailTemplates] ([Type]);

    PRINT 'EmailTemplates table created successfully';
END
ELSE
BEGIN
    -- Check if Structure column exists, if not add it
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[EmailTemplates]') AND name = 'Structure')
    BEGIN
        ALTER TABLE [dbo].[EmailTemplates] ADD [Structure] NVARCHAR(MAX) NULL;
        PRINT 'Structure column added to EmailTemplates table';
    END

    -- Check if CreatedAt and UpdatedAt columns exist, if not add them
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[EmailTemplates]') AND name = 'CreatedAt')
    BEGIN
        ALTER TABLE [dbo].[EmailTemplates] ADD [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE();
        PRINT 'CreatedAt column added to EmailTemplates table';
    END

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[EmailTemplates]') AND name = 'UpdatedAt')
    BEGIN
        ALTER TABLE [dbo].[EmailTemplates] ADD [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE();
        PRINT 'UpdatedAt column added to EmailTemplates table';
    END

    PRINT 'EmailTemplates table already exists, structure updated if needed';
END
GO

