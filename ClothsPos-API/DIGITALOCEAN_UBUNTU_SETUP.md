# DigitalOcean Ubuntu Server - MySQL Setup Guide

## Step 1: SSH into Your DigitalOcean Ubuntu Server

```bash
ssh root@your_server_ip
# or
ssh your_username@your_server_ip
```

## Step 2: Install MySQL (if not already installed)

```bash
# Update package list
sudo apt update

# Install MySQL server
sudo apt install mysql-server -y

# Secure MySQL installation
sudo mysql_secure_installation
```

During `mysql_secure_installation`, you'll be asked:
- **Password validation policy**: Enter `1` (MEDIUM)
- **Set root password**: Enter a strong password (or press Enter to skip if using auth_socket)
- **Remove anonymous users**: `Y`
- **Disallow root login remotely**: `Y` (recommended)
- **Remove test database**: `Y`
- **Reload privilege tables**: `Y`

## Step 3: Create MySQL Database and User

```bash
# Connect to MySQL as root
sudo mysql -u root -p
```

Then run these SQL commands:

```sql
-- Create database
CREATE DATABASE IF NOT EXISTS ClothPosDB CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

-- Create user for localhost (for API running on same server)
CREATE USER IF NOT EXISTS 'clothpos_user'@'localhost' IDENTIFIED BY 'Cl0thP0s@2024#Secure!';

-- Create user for remote connections (if API is on different server)
CREATE USER IF NOT EXISTS 'clothpos_user'@'%' IDENTIFIED BY 'Cl0thP0s@2024#Secure!';

-- Grant privileges
GRANT ALL PRIVILEGES ON ClothPosDB.* TO 'clothpos_user'@'localhost';
GRANT ALL PRIVILEGES ON ClothPosDB.* TO 'clothpos_user'@'%';

-- Apply changes
FLUSH PRIVILEGES;

-- Verify user was created
SELECT User, Host FROM mysql.user WHERE User = 'clothpos_user';

-- Exit MySQL
EXIT;
```

## Step 4: Configure MySQL to Allow Remote Connections (if needed)

If your API is running on a different server, you need to allow remote connections:

### 4.1: Edit MySQL Configuration

```bash
sudo nano /etc/mysql/mysql.conf.d/mysqld.cnf
```

Find the line:
```
bind-address = 127.0.0.1
```

Change it to:
```
bind-address = 0.0.0.0
```

Or comment it out:
```
# bind-address = 127.0.0.1
```

Save and exit (Ctrl+X, then Y, then Enter).

### 4.2: Restart MySQL

```bash
sudo systemctl restart mysql
```

### 4.3: Configure Firewall (UFW)

```bash
# Allow MySQL port (3306)
sudo ufw allow 3306/tcp

# Or allow from specific IP only (more secure)
sudo ufw allow from YOUR_API_SERVER_IP to any port 3306

# Check firewall status
sudo ufw status
```

### 4.4: Configure DigitalOcean Firewall (if enabled)

1. Go to DigitalOcean Dashboard
2. Click on your droplet
3. Go to "Networking" tab
4. Click "Edit Firewall Rules"
5. Add rule:
   - **Type**: Custom
   - **Protocol**: TCP
   - **Port Range**: 3306
   - **Sources**: Your API server IP (or allow all for testing)

## Step 5: Test MySQL Connection

### From the Ubuntu server itself:

```bash
mysql -u clothpos_user -p ClothPosDB
# Enter password: Cl0thP0s@2024#Secure!
```

### From your local machine (if remote access is enabled):

```bash
mysql -h YOUR_SERVER_IP -u clothpos_user -p ClothPosDB
# Enter password: Cl0thP0s@2024#Secure!
```

## Step 6: Update appsettings.json

Update your `appsettings.json` with your server's IP address:

```json
{
  "UseMySql": true,
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER_IP;Database=ClothPosDB;User=clothpos_user;Password=Cl0thP0s@2024#Secure!;Port=3306;SslMode=None;"
  }
}
```

