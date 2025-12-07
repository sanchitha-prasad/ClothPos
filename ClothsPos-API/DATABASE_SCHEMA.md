# Database Schema Documentation

## Overview

This document describes the complete database schema for the ClothPos API. The database uses SQLite by default but can be migrated to SQL Server.

## Database: ClothPos.db (SQLite)

---

## Table: Users

**Description**: Stores shop staff and admin users

| Column Name | Data Type | Constraints | Description |
|------------|-----------|-------------|-------------|
| Id | TEXT (GUID) | PRIMARY KEY | Unique identifier |
| Name | TEXT(200) | NOT NULL | User's full name |
| Email | TEXT(200) | NOT NULL, UNIQUE | Email address (unique) |
| Role | INTEGER | NOT NULL | User role (0=Admin, 1=Cashier, 2=MobileStaff) |
| Passcode | TEXT | NULL | Hashed password (BCrypt for Admin, plain for others) |
| Permissions | TEXT | NOT NULL | JSON array of permission strings |
| IsActive | INTEGER (BOOL) | NOT NULL, DEFAULT 1 | Active status |
| CreatedAt | TEXT (DateTime) | NOT NULL | Creation timestamp |
| UpdatedAt | TEXT (DateTime) | NOT NULL | Last update timestamp |

**Indexes:**
- PRIMARY KEY: Id
- UNIQUE INDEX: Email

**Relationships:**
- One-to-Many: Users → Sales (CashierId)

---

## Table: Categories

**Description**: Product categories

| Column Name | Data Type | Constraints | Description |
|------------|-----------|-------------|-------------|
| Id | TEXT (GUID) | PRIMARY KEY | Unique identifier |
| Name | TEXT(100) | NOT NULL, UNIQUE | Category name |
| Description | TEXT(500) | NULL | Category description |
| CreatedAt | TEXT (DateTime) | NOT NULL | Creation timestamp |
| UpdatedAt | TEXT (DateTime) | NOT NULL | Last update timestamp |

**Indexes:**
- PRIMARY KEY: Id
- UNIQUE INDEX: Name

**Relationships:**
- One-to-Many: Categories → Items (CategoryId)

---

## Table: Items

**Description**: Product/Item inventory

| Column Name | Data Type | Constraints | Description |
|------------|-----------|-------------|-------------|
| Id | TEXT (GUID) | PRIMARY KEY | Unique identifier |
| Name | TEXT(200) | NOT NULL | Product name |
| Description | TEXT(1000) | NULL | Product description |
| CategoryId | TEXT (GUID) | NOT NULL, FOREIGN KEY | Reference to Categories.Id |
| Price | DECIMAL(18,2) | NOT NULL | Selling price |
| Cost | DECIMAL(18,2) | NOT NULL, DEFAULT 0 | Cost price |
| Stock | DECIMAL(18,2) | NOT NULL, DEFAULT 0 | Current stock quantity |
| MinStockLevel | DECIMAL(18,2) | NOT NULL, DEFAULT 10 | Minimum stock threshold |
| Barcode | TEXT(50) | NULL | Barcode number |
| SKU | TEXT(50) | NOT NULL, UNIQUE | Stock Keeping Unit |
| Code | TEXT(50) | NULL | Product code |
| Brand | TEXT(100) | NULL | Brand name |
| ProductUnit | TEXT(50) | NOT NULL, DEFAULT 'piece' | Product unit |
| SaleUnit | TEXT(50) | NOT NULL, DEFAULT 'piece' | Sale unit |
| PurchaseUnit | TEXT(50) | NOT NULL, DEFAULT 'piece' | Purchase unit |
| Images | TEXT | NOT NULL, DEFAULT '[]' | JSON array of image URLs |
| Sizes | TEXT | NOT NULL, DEFAULT '[]' | JSON array of Size objects |
| Colors | TEXT | NOT NULL, DEFAULT '[]' | JSON array of Color objects |
| Note | TEXT | NULL | Additional notes |
| IsActive | INTEGER (BOOL) | NOT NULL, DEFAULT 1 | Active status |
| CreatedAt | TEXT (DateTime) | NOT NULL | Creation timestamp |
| UpdatedAt | TEXT (DateTime) | NOT NULL | Last update timestamp |

**Indexes:**
- PRIMARY KEY: Id
- UNIQUE INDEX: SKU
- INDEX: Code
- FOREIGN KEY: CategoryId → Categories.Id (RESTRICT on delete)

