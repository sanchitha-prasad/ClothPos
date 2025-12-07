# How to Change Database User

## Current Setup

You're currently using **SQL Server with Windows Authentication** (Integrated Security), which uses your Windows account.

## Option 1: Switch to SQL Server Authentication (Username/Password)

### Step 1: Create SQL Server User

Connect to SQL Server as administrator and run:

```sql
-- Connect to SQL Server
sqlcmd -S localhost\SQLEXPRESS -E

-- Create a new login
CREATE LOGIN clothpos_user WITH PASSWORD = 'Cl0thP0s@2024#Secure!';

-- Create user in the database
USE ClothPosDB;
GO
CREATE USER clothpos_user FOR LOGIN clothpos_user;
GO

-- Grant permissions
ALTER ROLE db_owner ADD MEMBER clothpos_user;
GO
```

### Step 2: Update appsettings.json

Change the connection string from Windows Authentication to SQL Server Authentication:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=localhost\\SQLEXPRESS;Initial Catalog=ClothPosDB;User Id=clothpos_user;Password=Cl0thP0s@2024#Secure!;TrustServerCertificate=True;"
  }
}
```

**Key changes:**
- Remove: `Integrated Security=True`
- Add: `User Id=clothpos_user;Password=Cl0thP0s@2024#Secure!`

---

## Option 2: Switch to MySQL with Specific User

### Step 1: Create MySQL User (if not exists)

```bash
mysql -u root -p
```

```sql
-- Create database
CREATE DATABASE IF NOT EXISTS ClothPosDB CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

-- Create user
CREATE USER IF NOT EXISTS 'clothpos_user'@'localhost' IDENTIFIED BY 'Cl0thP0s@2024#Secure!';
CREATE USER IF NOT EXISTS 'clothpos_user'@'%' IDENTIFIED BY 'Cl0thP0s@2024#Secure!';

-- Grant privileges
GRANT ALL PRIVILEGES ON ClothPosDB.* TO 'clothpos_user'@'localhost';
GRANT ALL PRIVILEGES ON ClothPosDB.* TO 'clothpos_user'@'%';
FLUSH PRIVILEGES;
```

### Step 2: Update appsettings.json for MySQL

```json
{
  "UseMySql": true,
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ClothPosDB;User=clothpos_user;Password=Cl0thP0s@2024#Secure!;Port=3306;"
  }
}
```

---

## Option 3: Use Different SQL Server User

If you want to use a different existing SQL Server user:

### Step 1: Check existing users

```sql
SELECT name FROM sys.sql_logins;
GO
```

### Step 2: Update appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=localhost\\SQLEXPRESS;Initial Catalog=ClothPosDB;User Id=your_username;Password=your_password;TrustServerCertificate=True;"
  }
}
```

---

## Option 4: Use Different MySQL User

If you want to use a different MySQL user:

### Step 1: List existing users

```sql
SELECT User, Host FROM mysql.user;
```

### Step 2: Update appsettings.json

```json
{
  "UseMySql": true,
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ClothPosDB;User=your_mysql_user;Password=your_password;Port=3306;"
  }
}
```

---

## Security Best Practices

### 1. Use Environment Variables (Recommended for Production)

Instead of storing passwords in `appsettings.json`, use environment variables:

**Windows (PowerShell):**
```powershell
$env:ConnectionStrings__DefaultConnection = "Data Source=localhost\SQLEXPRESS;Initial Catalog=ClothPosDB;User Id=clothpos_user;Password=Cl0thP0s@2024#Secure!;TrustServerCertificate=True;"
```

**Linux/Mac:**
```bash
export ConnectionStrings__DefaultConnection="Server=localhost;Database=ClothPosDB;User=clothpos_user;Password=Cl0thP0s@2024#Secure!;Port=3306;"
```

### 2. Use appsettings.Production.json

Create `appsettings.Production.json` (already in .gitignore):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Your production connection string here"
  }
}
```

### 3. Use Azure Key Vault or Similar

For production, consider using Azure Key Vault, AWS Secrets Manager, or similar services.

---

## Testing the Connection

After changing the user, test the connection:

### SQL Server:
```bash
sqlcmd -S localhost\SQLEXPRESS -U clothpos_user -P Cl0thP0s@2024#Secure! -d ClothPosDB -Q "SELECT @@VERSION"
```

### MySQL:
```bash
mysql -u clothpos_user -p ClothPosDB
# Enter password: Cl0thP0s@2024#Secure!
```

---

## Troubleshooting

### SQL Server: "Login failed for user"

**Solution:**
1. Make sure SQL Server Authentication is enabled:
   - Open SQL Server Management Studio
   - Right-click server → Properties → Security
   - Enable "SQL Server and Windows Authentication mode"
   - Restart SQL Server service

2. Verify user exists:
   ```sql
   SELECT name FROM sys.sql_logins WHERE name = 'clothpos_user';
   ```

### MySQL: "Access denied"

**Solution:**
1. Verify user exists:
   ```sql
   SELECT User, Host FROM mysql.user WHERE User = 'clothpos_user';
   ```

2. Check permissions:
   ```sql
   SHOW GRANTS FOR 'clothpos_user'@'localhost';
   ```

3. Reset password if needed:
   ```sql
   ALTER USER 'clothpos_user'@'localhost' IDENTIFIED BY 'Cl0thP0s@2024#Secure!';
   FLUSH PRIVILEGES;
   ```

---

## Quick Reference

### SQL Server Connection String Format:
```
Data Source=server;Initial Catalog=database;User Id=username;Password=password;TrustServerCertificate=True;
```

### MySQL Connection String Format:
```
Server=host;Database=database;User=username;Password=password;Port=3306;
```


