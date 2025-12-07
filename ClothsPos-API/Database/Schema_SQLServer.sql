-- ClothPos Database Schema
-- SQL Server Version
-- Generated for ClothPos-API

-- ============================================
-- Table: Users
-- ============================================
CREATE TABLE [Users] (
    [Id] NVARCHAR(50) PRIMARY KEY,
    [Name] NVARCHAR(200) NOT NULL,
    [Username] NVARCHAR(50) NOT NULL UNIQUE,
    [Email] NVARCHAR(200) NOT NULL UNIQUE,
    [Role] INT NOT NULL,
    [Passcode] NVARCHAR(MAX) NULL,
    [Permissions] NVARCHAR(MAX) NOT NULL DEFAULT '[]',
    [IsActive] BIT NOT NULL DEFAULT 1,
    [CreatedAt] DATETIME2 NOT NULL,
    [UpdatedAt] DATETIME2 NOT NULL
);

CREATE UNIQUE INDEX IX_Users_Email ON [Users]([Email]);
CREATE UNIQUE INDEX IX_Users_Username ON [Users]([Username]);

-- ============================================
-- Table: Categories
-- ============================================
CREATE TABLE [Categories] (
    [Id] NVARCHAR(50) PRIMARY KEY,
    [Name] NVARCHAR(100) NOT NULL UNIQUE,
    [Description] NVARCHAR(500) NULL,
    [CreatedAt] DATETIME2 NOT NULL,
    [UpdatedAt] DATETIME2 NOT NULL
);

CREATE UNIQUE INDEX IX_Categories_Name ON [Categories]([Name]);

-- ============================================
-- Table: Items
-- ============================================
CREATE TABLE [Items] (
    [Id] NVARCHAR(50) PRIMARY KEY,
    [Name] NVARCHAR(200) NOT NULL,
    [Description] NVARCHAR(1000) NULL,
    [CategoryId] NVARCHAR(50) NOT NULL,
    [Price] DECIMAL(18,2) NOT NULL,
    [Cost] DECIMAL(18,2) NOT NULL DEFAULT 0,
    [Stock] DECIMAL(18,2) NOT NULL DEFAULT 0,
    [MinStockLevel] DECIMAL(18,2) NOT NULL DEFAULT 10,
    [Barcode] NVARCHAR(50) NULL,
    [SKU] NVARCHAR(50) NOT NULL UNIQUE,
    [Code] NVARCHAR(50) NULL,
    [Brand] NVARCHAR(100) NULL,
    [ProductUnit] NVARCHAR(50) NOT NULL DEFAULT 'piece',
    [SaleUnit] NVARCHAR(50) NOT NULL DEFAULT 'piece',
    [PurchaseUnit] NVARCHAR(50) NOT NULL DEFAULT 'piece',
    [Images] NVARCHAR(MAX) NOT NULL DEFAULT '[]',
    [Sizes] NVARCHAR(MAX) NOT NULL DEFAULT '[]',
    [Colors] NVARCHAR(MAX) NOT NULL DEFAULT '[]',
    [Note] NVARCHAR(MAX) NULL,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [CreatedAt] DATETIME2 NOT NULL,
    [UpdatedAt] DATETIME2 NOT NULL,
    CONSTRAINT FK_Items_Categories FOREIGN KEY ([CategoryId]) 
        REFERENCES [Categories]([Id]) ON DELETE NO ACTION
);

CREATE UNIQUE INDEX IX_Items_SKU ON [Items]([SKU]);
CREATE INDEX IX_Items_Code ON [Items]([Code]);
CREATE INDEX IX_Items_CategoryId ON [Items]([CategoryId]);

-- ============================================
-- Table: Sales
-- ============================================
CREATE TABLE [Sales] (
    [Id] NVARCHAR(50) PRIMARY KEY,
    [Date] DATETIME2 NOT NULL,
    [Total] DECIMAL(18,2) NOT NULL,
    [Tax] DECIMAL(18,2) NOT NULL DEFAULT 0,
    [Subtotal] DECIMAL(18,2) NOT NULL,
    [PaymentMethod] NVARCHAR(50) NOT NULL DEFAULT 'cash',
    [Status] NVARCHAR(20) NOT NULL DEFAULT 'completed',
    [CustomerName] NVARCHAR(200) NULL,
    [CashierId] NVARCHAR(50) NOT NULL,
    CONSTRAINT FK_Sales_Users FOREIGN KEY ([CashierId]) 
        REFERENCES [Users]([Id]) ON DELETE NO ACTION
);

CREATE INDEX IX_Sales_Date ON [Sales]([Date]);
CREATE INDEX IX_Sales_Status ON [Sales]([Status]);
CREATE INDEX IX_Sales_CashierId ON [Sales]([CashierId]);

