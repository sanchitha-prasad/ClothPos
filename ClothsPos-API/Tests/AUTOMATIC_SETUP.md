# Automatic Test Setup - No Manual Steps Required

## ✅ Fully Automated Test Script

The test script (`ApiTestScript.js`) is **completely automatic** and handles all setup without any manual intervention.

## What Happens Automatically

### 1. **API Connection Detection**
- Automatically tries multiple URLs (`http://localhost:5000`, `https://localhost:5001`, etc.)
- Detects which URL the API is running on
- No manual configuration needed

### 2. **Roles Seeding Check**
- Automatically fetches roles (endpoint is public)
- If no roles found, waits for API to seed them
- Retries automatically after waiting
- No manual role creation needed

### 3. **Admin User Creation (Automatic)**
- Tries multiple credential combinations:
  - `admin-dev` / `12345` (test script default)
  - `admin@shop.com` / `Admin123!` (API seeder default)
  - Other common combinations
- If login fails, **automatically creates admin user** via bootstrap endpoint
- Uses `/api/auth/bootstrap` endpoint (no auth required)
- Creates user with correct role and permissions
- Retries login automatically after creation

### 4. **Test User Creation**
- After successful login, automatically creates additional test users:
  - `cashier-test` - Cashier role
  - `staff-test` - Staff role
- Only creates if they don't exist
- Handles duplicates gracefully

## Complete Flow (100% Automatic)

```
1. Test Script Starts
   ↓
2. Connects to API (auto-detects URL)
   ↓
3. Fetches Roles (public endpoint)
   ↓
4. Tries Login with Multiple Credentials
   ├─ Success → Continue with tests
   └─ Failure → Auto-create admin user
       ↓
5. Bootstrap Admin User (if needed)
   ├─ Creates roles if missing (waits for API seeder)
   └─ Creates admin user via bootstrap endpoint
       ↓
6. Retry Login (automatic)
   ↓
7. Continue with All Tests
   ├─ Categories
   ├─ Items
   ├─ Sales
   ├─ Stock Calculations
   ├─ Refunds
   └─ Reports
```

## No Manual Steps Required

### ❌ You DON'T Need To:
- Manually create admin user
- Manually seed roles
- Manually configure credentials
- Manually check database
- Manually run SQL scripts

### ✅ The Script Does Everything:
- ✅ Detects API connection
- ✅ Fetches/seeds roles automatically
- ✅ Creates admin user if missing
- ✅ Tries multiple credential combinations
- ✅ Handles all edge cases
- ✅ Continues with full test suite

## Usage

Simply run:
```bash
cd ClothsPos-API/Tests
node ApiTestScript.js
```

Or use the PowerShell helper:
```powershell
.\run-tests.ps1
```

## What If Something Fails?

The script provides clear error messages:

### If API Not Running:
```
❌ Connection refused. Make sure API is running on http://localhost:5000
   Run: cd ClothsPos-API && dotnet run
```

### If Roles Not Seeded:
```
⚠️  No roles found. API should seed them on startup.
   Waiting 3 seconds for API to seed roles...
```

### If User Creation Fails:
```
⚠️  Admin user already exists (bootstrap not needed)
   Trying alternative credentials...
```

## Bootstrap Endpoint

The script uses the `/api/auth/bootstrap` endpoint which:
- ✅ Works **without authentication**
- ✅ Creates the **first admin user** only
- ✅ Validates input (username, password length)
- ✅ Uses BCrypt password hashing
- ✅ Prevents duplicate admin creation
- ✅ Returns clear error messages

## Test Credentials

The script will try these credentials automatically:
1. `admin-dev` / `12345` (created by bootstrap if needed)
2. `admin@shop.com` / `Admin123!` (API seeder default)
3. `admin` / `12345` (alternative)

## Summary

**Everything is automatic!** Just:
1. Start your API: `dotnet run`
2. Run the test script: `node ApiTestScript.js`
3. Watch it work! 🎉

No manual database setup, no manual user creation, no manual configuration needed!