**Relationships:**
- Many-to-One: Items → Categories (CategoryId)
- One-to-Many: Items → SaleItems (ItemId)

---

## Table: Sales

**Description**: Sales transactions

| Column Name | Data Type | Constraints | Description |
|------------|-----------|-------------|-------------|
| Id | TEXT (GUID) | PRIMARY KEY | Unique identifier |
| Date | TEXT (DateTime) | NOT NULL | Sale date and time |
| Total | DECIMAL(18,2) | NOT NULL | Total amount |
| Tax | DECIMAL(18,2) | NOT NULL, DEFAULT 0 | Tax amount |
| Subtotal | DECIMAL(18,2) | NOT NULL | Subtotal before tax |
| PaymentMethod | TEXT(50) | NOT NULL, DEFAULT 'cash' | Payment method |
| Status | TEXT(20) | NOT NULL, DEFAULT 'completed' | Status (completed, pending, refunded, voided) |
| CustomerName | TEXT(200) | NULL | Customer name |
| CashierId | TEXT (GUID) | NOT NULL, FOREIGN KEY | Reference to Users.Id |

**Indexes:**
- PRIMARY KEY: Id
- INDEX: Date
- INDEX: Status
- FOREIGN KEY: CashierId → Users.Id (RESTRICT on delete)

**Relationships:**
- Many-to-One: Sales → Users (CashierId)
- One-to-Many: Sales → SaleItems (SaleId)
- One-to-Many: Sales → PaymentDues (SaleId)

---

## Table: SaleItems

**Description**: Individual items in a sale

| Column Name | Data Type | Constraints | Description |
|------------|-----------|-------------|-------------|
| Id | TEXT (GUID) | PRIMARY KEY | Unique identifier |
| SaleId | TEXT (GUID) | NOT NULL, FOREIGN KEY | Reference to Sales.Id |
| ItemId | TEXT (GUID) | NOT NULL, FOREIGN KEY | Reference to Items.Id |
| Quantity | DECIMAL(18,2) | NOT NULL | Quantity sold |
| Price | DECIMAL(18,2) | NOT NULL | Unit price at time of sale |
| Total | DECIMAL(18,2) | NOT NULL | Line total (Quantity × Price) |

**Indexes:**
- PRIMARY KEY: Id
- FOREIGN KEY: SaleId → Sales.Id (CASCADE on delete)
- FOREIGN KEY: ItemId → Items.Id (RESTRICT on delete)

**Relationships:**
- Many-to-One: SaleItems → Sales (SaleId)
- Many-to-One: SaleItems → Items (ItemId)

---

## Table: PaymentDues

**Description**: Pending payments and dues

| Column Name | Data Type | Constraints | Description |
|------------|-----------|-------------|-------------|
| Id | TEXT (GUID) | PRIMARY KEY | Unique identifier |
| SaleId | TEXT (GUID) | NOT NULL, FOREIGN KEY | Reference to Sales.Id |
| Amount | DECIMAL(18,2) | NOT NULL | Due amount |
| DueDate | TEXT (DateTime) | NOT NULL | Due date |
| Status | TEXT(20) | NOT NULL, DEFAULT 'pending' | Status (pending, paid, overdue) |

**Indexes:**
- PRIMARY KEY: Id
- INDEX: Status
- INDEX: DueDate
- FOREIGN KEY: SaleId → Sales.Id (RESTRICT on delete)

**Relationships:**
- Many-to-One: PaymentDues → Sales (SaleId)

---

## Table: ShopSettings

**Description**: Shop configuration and settings