**Replace `YOUR_SERVER_IP` with your actual DigitalOcean server IP address.**

### For SSL Connection (Recommended for Production):

If you want to use SSL (more secure):

```json
{
  "UseMySql": true,
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER_IP;Database=ClothPosDB;User=clothpos_user;Password=Cl0thP0s@2024#Secure!;Port=3306;SslMode=Required;"
  }
}
```

## Step 7: Use Environment Variables (Recommended for Production)

Instead of storing passwords in `appsettings.json`, use environment variables on your server:

```bash
# On your Ubuntu server
export UseMySql=true
export ConnectionStrings__DefaultConnection="Server=YOUR_SERVER_IP;Database=ClothPosDB;User=clothpos_user;Password=Cl0thP0s@2024#Secure!;Port=3306;SslMode=None;"
```

Or create a `.env` file or add to your systemd service file.

## Step 8: Verify MySQL Service is Running

```bash
# Check MySQL status
sudo systemctl status mysql

# Start MySQL if not running
sudo systemctl start mysql

# Enable MySQL to start on boot
sudo systemctl enable mysql
```

## Security Best Practices

### 1. Use Strong Passwords
- Use the password: `Cl0thP0s@2024#Secure!` or generate a new one
- Never use default passwords

### 2. Restrict Remote Access
- Only allow connections from your API server IP
- Use firewall rules to restrict access
- Consider using SSH tunnel for additional security

### 3. Use SSL (Optional but Recommended)
- Configure MySQL SSL certificates
- Use `SslMode=Required` in connection string

### 4. Regular Backups
```bash
# Create backup
mysqldump -u clothpos_user -p ClothPosDB > backup_$(date +%Y%m%d).sql

# Restore backup
mysql -u clothpos_user -p ClothPosDB < backup_20240101.sql
```

### 5. Monitor MySQL Logs
```bash
# View MySQL error log
sudo tail -f /var/log/mysql/error.log

# View MySQL general log
sudo tail -f /var/log/mysql/mysql.log
```

## Troubleshooting

### Issue: "Can't connect to MySQL server"

**Solutions:**
1. Check MySQL is running: `sudo systemctl status mysql`
2. Check MySQL is listening: `sudo netstat -tuln | grep 3306`
3. Check firewall: `sudo ufw status`
4. Check bind-address in `/etc/mysql/mysql.conf.d/mysqld.cnf`

### Issue: "Access denied for user"

**Solutions:**
1. Verify user exists: `SELECT User, Host FROM mysql.user WHERE User = 'clothpos_user';`
2. Check password is correct
3. Verify grants: `SHOW GRANTS FOR 'clothpos_user'@'localhost';`
4. Run `FLUSH PRIVILEGES;` after creating user

### Issue: "Unknown database"

**Solution:**
```sql
CREATE DATABASE ClothPosDB CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
```

### Issue: Connection timeout

**Solutions:**
1. Check firewall allows port 3306
2. Check DigitalOcean firewall rules
3. Verify MySQL bind-address is correct
4. Check if MySQL port is open: `telnet YOUR_SERVER_IP 3306`

## Quick Reference Commands

```bash
# Connect to MySQL
sudo mysql -u root -p
mysql -u clothpos_user -p ClothPosDB

# Check MySQL status
sudo systemctl status mysql
sudo systemctl restart mysql

# Check MySQL version
mysql --version

# List databases
mysql -u root -p -e "SHOW DATABASES;"

# List users
mysql -u root -p -e "SELECT User, Host FROM mysql.user;"

# Backup database
mysqldump -u clothpos_user -p ClothPosDB > backup.sql

# Restore database
mysql -u clothpos_user -p ClothPosDB < backup.sql
```

## Next Steps

1. ✅ MySQL installed and configured
2. ✅ Database and user created
3. ✅ Connection string updated in appsettings.json
4. ✅ Test connection from your API server
5. ✅ Deploy your API and test database connection

Your MySQL database is now ready to use with your ClothPos API! 🚀