-- ============================================
-- Table: SaleItems
-- ============================================
CREATE TABLE [SaleItems] (
    [Id] NVARCHAR(50) PRIMARY KEY,
    [SaleId] NVARCHAR(50) NOT NULL,
    [ItemId] NVARCHAR(50) NOT NULL,
    [Quantity] DECIMAL(18,2) NOT NULL,
    [Price] DECIMAL(18,2) NOT NULL,
    [Total] DECIMAL(18,2) NOT NULL,
    CONSTRAINT FK_SaleItems_Sales FOREIGN KEY ([SaleId]) 
        REFERENCES [Sales]([Id]) ON DELETE CASCADE,
    CONSTRAINT FK_SaleItems_Items FOREIGN KEY ([ItemId]) 
        REFERENCES [Items]([Id]) ON DELETE NO ACTION
);

CREATE INDEX IX_SaleItems_SaleId ON [SaleItems]([SaleId]);
CREATE INDEX IX_SaleItems_ItemId ON [SaleItems]([ItemId]);

-- ============================================
-- Table: PaymentDues
-- ============================================
CREATE TABLE [PaymentDues] (
    [Id] NVARCHAR(50) PRIMARY KEY,
    [SaleId] NVARCHAR(50) NOT NULL,
    [Amount] DECIMAL(18,2) NOT NULL,
    [DueDate] DATETIME2 NOT NULL,
    [Status] NVARCHAR(20) NOT NULL DEFAULT 'pending',
    CONSTRAINT FK_PaymentDues_Sales FOREIGN KEY ([SaleId]) 
        REFERENCES [Sales]([Id]) ON DELETE NO ACTION
);

CREATE INDEX IX_PaymentDues_SaleId ON [PaymentDues]([SaleId]);
CREATE INDEX IX_PaymentDues_Status ON [PaymentDues]([Status]);
CREATE INDEX IX_PaymentDues_DueDate ON [PaymentDues]([DueDate]);

-- ============================================
-- Table: ShopSettings
-- ============================================
CREATE TABLE [ShopSettings] (
    [Id] NVARCHAR(50) PRIMARY KEY,
    [ShopName] NVARCHAR(200) NOT NULL DEFAULT 'My Shop',
    [Logo] NVARCHAR(MAX) NULL,
    [Address] NVARCHAR(500) NOT NULL,
    [City] NVARCHAR(100) NOT NULL,
    [State] NVARCHAR(100) NOT NULL,
    [ZipCode] NVARCHAR(20) NOT NULL,
    [Country] NVARCHAR(100) NOT NULL DEFAULT 'Sri Lanka',
    [Phone] NVARCHAR(20) NOT NULL,
    [Email] NVARCHAR(200) NOT NULL,
    [TaxRate] DECIMAL(5,2) NOT NULL DEFAULT 0,
    [RoundingRule] NVARCHAR(20) NOT NULL DEFAULT 'none',
    [ReceiptTemplate] NVARCHAR(MAX) NOT NULL DEFAULT 'default',
    [PrinterName] NVARCHAR(100) NULL,
    [CurrencySymbol] NVARCHAR(10) NOT NULL DEFAULT 'Rs.',
    [CurrencyCode] NVARCHAR(3) NOT NULL DEFAULT 'LKR',
    [CurrencyPosition] NVARCHAR(10) NOT NULL DEFAULT 'before',
    [POSDevices] NVARCHAR(MAX) NOT NULL DEFAULT '[]'
);

-- ============================================
-- Seed Data: Categories
-- ============================================
IF NOT EXISTS (SELECT 1 FROM [Categories] WHERE [Id] = '1')
BEGIN
    INSERT INTO [Categories] ([Id], [Name], [Description], [CreatedAt], [UpdatedAt]) VALUES
    ('1', 'Men''s Clothing', 'All men''s apparel', GETUTCDATE(), GETUTCDATE()),
    ('2', 'Women''s Clothing', 'All women''s apparel', GETUTCDATE(), GETUTCDATE()),
    ('3', 'Accessories', 'Bags, belts, watches', GETUTCDATE(), GETUTCDATE()),
    ('4', 'Shoes', 'Footwear for all', GETUTCDATE(), GETUTCDATE());
END

-- ============================================
-- Seed Data: ShopSettings
-- ============================================
IF NOT EXISTS (SELECT 1 FROM [ShopSettings] WHERE [Id] = '1')
BEGIN
    INSERT INTO [ShopSettings] (
        [Id], [ShopName], [Address], [City], [State], [ZipCode], [Country], 
        [Phone], [Email], [TaxRate], [RoundingRule], [ReceiptTemplate],
        [CurrencySymbol], [CurrencyCode], [CurrencyPosition], [POSDevices]
    ) VALUES (
        '1', 
        'ClothPos Shop', 
        '123 Main Street', 
        'Colombo', 
        'Western Province', 
        '00100', 
        'Sri Lanka',
        '+94 11 1234567', 
        'info@clothspos.com', 
        0, 
        'none', 
        'default',
        'Rs.', 
        'LKR', 
        'before', 
        '[]'
    );
END