| Column Name | Data Type | Constraints | Description |
|------------|-----------|-------------|-------------|
| Id | TEXT (GUID) | PRIMARY KEY | Unique identifier |
| ShopName | TEXT(200) | NOT NULL, DEFAULT 'My Shop' | Shop name |
| Logo | TEXT | NULL | Logo image URL |
| Address | TEXT(500) | NOT NULL | Street address |
| City | TEXT(100) | NOT NULL | City |
| State | TEXT(100) | NOT NULL | State/Province |
| ZipCode | TEXT(20) | NOT NULL | ZIP/Postal code |
| Country | TEXT(100) | NOT NULL, DEFAULT 'Sri Lanka' | Country |
| Phone | TEXT(20) | NOT NULL | Phone number |
| Email | TEXT(200) | NOT NULL | Contact email |
| TaxRate | DECIMAL(5,2) | NOT NULL, DEFAULT 0 | Tax rate percentage |
| RoundingRule | TEXT(20) | NOT NULL, DEFAULT 'none' | Rounding rule (none, round, ceil, floor) |
| ReceiptTemplate | TEXT | NOT NULL, DEFAULT 'default' | Receipt template |
| PrinterName | TEXT(100) | NULL | Printer name |
| CurrencySymbol | TEXT(10) | NOT NULL, DEFAULT 'Rs.' | Currency symbol |
| CurrencyCode | TEXT(3) | NOT NULL, DEFAULT 'LKR' | Currency code (ISO) |
| CurrencyPosition | TEXT(10) | NOT NULL, DEFAULT 'before' | Currency position (before, after) |
| POSDevices | TEXT | NOT NULL, DEFAULT '[]' | JSON array of POS device objects |

**Indexes:**
- PRIMARY KEY: Id

**Relationships:**
- None (Singleton table - typically only one record)

---

## Entity Relationship Diagram (ERD)

```
┌─────────────┐
│   Users     │
│─────────────│
│ Id (PK)     │
│ Name        │
│ Email (UK)  │
│ Role        │
│ Passcode    │
│ ...         │
└──────┬──────┘
       │
       │ 1
       │
       │ N
┌──────▼──────┐
│   Sales     │
│─────────────│
│ Id (PK)     │
│ CashierId   │──FK──┐
│ Date        │      │
│ Total       │      │
│ Status      │      │
│ ...         │      │
└──────┬──────┘      │
       │             │
       │ 1           │
       │             │
       │ N           │
┌──────▼──────┐      │
│ SaleItems   │      │
│─────────────│      │
│ Id (PK)     │      │
│ SaleId (FK) │──────┘
│ ItemId (FK) │──┐
│ Quantity    │  │
│ Price       │  │
│ Total       │  │
└─────────────┘  │
                 │
                 │ N
                 │
                 │ 1
        ┌────────▼──────┐
        │    Items      │
        │───────────────│
        │ Id (PK)       │
        │ CategoryId(FK)│──┐
        │ SKU (UK)      │  │
        │ Name          │  │
        │ Price         │  │
        │ Stock         │  │
        │ ...           │  │
        └───────────────┘  │
                           │
                           │ N
                           │
                           │ 1
                  ┌────────▼──────┐
                  │  Categories   │
                  │───────────────│
                  │ Id (PK)       │
                  │ Name (UK)     │
                  │ Description   │
                  │ ...           │
                  └───────────────┘

┌─────────────┐
│   Sales     │
└──────┬──────┘
       │
       │ 1
       │
       │ N
┌──────▼──────────┐
│ PaymentDues     │
│─────────────────│
│ Id (PK)         │
│ SaleId (FK)     │
│ Amount          │
│ DueDate         │
│ Status          │
└─────────────────┘
```

---

## Data Types Mapping

### SQLite to .NET Types

| SQLite Type | .NET Type | Notes |
|-------------|-----------|-------|
| TEXT | string | Used for GUIDs, strings, JSON |
| INTEGER | bool, int, enum | Boolean (0/1), integers, enums |
| DECIMAL(18,2) | decimal | Decimal numbers with 2 decimal places |
| TEXT (DateTime) | DateTime | Stored as ISO 8601 string |

---

## Default Values

### Users
- `IsActive`: `true`
- `CreatedAt`: Current UTC time
- `UpdatedAt`: Current UTC time

### Categories
- `CreatedAt`: Current UTC time
- `UpdatedAt`: Current UTC time

### Items
- `Cost`: `0`
- `Stock`: `0`
- `MinStockLevel`: `10`
- `ProductUnit`: `'piece'`
- `SaleUnit`: `'piece'`
- `PurchaseUnit`: `'piece'`
- `Images`: `'[]'` (empty JSON array)
- `Sizes`: `'[]'` (empty JSON array)
- `Colors`: `'[]'` (empty JSON array)
- `IsActive`: `true`
- `CreatedAt`: Current UTC time
- `UpdatedAt`: Current UTC time

### Sales
- `Tax`: `0`
- `PaymentMethod`: `'cash'`
- `Status`: `'completed'`
- `Date`: Current UTC time

### PaymentDues
- `Status`: `'pending'`

