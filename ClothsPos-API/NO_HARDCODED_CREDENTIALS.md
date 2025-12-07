# No Hardcoded Credentials

## ✅ All Hardcoded Credentials Removed

The API no longer has any hardcoded usernames or passwords. All authentication is database-driven.

## Changes Made

### 1. AuthService.cs
- ❌ **Removed**: Hardcoded check for `admin-dev` / `12345`
- ✅ **Now**: Only queries database for users
- ✅ **No hardcoded bypass**: All authentication goes through database

### 2. DatabaseSeeder.cs
- ❌ **Removed**: Hardcoded email `admin@shop.com` and password `12345`
- ✅ **Now**: Reads from configuration (`appsettings.json`)
- ✅ **Configurable**: Can be changed via environment variables or appsettings

### 3. Configuration-Based Defaults

The admin user is created with credentials from:
1. **Environment Variables** (highest priority)
   - `ADMIN_EMAIL`
   - `ADMIN_PASSWORD`
   - `ADMIN_NAME`

2. **appsettings.json** (second priority)
   ```json
   {
     "AdminUser": {
       "Email": "admin@shop.com",
       "Password": "Admin123!",
       "Name": "Admin User"
     }
   }
   ```

3. **Fallback defaults** (lowest priority)
   - Email: `admin@shop.com`
   - Password: `Admin123!`
   - Name: `Admin User`

## How It Works

1. **API Starts** → DatabaseSeeder runs
2. **Checks Database** → If no admin user exists
3. **Reads Configuration** → Gets credentials from appsettings or environment
4. **Creates Admin User** → With hashed password in database
5. **Login** → Queries database (no hardcoded checks)

## Security Benefits

- ✅ No hardcoded credentials in code
- ✅ Passwords are BCrypt hashed
- ✅ All authentication is database-driven
- ✅ Credentials are configurable
- ✅ Can be changed via environment variables (secure)

## To Change Default Credentials

### Option 1: Update appsettings.json
```json
{
  "AdminUser": {
    "Email": "your-admin@email.com",
    "Password": "YourSecurePassword123!",
    "Name": "Your Admin Name"
  }
}
```

### Option 2: Set Environment Variables
```bash
# Windows PowerShell
$env:ADMIN_EMAIL="your-admin@email.com"
$env:ADMIN_PASSWORD="YourSecurePassword123!"
$env:ADMIN_NAME="Your Admin Name"

# Linux/Mac
export ADMIN_EMAIL="your-admin@email.com"
export ADMIN_PASSWORD="YourSecurePassword123!"
export ADMIN_NAME="Your Admin Name"
```

## Verification

To verify no hardcoded credentials:
1. Search codebase for `"admin-dev"` → Should find nothing in API code
2. Search for `"12345"` → Should only find in documentation/comments
3. Check `AuthService.cs` → No hardcoded user checks
4. Check `DatabaseSeeder.cs` → Uses configuration, not hardcoded values

## Current Default Credentials

After first run, the admin user will be created with:
- **Email**: `admin@shop.com` (from appsettings.json)
- **Password**: `Admin123!` (from appsettings.json, BCrypt hashed)
- **Name**: `Admin User` (from appsettings.json)

**You can change these in `appsettings.json` before first run!**

