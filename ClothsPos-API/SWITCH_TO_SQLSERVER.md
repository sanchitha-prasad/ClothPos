# Switching to SQL Server (Supports TOP Syntax)

SQL Server uses `SELECT TOP 1` syntax which is already supported in the code. This guide shows how to configure it.

## Quick Switch

### Option 1: Update appsettings.json (Recommended)

Edit `/var/www/clothpos-api/ClothPos/ClothsPos-API/publish/appsettings.json` on your server:

```json
{
  "UseMySql": false,
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=localhost;Initial Catalog=ClothPosDB;Integrated Security=True;TrustServerCertificate=True;"
  }
}
```

**For SQL Server with username/password:**
```json
{
  "UseMySql": false,
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=localhost;Initial Catalog=ClothPosDB;User Id=sa;Password=YourPassword;TrustServerCertificate=True;"
  }
}
```

**For remote SQL Server:**
```json
{
  "UseMySql": false,
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=YOUR_SERVER_IP,1433;Initial Catalog=ClothPosDB;User Id=sa;Password=YourPassword;TrustServerCertificate=True;"
  }
}
```

### Option 2: Use Environment Variables

In `/etc/systemd/system/clothpos-api.service`, add:

```ini
[Service]
Environment=UseMySql=false
Environment=ConnectionStrings__DefaultConnection=Data Source=localhost;Initial Catalog=ClothPosDB;Integrated Security=True;TrustServerCertificate=True;
```

## SQL Server Connection String Formats

### Windows Authentication (Local)
```
Data Source=localhost;Initial Catalog=ClothPosDB;Integrated Security=True;TrustServerCertificate=True;
```

### SQL Server Authentication
```
Data Source=localhost;Initial Catalog=ClothPosDB;User Id=sa;Password=YourPassword;TrustServerCertificate=True;
```

### Remote SQL Server
```
Data Source=YOUR_SERVER_IP,1433;Initial Catalog=ClothPosDB;User Id=sa;Password=YourPassword;TrustServerCertificate=True;
```

### SQL Server Express
```
Data Source=localhost\SQLEXPRESS;Initial Catalog=ClothPosDB;Integrated Security=True;TrustServerCertificate=True;
```

## After Changing Configuration

1. **Restart the service:**
   ```bash
   sudo systemctl restart clothpos-api
   ```

2. **Check status:**
   ```bash
   sudo systemctl status clothpos-api
   ```

3. **View logs:**
   ```bash
   sudo journalctl -u clothpos-api -f
   ```

## Benefits of SQL Server

- ✅ Supports `SELECT TOP` syntax (no need for LIMIT)
- ✅ Better integration with .NET
- ✅ More features (stored procedures, functions, etc.)
- ✅ Better performance for complex queries
- ✅ Native Windows Authentication support

## Switching Back to MySQL

If you need to switch back to MySQL:

```json
{
  "UseMySql": true,
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ClothPosDB;User=clothpos_user;Password=YourPassword;Port=3306;SslMode=None;"
  }
}
```

The code automatically detects which database to use based on:
1. `UseMySql` setting (true/false)
2. Connection string format (MySQL uses `Server=`, SQL Server uses `Data Source=`)

## Database Setup

### SQL Server Setup

1. **Install SQL Server** (if not already installed):
   ```bash
   # For Ubuntu/Debian, SQL Server is available but requires specific setup
   # See: https://learn.microsoft.com/en-us/sql/linux/sql-server-linux-setup
   ```

2. **Create Database:**
   ```sql
   CREATE DATABASE ClothPosDB;
   ```

3. **The API will automatically create tables on first run**

### MySQL Setup

If you want to use MySQL instead, see `DIGITALOCEAN_UBUNTU_SETUP.md` for MySQL setup instructions.

## Code Support

The code already supports both databases:
- **SQL Server**: Uses `SELECT TOP 1` syntax
- **MySQL**: Uses `SELECT ... LIMIT 1` syntax

The code automatically detects which database you're using and uses the correct syntax.