### ShopSettings
- `ShopName`: `'My Shop'`
- `TaxRate`: `0`
- `RoundingRule`: `'none'`
- `ReceiptTemplate`: `'default'`
- `Country`: `'Sri Lanka'`
- `CurrencySymbol`: `'Rs.'`
- `CurrencyCode`: `'LKR'`
- `CurrencyPosition`: `'before'`
- `POSDevices`: `'[]'` (empty JSON array)

---

## Seed Data

### Initial Categories
1. Men's Clothing
2. Women's Clothing
3. Accessories
4. Shoes

### Initial Shop Settings
- Shop Name: "Cloths POS Shop"
- Currency: LKR (Rs.)
- Default settings for Sri Lankan market

---

## JSON Fields

### Users.Permissions
```json
["all"]
// or
["read", "write", "delete"]
```

### Items.Images
```json
["/uploads/items/{itemId}/image1.jpg", "/uploads/items/{itemId}/image2.jpg"]
```

### Items.Sizes
```json
[{"id": "1", "name": "M"}, {"id": "2", "name": "L"}]
```

### Items.Colors
```json
[{"id": "1", "name": "Blue", "hexCode": "#1976d2"}]
```

### ShopSettings.POSDevices
```json
[{"id": "1", "name": "Desktop POS", "type": "desktop", "isActive": true}]
```

---

## Foreign Key Constraints

1. **Items.CategoryId** → Categories.Id
   - Action: RESTRICT on delete
   - Prevents deleting categories with associated items

2. **Sales.CashierId** → Users.Id
   - Action: RESTRICT on delete
   - Prevents deleting users with sales records

3. **SaleItems.SaleId** → Sales.Id
   - Action: CASCADE on delete
   - Deletes sale items when sale is deleted

4. **SaleItems.ItemId** → Items.Id
   - Action: RESTRICT on delete
   - Prevents deleting items with sale history

5. **PaymentDues.SaleId** → Sales.Id
   - Action: RESTRICT on delete
   - Prevents deleting sales with pending payments

---

## Indexes

### Primary Keys
- All tables have `Id` as PRIMARY KEY

### Unique Indexes
- `Users.Email` - Unique email constraint
- `Categories.Name` - Unique category name
- `Items.SKU` - Unique SKU constraint

### Regular Indexes
- `Items.Code` - For faster code lookups
- `Items.CategoryId` - Foreign key index
- `Sales.Date` - For date range queries
- `Sales.Status` - For status filtering
- `PaymentDues.Status` - For status filtering
- `PaymentDues.DueDate` - For due date queries

---

## Migration to SQL Server

To migrate to SQL Server, change the following:

1. **Connection String** in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ClothsPosDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

2. **Program.cs**:
```csharp
options.UseSqlServer(connectionString); // Instead of UseSqlite
```

3. **Data Types**:
- TEXT → NVARCHAR(MAX) or appropriate size
- INTEGER (BOOL) → BIT
- DECIMAL(18,2) → DECIMAL(18,2) (same)
- TEXT (DateTime) → DATETIME2

---

## Sample Queries

### Get all items with category
```sql
SELECT i.*, c.Name as CategoryName 
FROM Items i 
LEFT JOIN Categories c ON i.CategoryId = c.Id
WHERE i.IsActive = 1;
```

### Get sales with cashier info
```sql
SELECT s.*, u.Name as CashierName 
FROM Sales s 
LEFT JOIN Users u ON s.CashierId = u.Id
ORDER BY s.Date DESC;
```

### Get low stock items
```sql
SELECT * FROM Items 
WHERE Stock <= MinStockLevel AND IsActive = 1;
```

### Get pending payments
```sql
SELECT pd.*, s.Date as SaleDate, s.Total as SaleTotal
FROM PaymentDues pd
LEFT JOIN Sales s ON pd.SaleId = s.Id
WHERE pd.Status = 'pending'
ORDER BY pd.DueDate;
```

---

## Notes

1. **GUIDs**: All IDs are stored as GUID strings (TEXT in SQLite)
2. **Timestamps**: All dates stored as UTC in ISO 8601 format
3. **JSON Fields**: Complex data stored as JSON strings for flexibility
4. **Soft Deletes**: Use `IsActive` flag instead of hard deletes where appropriate
5. **Decimal Precision**: All monetary values use DECIMAL(18,2) for accuracy


