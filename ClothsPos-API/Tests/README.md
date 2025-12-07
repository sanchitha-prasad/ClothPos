# API Test Script

Comprehensive test script to verify all ClothPos API endpoints with test data and stock calculations.

## Prerequisites

1. **Node.js** installed (v14 or higher)
2. **API running** on `https://localhost:5001` (or set `API_URL` environment variable)
3. **Admin user** exists with credentials:
   - Username: `admin-dev`
   - Password: `12345`

## Installation

```bash
cd ClothsPos-API/Tests
npm install axios
```

## Running Tests

### Quick Start (Windows PowerShell)

```powershell
cd ClothsPos-API/Tests
.\run-tests.ps1
```

This script will:
- ✅ Check if Node.js is installed
- ✅ Install dependencies automatically
- ✅ Check if API is running
- ✅ Run all tests

### Basic Usage

```bash
# Make sure API is running first
cd ClothsPos-API
dotnet run

# In another terminal, run tests
cd Tests
npm install axios
node ApiTestScript.js
```

### Custom API URL

```bash
# Windows PowerShell
$env:API_URL="http://localhost:5000"; node ApiTestScript.js

# Linux/Mac
API_URL=http://localhost:5000 node ApiTestScript.js
```

### Default Configuration

The script now defaults to:
- **HTTP on port 5000** (most common for development)
- Automatically tries alternative URLs if connection fails:
  - `http://localhost:5000`
  - `http://127.0.0.1:5000`
  - `https://localhost:5001`
  - `https://127.0.0.1:5001`

## What Gets Tested

### ✅ Core Functionality
- **Authentication** - Login and token generation
- **Categories** - CRUD operations
- **Items** - Create, read, update items with stock
- **Stock Calculation** - Verify stock tracking
- **Sales** - Create sales and verify stock deduction
- **Refunds** - Test refunds and stock restoration
- **Void Sales** - Test voiding sales and stock restoration

### 📊 Reports & Analytics
- **Low Stock Alerts** - Items below minimum stock level
- **Sales Reports** - Daily, weekly, monthly reports

### 👥 User Management
- **Users** - User listing and management
- **Roles** - Role management
- **Settings** - Shop settings retrieval

## Test Data Created

The script creates test data for:
- **3 Categories** (T-Shirts, Jeans, Hoodies)
- **3 Items** with different stock levels
- **Sales** with multiple items
- **Stock calculations** verified at each step

## Expected Output

```
╔════════════════════════════════════════╗
║   ClothPos API Comprehensive Tests   ║
╚════════════════════════════════════════╝

=== Testing Authentication ===
✅ Login successful

=== Testing Categories API ===
✅ Category created
✅ Retrieved 1 categories

=== Testing Items API ===
✅ Item created: Blue Cotton T-Shirt
   Stock: 50, Price: 25.99 LKR
...

╔════════════════════════════════════════╗
║           TEST SUMMARY                  ║
╚════════════════════════════════════════╝
Total Tests: 11
Passed: 11
Failed: 0

Success Rate: 100.0%
🎉 All tests passed!
```

## Stock Calculation Verification

The script verifies:

1. **Initial Stock** - Stock levels when items are created
2. **After Sale** - Stock is correctly deducted
   - Example: Item with 50 stock, selling 5 → stock becomes 45
3. **After Refund** - Stock is correctly restored
   - Example: Refunding 5 items → stock returns to 50

## Troubleshooting

### "Login failed"
- Ensure admin user exists in database
- Check username/password are correct
- Verify API is running

### "ECONNREFUSED" or "Connection refused"
- Ensure API is running on the correct port
- Check firewall settings
- Verify SSL certificate (for HTTPS)

### "Stock calculation incorrect"
- Check database connection
- Verify SaleService.CreateSaleAsync is working
- Check for concurrent transactions

## Manual Testing

You can also test individual endpoints using:

### Postman
Import the API endpoints from Swagger:
```
https://localhost:5001/swagger
```

### cURL Examples

```bash
# Login
curl -k -X POST https://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin-dev","password":"12345"}'

# Get Items (with token)
curl -k -X GET https://localhost:5001/api/items \
  -H "Authorization: Bearer YOUR_TOKEN"

# Create Sale
curl -k -X POST https://localhost:5001/api/sales \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "date": "2024-01-15T10:00:00Z",
    "subtotal": 100.00,
    "tax": 0,
    "total": 100.00,
    "paymentMethod": "cash",
    "status": "completed",
    "customerName": "Test Customer",
    "cashierId": "CASHIER_ID",
    "saleItems": [
      {
        "itemId": "ITEM_ID",
        "quantity": 2,
        "price": 50.00,
        "total": 100.00
      }
    ]
  }'
```

## Notes

- Test data is created in your database
- Stock calculations are verified at each step
- All tests are independent and can be run multiple times
- The script uses colored output for better readability

