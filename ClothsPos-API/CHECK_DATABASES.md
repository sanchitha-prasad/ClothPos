# How to Check Databases

## MySQL - Check Databases

### 1. Connect to MySQL

```bash
# Connect as root
mysql -u root -p

# Or connect as specific user
mysql -u clothpos_user -p
```

### 2. List All Databases

```sql
SHOW DATABASES;
```

### 3. Use a Specific Database

```sql
USE ClothPosDB;
```

### 4. Show Current Database

```sql
SELECT DATABASE();
```

### 5. List All Tables in Current Database

```sql
SHOW TABLES;
```

### 6. Show Table Structure

```sql
DESCRIBE Users;
-- or
SHOW COLUMNS FROM Users;
```

### 7. Show All Users

```sql
SELECT User, Host FROM mysql.user;
```

### 8. Check Database Size

```sql
SELECT 
    table_schema AS 'Database',
    ROUND(SUM(data_length + index_length) / 1024 / 1024, 2) AS 'Size (MB)'
FROM information_schema.tables
WHERE table_schema = 'ClothPosDB'
GROUP BY table_schema;
```

### 9. Check Table Sizes

```sql
SELECT 
    table_name AS 'Table',
    ROUND(((data_length + index_length) / 1024 / 1024), 2) AS 'Size (MB)'
FROM information_schema.tables
WHERE table_schema = 'ClothPosDB'
ORDER BY (data_length + index_length) DESC;
```

### 10. Quick One-Liner (from command line)

```bash
mysql -u root -p -e "SHOW DATABASES;"
```

---

## SQL Server - Check Databases

### 1. Connect to SQL Server

```bash
# Using sqlcmd
sqlcmd -S localhost\SQLEXPRESS -E

# Or with username/password
sqlcmd -S localhost\SQLEXPRESS -U your_username -P your_password
```

### 2. List All Databases

```sql
SELECT name FROM sys.databases;
GO
```

### 3. Use a Specific Database

```sql
USE ClothPosDB;
GO
```

### 4. Show Current Database

```sql
SELECT DB_NAME() AS CurrentDatabase;
GO
```

### 5. List All Tables in Current Database

```sql
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_TYPE = 'BASE TABLE';
GO
```

### 6. Show Table Structure

```sql
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Users';
GO
```

### 7. Check Database Size

```sql
SELECT 
    name AS DatabaseName,
    size * 8 / 1024 AS SizeMB
FROM sys.master_files
WHERE database_id = DB_ID('ClothPosDB');
GO
```

### 8. Quick One-Liner (from command line)

```bash
sqlcmd -S localhost\SQLEXPRESS -E -Q "SELECT name FROM sys.databases"
```

---

## Using SQL Server Management Studio (SSMS)

If you have SSMS installed:

1. **Open SQL Server Management Studio**
2. **Connect** to your server (localhost\SQLEXPRESS)
3. **Object Explorer** → Expand "Databases" to see all databases
4. **Right-click** on a database → Properties → See size and details

---

## Using MySQL Workbench

If you have MySQL Workbench installed:

1. **Open MySQL Workbench**
2. **Connect** to your MySQL server
3. **Navigator** → Expand "SCHEMAS" to see all databases
4. **Click** on a database to see tables

---

## Quick Commands Reference

### MySQL
```bash
# List databases
mysql -u root -p -e "SHOW DATABASES;"

# List tables in a database
mysql -u root -p -e "USE ClothPosDB; SHOW TABLES;"

# Check if database exists
mysql -u root -p -e "SHOW DATABASES LIKE 'ClothPosDB';"
```

### SQL Server
```bash
# List databases
sqlcmd -S localhost\SQLEXPRESS -E -Q "SELECT name FROM sys.databases"

# List tables
sqlcmd -S localhost\SQLEXPRESS -E -d ClothPosDB -Q "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'"
```

---

## Check from Your Application

You can also check databases programmatically from your .NET application:

### Check MySQL Connection

```csharp
// In Program.cs or a test endpoint
using (var connection = new MySqlConnection(connectionString))
{
    connection.Open();
    var command = new MySqlCommand("SHOW DATABASES", connection);
    var reader = command.ExecuteReader();
    while (reader.Read())
    {
        Console.WriteLine(reader.GetString(0));
    }
}
```

### Check SQL Server Connection

```csharp
// In Program.cs or a test endpoint
using (var connection = new SqlConnection(connectionString))
{
    connection.Open();
    var command = new SqlCommand("SELECT name FROM sys.databases", connection);
    var reader = command.ExecuteReader();
    while (reader.Read())
    {
        Console.WriteLine(reader.GetString(0));
    }
}
```

---

## Common Queries

### Check if Database Exists (MySQL)

```sql
SELECT SCHEMA_NAME 
FROM INFORMATION_SCHEMA.SCHEMATA 
WHERE SCHEMA_NAME = 'ClothPosDB';
```

### Check if Database Exists (SQL Server)

```sql
IF DB_ID('ClothPosDB') IS NOT NULL
    SELECT 'Database exists'
ELSE
    SELECT 'Database does not exist';
GO
```

### Count Tables in Database (MySQL)

```sql
SELECT COUNT(*) AS TableCount
FROM information_schema.tables
WHERE table_schema = 'ClothPosDB';
```

### Count Tables in Database (SQL Server)

```sql
SELECT COUNT(*) AS TableCount
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE';
GO
```

