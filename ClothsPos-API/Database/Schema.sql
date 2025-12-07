-- ClothPos Database Schema
-- SQLite Version
-- Generated for ClothPos-API

-- ============================================
-- Table: Users
-- ============================================
CREATE TABLE IF NOT EXISTS Users (
    Id TEXT PRIMARY KEY,
    Name TEXT NOT NULL,
    Username TEXT NOT NULL UNIQUE,
    Email TEXT NOT NULL UNIQUE,
    Role INTEGER NOT NULL,
    Passcode TEXT,
    Permissions TEXT NOT NULL DEFAULT '[]',
    IsActive INTEGER NOT NULL DEFAULT 1,
    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT NOT NULL
);

CREATE INDEX IF NOT EXISTS IX_Users_Email ON Users(Email);
CREATE UNIQUE INDEX IF NOT EXISTS IX_Users_Username ON Users(Username);

-- ============================================
-- Table: Categories
-- ============================================
CREATE TABLE IF NOT EXISTS Categories (
    Id TEXT PRIMARY KEY,
    Name TEXT NOT NULL UNIQUE,
    Description TEXT,
    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT NOT NULL
);

CREATE INDEX IF NOT EXISTS IX_Categories_Name ON Categories(Name);

-- ============================================
-- Table: Items
-- ============================================
CREATE TABLE IF NOT EXISTS Items (
    Id TEXT PRIMARY KEY,
    Name TEXT NOT NULL,
    Description TEXT,
    CategoryId TEXT NOT NULL,
    Price DECIMAL(18,2) NOT NULL,
    Cost DECIMAL(18,2) NOT NULL DEFAULT 0,
    Stock DECIMAL(18,2) NOT NULL DEFAULT 0,
    MinStockLevel DECIMAL(18,2) NOT NULL DEFAULT 10,
    Barcode TEXT,
    SKU TEXT NOT NULL UNIQUE,
    Code TEXT,
    Brand TEXT,
    ProductUnit TEXT NOT NULL DEFAULT 'piece',
    SaleUnit TEXT NOT NULL DEFAULT 'piece',
    PurchaseUnit TEXT NOT NULL DEFAULT 'piece',
    Images TEXT NOT NULL DEFAULT '[]',
    Sizes TEXT NOT NULL DEFAULT '[]',
    Colors TEXT NOT NULL DEFAULT '[]',
    Note TEXT,
    IsActive INTEGER NOT NULL DEFAULT 1,
    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT NOT NULL,
    FOREIGN KEY (CategoryId) REFERENCES Categories(Id) ON DELETE RESTRICT
);

CREATE INDEX IF NOT EXISTS IX_Items_SKU ON Items(SKU);
CREATE INDEX IF NOT EXISTS IX_Items_Code ON Items(Code);
CREATE INDEX IF NOT EXISTS IX_Items_CategoryId ON Items(CategoryId);

-- ============================================
-- Table: Sales
-- ============================================
CREATE TABLE IF NOT EXISTS Sales (
    Id TEXT PRIMARY KEY,
    Date TEXT NOT NULL,
    Total DECIMAL(18,2) NOT NULL,
    Tax DECIMAL(18,2) NOT NULL DEFAULT 0,
    Subtotal DECIMAL(18,2) NOT NULL,
    PaymentMethod TEXT NOT NULL DEFAULT 'cash',
    Status TEXT NOT NULL DEFAULT 'completed',
    CustomerName TEXT,
    CashierId TEXT NOT NULL,
    FOREIGN KEY (CashierId) REFERENCES Users(Id) ON DELETE RESTRICT
);

CREATE INDEX IF NOT EXISTS IX_Sales_Date ON Sales(Date);
CREATE INDEX IF NOT EXISTS IX_Sales_Status ON Sales(Status);
CREATE INDEX IF NOT EXISTS IX_Sales_CashierId ON Sales(CashierId);

-- ============================================
-- Table: SaleItems
-- ============================================
CREATE TABLE IF NOT EXISTS SaleItems (
    Id TEXT PRIMARY KEY,
    SaleId TEXT NOT NULL,
    ItemId TEXT NOT NULL,
    Quantity DECIMAL(18,2) NOT NULL,
    Price DECIMAL(18,2) NOT NULL,
    Total DECIMAL(18,2) NOT NULL,
    FOREIGN KEY (SaleId) REFERENCES Sales(Id) ON DELETE CASCADE,
    FOREIGN KEY (ItemId) REFERENCES Items(Id) ON DELETE RESTRICT
);

CREATE INDEX IF NOT EXISTS IX_SaleItems_SaleId ON SaleItems(SaleId);
CREATE INDEX IF NOT EXISTS IX_SaleItems_ItemId ON SaleItems(ItemId);

-- ============================================
-- Table: PaymentDues
-- ============================================
CREATE TABLE IF NOT EXISTS PaymentDues (
    Id TEXT PRIMARY KEY,
    SaleId TEXT NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    DueDate TEXT NOT NULL,
    Status TEXT NOT NULL DEFAULT 'pending',
    FOREIGN KEY (SaleId) REFERENCES Sales(Id) ON DELETE RESTRICT
);

CREATE INDEX IF NOT EXISTS IX_PaymentDues_SaleId ON PaymentDues(SaleId);
CREATE INDEX IF NOT EXISTS IX_PaymentDues_Status ON PaymentDues(Status);
CREATE INDEX IF NOT EXISTS IX_PaymentDues_DueDate ON PaymentDues(DueDate);

-- ============================================
-- Table: ShopSettings
-- ============================================
CREATE TABLE IF NOT EXISTS ShopSettings (
    Id TEXT PRIMARY KEY,
    ShopName TEXT NOT NULL DEFAULT 'My Shop',
    Logo TEXT,
    Address TEXT NOT NULL,
    City TEXT NOT NULL,
    State TEXT NOT NULL,
    ZipCode TEXT NOT NULL,
    Country TEXT NOT NULL DEFAULT 'Sri Lanka',
    Phone TEXT NOT NULL,
    Email TEXT NOT NULL,
    TaxRate DECIMAL(5,2) NOT NULL DEFAULT 0,
    RoundingRule TEXT NOT NULL DEFAULT 'none',
    ReceiptTemplate TEXT NOT NULL DEFAULT 'default',
    PrinterName TEXT,
    CurrencySymbol TEXT NOT NULL DEFAULT 'Rs.',
    CurrencyCode TEXT NOT NULL DEFAULT 'LKR',
    CurrencyPosition TEXT NOT NULL DEFAULT 'before',
    POSDevices TEXT NOT NULL DEFAULT '[]'
);

-- ============================================
-- Seed Data: Categories
-- ============================================
INSERT OR IGNORE INTO Categories (Id, Name, Description, CreatedAt, UpdatedAt) VALUES
('1', 'Men''s Clothing', 'All men''s apparel', datetime('now'), datetime('now')),
('2', 'Women''s Clothing', 'All women''s apparel', datetime('now'), datetime('now')),
('3', 'Accessories', 'Bags, belts, watches', datetime('now'), datetime('now')),
('4', 'Shoes', 'Footwear for all', datetime('now'), datetime('now'));

-- ============================================
-- Seed Data: ShopSettings
-- ============================================
INSERT OR IGNORE INTO ShopSettings (
    Id, ShopName, Address, City, State, ZipCode, Country, 
    Phone, Email, TaxRate, RoundingRule, ReceiptTemplate,
    CurrencySymbol, CurrencyCode, CurrencyPosition, POSDevices
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


