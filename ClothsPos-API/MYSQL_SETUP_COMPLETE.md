# MySQL Setup - Complete Guide

Your application is now configured to use MySQL. This guide covers everything you need.

## ✅ Current Configuration

Your `appsettings.json` is set for MySQL:
```json
{
  "UseMySql": true,
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ClothPosDB;User=clothpos_user;Password=Cl0thP0s@2024#Secure!;Port=3306;SslMode=None;"
  }
}
```

## 🚀 Quick Setup on Ubuntu Server

### Step 1: Install MySQL (if not already installed)

```bash
sudo apt update
sudo apt install -y mysql-server
sudo mysql_secure_installation
```

### Step 2: Create Database and User

```bash
sudo mysql -u root -p
```

Then run these SQL commands:

```sql
-- Create database
CREATE DATABASE IF NOT EXISTS ClothPosDB CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

-- Create user
CREATE USER IF NOT EXISTS 'clothpos_user'@'localhost' IDENTIFIED BY 'Cl0thP0s@2024#Secure!';

-- Grant privileges
GRANT ALL PRIVILEGES ON ClothPosDB.* TO 'clothpos_user'@'localhost';

-- Apply changes
FLUSH PRIVILEGES;

-- Verify
SELECT User, Host FROM mysql.user WHERE User = 'clothpos_user';

-- Exit
EXIT;
```

### Step 3: Test Connection

```bash
mysql -u clothpos_user -p'Cl0thP0s@2024#Secure!' -h localhost ClothPosDB -e "SELECT 1;"
```

If this works, MySQL is ready!

## 📝 Update Configuration on Server

On your Ubuntu server, update the publish directory:

```bash
cd /var/www/clothpos-api/ClothPos/ClothsPos-API/publish

# Update appsettings.json
sudo nano appsettings.json
```

Make sure it has:
```json
{
  "UseMySql": true,
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ClothPosDB;User=clothpos_user;Password=Cl0thP0s@2024#Secure!;Port=3306;SslMode=None;"
  }
}
```

**Important:** Replace `Cl0thP0s@2024#Secure!` with your actual MySQL password if different.

## 🔄 Restart Service

```bash
sudo systemctl restart clothpos-api
sudo systemctl status clothpos-api
```

## ✅ What Happens on First Run

Entity Framework Core will:
1. ✅ Connect to MySQL
2. ✅ Create the database if it doesn't exist
3. ✅ Create all tables automatically
4. ✅ Seed initial data (roles, admin user)

**No manual schema scripts needed!** EF Core handles everything.

## 🔍 Verify It's Working

```bash
# Check service status
sudo systemctl status clothpos-api

# View logs
sudo journalctl -u clothpos-api -f

# Test API endpoint
curl http://localhost:5000/api/items
```

## 🔧 Connection String Options

### Local MySQL (Current)
```
Server=localhost;Database=ClothPosDB;User=clothpos_user;Password=YourPassword;Port=3306;SslMode=None;
```

### Remote MySQL
```
Server=YOUR_SERVER_IP;Database=ClothPosDB;User=clothpos_user;Password=YourPassword;Port=3306;SslMode=Required;
```

### With SSL
```
Server=localhost;Database=ClothPosDB;User=clothpos_user;Password=YourPassword;Port=3306;SslMode=Required;
```

## 🛠️ Troubleshooting

### Error: "Access denied for user"

**Solution:**
```bash
# Check if user exists
sudo mysql -u root -p -e "SELECT User, Host FROM mysql.user WHERE User = 'clothpos_user';"

# If user doesn't exist, create it (see Step 2 above)
```

### Error: "Unknown database 'ClothPosDB'"

**Solution:**
```bash
# Create database
sudo mysql -u root -p -e "CREATE DATABASE ClothPosDB CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;"
```

### Error: "Can't connect to MySQL server"

**Solution:**
```bash
# Check MySQL is running
sudo systemctl status mysql

# Start MySQL if not running
sudo systemctl start mysql

# Check if port 3306 is listening
sudo netstat -tuln | grep 3306
```

### Error: SQL syntax error with LIMIT

**Fixed!** The code now automatically uses `LIMIT 1` for MySQL and `TOP 1` for SQL Server. No action needed.

## 📊 Database Management

### View Tables
```bash
mysql -u clothpos_user -p'Cl0thP0s@2024#Secure!' ClothPosDB -e "SHOW TABLES;"
```

### View Users Table
```bash
mysql -u clothpos_user -p'Cl0thP0s@2024#Secure!' ClothPosDB -e "SELECT * FROM Users;"
```

### Backup Database
```bash
mysqldump -u clothpos_user -p'Cl0thP0s@2024#Secure!' ClothPosDB > backup_$(date +%Y%m%d).sql
```

### Restore Database
```bash
mysql -u clothpos_user -p'Cl0thP0s@2024#Secure!' ClothPosDB < backup_20241207.sql
```

## 🎯 Benefits of MySQL

- ✅ **Easy installation** - Simple `apt install`
- ✅ **Free and open source**
- ✅ **Well documented**
- ✅ **Good performance**
- ✅ **Works great with .NET**
- ✅ **Automatic schema creation** - EF Core handles it

## 📝 Environment Variables (Alternative)

Instead of editing files, you can use environment variables in the service file:

```bash
sudo nano /etc/systemd/system/clothpos-api.service
```

Add:
```ini
[Service]
Environment=UseMySql=true
Environment=ConnectionStrings__DefaultConnection=Server=localhost;Database=ClothPosDB;User=clothpos_user;Password=Cl0thP0s@2024#Secure!;Port=3306;SslMode=None;
```

Then:
```bash
sudo systemctl daemon-reload
sudo systemctl restart clothpos-api
```

## ✅ Next Steps

1. ✅ MySQL is configured in appsettings.json
2. ✅ Install MySQL on your server (if not already)
3. ✅ Create database and user
4. ✅ Update appsettings.json on server
5. ✅ Restart service
6. ✅ Test API

Your application is ready to use MySQL! 🎉

